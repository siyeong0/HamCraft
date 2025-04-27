using Unity.Mathematics;
using UnityEngine;

using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HamCraft
{
	public class Fluid : MonoBehaviour
	{
		[Header("Simulation")]
		[SerializeField] int numParcels = 10000;
		[SerializeField] float interactionRadius = 1.0f;
		[SerializeField] float targetDensity = 1.0f;
		[SerializeField] float pressureCoefficient = 0.5f;
		[SerializeField] float nearPressureCoefficient = 0.0f;
		[SerializeField] float viscosityStrength = 1.0f;
		[SerializeField] float gravity;
		[SerializeField] int subStepCount;

		[Header("Visualization")]
		[SerializeField] float radius = 1.0f;
		[SerializeField] Color color = Color.white;

		[Header("Shaders")]
		[SerializeField] ComputeShader fluidComputeShader;
		[SerializeField] Material fluidMaterialShader;

		[Header("Input Interaction")]
		[SerializeField] float controlRadius = 2f;
		[SerializeField] float controlStregth = 1000f;

		[Header("Debug")]
		[SerializeField] bool drawDebug = true;
		[SerializeField] Material visualizeDensityMaterialShader;
		[SerializeField] Color positiveDensityColor = Color.red;
		[SerializeField] Color negativeDensityColor = Color.blue;
		[SerializeField] Color zeroDensityColor = Color.white;
		[SerializeField] Gradient speedColorMap;
		Texture2D speedColorTexture;

		// compute shader kernels
		int solveKernel;
		// parcel data buffer
		float2[] hostPositionBuffer;
		float2[] hostVelocityBuffer;
		ComputeBuffer devicePositionBuffer;
		ComputeBuffer deviceVelocityBuffer;
		float2[] hostPredictedPositionBuffer;
		float2[] hostDensityBuffer;
		// spatial grid
		SpatialGrid fixedRadiusNeighbourSearch;
		//
		ComputeBuffer argsBuffer;
		Mesh mesh;
		Vector2 halfSize;

		void Start()
		{
			halfSize = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));

			// init kernels
			solveKernel = fluidComputeShader.FindKernel("Solve");

			// init host buffers
			hostPositionBuffer = new float2[numParcels];
			hostVelocityBuffer = new float2[numParcels];
			hostPredictedPositionBuffer = new float2[numParcels];
			hostDensityBuffer = new float2[numParcels];

			// init device buffers
			devicePositionBuffer = new ComputeBuffer(numParcels, Marshal.SizeOf<float2>());
			deviceVelocityBuffer = new ComputeBuffer(numParcels, Marshal.SizeOf<float2>());

			// init parcel position
			int numParcelsPerLine = Mathf.CeilToInt(Mathf.Sqrt(numParcels));
			float spacePerParcel = ((2f + 0.5f) * radius);
			float basePos = -numParcelsPerLine / 2 * spacePerParcel;
			for (int i = 0; i < numParcels; i++)
			{
				float x = i % numParcelsPerLine * spacePerParcel + basePos;
				float y = i / numParcelsPerLine * spacePerParcel + basePos;
				//float x = UnityEngine.Random.Range(-halfSize.x, halfSize.x);
				//float y = UnityEngine.Random.Range(-halfSize.y, halfSize.y);
				hostPositionBuffer[i] = new float2(x, y);
			}
			devicePositionBuffer.SetData(hostPositionBuffer);

			// set buffers
			fluidComputeShader.SetBuffer(solveKernel, "positionBuffer", devicePositionBuffer);
			fluidComputeShader.SetBuffer(solveKernel, "velocityBuffer", deviceVelocityBuffer);
			fluidMaterialShader.SetBuffer("positionBuffer", devicePositionBuffer);
			fluidMaterialShader.SetBuffer("velocityBuffer", deviceVelocityBuffer);

			// init other members
			fixedRadiusNeighbourSearch = new SpatialGrid(numParcels);
			mesh = createMesh();

			// init instance indirect args
			uint[] args = new uint[5] { mesh.GetIndexCount(0), (uint)numParcels, 0, 0, 0 };
			argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

			// debug
			visualizeDensityMaterialShader.SetBuffer("positionBuffer", devicePositionBuffer);
			visualizeDensityMaterialShader.SetBuffer("velocityBuffer", deviceVelocityBuffer);
			speedColorTexture = new Texture2D(256, 1, TextureFormat.RGBA32, false);
			for (int i = 0; i < speedColorTexture.width; ++i)
			{
				float t = i / (float)(speedColorTexture.width - 1);
				Color color = speedColorMap.Evaluate(t);
				speedColorTexture.SetPixel(i, 0, color);
			}
			speedColorTexture.Apply();
		}

		void Update()
		{
			uint[] args = new uint[5] { mesh.GetIndexCount(0), (uint)numParcels, 0, 0, 0 };
			argsBuffer.SetData(args);

			// draw debug info
			if (drawDebug)
			{
				visualizeDensityMaterialShader.SetInt("numParcels", numParcels);
				visualizeDensityMaterialShader.SetFloat("smoothingRadius", interactionRadius);
				visualizeDensityMaterialShader.SetFloat("targetDensity", targetDensity);
				visualizeDensityMaterialShader.SetColor("positiveColor", positiveDensityColor);
				visualizeDensityMaterialShader.SetColor("negativeColor", negativeDensityColor);
				visualizeDensityMaterialShader.SetColor("zeroColor", zeroDensityColor);
				Graphics.DrawProcedural(visualizeDensityMaterialShader, new Bounds(Vector3.zero, Vector3.one * 1000f), MeshTopology.Triangles, 6, 1);
			}

			// draw parcels
			fluidMaterialShader.SetFloat("radius", radius);
			fluidMaterialShader.SetColor("color", color);
			fluidMaterialShader.SetFloat("maxSpeed", 5.0f);
			fluidMaterialShader.SetTexture("speedColorMap", speedColorTexture);
			Graphics.DrawMeshInstancedIndirect(mesh, 0, fluidMaterialShader, new Bounds(Vector3.zero, Vector3.one * 1000f), argsBuffer);
		}

		void FixedUpdate()
		{
			//fluidComputeShader.SetFloat("deltaTime", Time.deltaTime);
			//fluidComputeShader.SetFloat("radius", interactionRadius);
			//fluidComputeShader.SetInt("numParcels", numParcels);
			//fluidComputeShader.Dispatch(solveKernel, Mathf.CeilToInt(numParcels / 1024f), 1, 1);
			//devicePositionBuffer.GetData(hostPositionBuffer);

			simulationStep(subStepCount);

			devicePositionBuffer.SetData(hostPositionBuffer);
			deviceVelocityBuffer.SetData(hostVelocityBuffer);
		}

		void simulationStep(int stepCount)
		{
			float fixedDeltaTime = 1f / 60f;
			float predictDeltaTime = 1f / 120f;
			for (int sc = 0; sc < stepCount; sc++)
			{
				// apply mouse input
				bool bLeftButtonPressed = Input.GetMouseButton(0);
				bool bRightButtonPressed = Input.GetMouseButton(1);
				float interactionInputStrength = 0f;
				float2 inputPosition = new float2(
						Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
						Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
				if (bLeftButtonPressed || bRightButtonPressed)
				{
					interactionInputStrength += Input.GetMouseButton(0) ? controlStregth : 0f;
					interactionInputStrength += Input.GetMouseButton(1) ? -controlStregth : 0f;
				}

				// apply external forces, and predict positions
				float2 gravityAccel = new float2(0, -gravity);
				Parallel.For(0, numParcels, i =>
				{
					float2 externalForce = calcExternalForce(i, inputPosition, interactionInputStrength);
					hostVelocityBuffer[i] += externalForce * fixedDeltaTime;
					hostPredictedPositionBuffer[i] = hostPositionBuffer[i] + hostVelocityBuffer[i] * predictDeltaTime;
				});

				fixedRadiusNeighbourSearch.UpdateSpatialLookup(hostPredictedPositionBuffer, interactionRadius);
				
				// update density and pressure
				Parallel.For(0, numParcels, i =>
				{
					hostDensityBuffer[i] = calcDensity(i);
				});

				// pressure force
				Parallel.For(0, numParcels, i =>
				{
					hostVelocityBuffer[i] += calcPressureForce(i) * fixedDeltaTime;
				});
				// viscosity force
				Parallel.For(0, numParcels, i =>
				{
					hostVelocityBuffer[i] += calcViscosityForce(i) * fixedDeltaTime;
				});

				// update positions
				Parallel.For(0, numParcels, i =>
				{
					hostPositionBuffer[i] += hostVelocityBuffer[i] * fixedDeltaTime;
					handleCollision(i);
				});
			}
		}

		void OnDestroy()
		{
			devicePositionBuffer?.Release();
			deviceVelocityBuffer?.Release();
			argsBuffer?.Release();
		}

		float2 calcDensity(int sampleIdx)
		{
			float density = 0.0f;
			float nearDensity = 0.0f;
			float2 samplePosition = hostPredictedPositionBuffer[sampleIdx];
			fixedRadiusNeighbourSearch.ForeachPointWithinRadius(samplePosition, i =>
			{
				float dist = math.length(hostPredictedPositionBuffer[i] - samplePosition);
				density += spikyPow2Kernel(dist);
				nearDensity += spikyPow3Kernel(dist);
			});

			return new float2(density, nearDensity);
		}

		float calcPressureFormDensity(float density)
		{
			float densityError = density - targetDensity;
			return pressureCoefficient * densityError;
		}

		float calcNearPressureFormDensity(float nearDensity)
		{
			return nearPressureCoefficient * nearDensity;
		}

		float2 calcExternalForce(int parcelIdx, float2 inputPos, float inputStrength)
		{
			float2 gravityAccel = new float2(0, -gravity);

			if (inputStrength != 0)
			{
				float2 offset = inputPos - hostPositionBuffer[parcelIdx];
				float dist = math.length(offset);
				if (dist < controlRadius && dist > 0.001f)
				{
					float2 dir = offset / dist;
					float centerT = 1.0f - dist / controlRadius;

					float gravityWeight = 1 - (centerT * math.saturate(inputStrength / 10));
					float2 accel = gravityAccel * gravityWeight + centerT * (dir * inputStrength - hostVelocityBuffer[parcelIdx]);

					return accel;
				}
			}

			return gravityAccel;
		}

		float2 calcPressureForce(int sampleIdx)
		{
			float2 pressureForce = float2.zero;
			float2 positionA = hostPredictedPositionBuffer[sampleIdx];
			float densityA = hostDensityBuffer[sampleIdx].x;
			float pressureA = calcPressureFormDensity(densityA);
			float nearDensityA = hostDensityBuffer[sampleIdx].y;
			float nearPressureA = calcNearPressureFormDensity(nearDensityA); 

			fixedRadiusNeighbourSearch.ForeachPointWithinRadius(positionA, j =>
			{
				if (j == sampleIdx) return;

				float2 vec = (positionA - hostPredictedPositionBuffer[j]);
				float dist = math.length(vec);

				float densityB = hostDensityBuffer[j].x;
				float pressureB = calcPressureFormDensity(densityB);
				float nearDensityB = hostDensityBuffer[j].y;
				float nearPressureB = calcNearPressureFormDensity(nearDensityB);

				float pressureTerm = -(pressureA + pressureB) / (2.0f * densityB);
				float nearPressureTerm = -(nearPressureA + nearPressureB) / (2.0f * nearDensityB);
				pressureForce += pressureTerm * spikyPow2Gradient(vec, dist);
				pressureForce += nearPressureTerm * spikyPow3Gradient(vec, dist);
			});

			return pressureForce / densityA;
		}

		float2 calcViscosityForce(int sampleIdx)
		{
			float2 viscosityForce = float2.zero;
			float2 positionA = hostPredictedPositionBuffer[sampleIdx];

			fixedRadiusNeighbourSearch.ForeachPointWithinRadius(positionA, j =>
			{
				if (j == sampleIdx) return;

				float dist = math.length(positionA - hostPredictedPositionBuffer[j]);
				float influence = viscositySmoothingKernel(dist);
				viscosityForce += influence * (hostVelocityBuffer[j] - hostVelocityBuffer[sampleIdx]);
			});

			return viscosityStrength * viscosityForce;
		}

		void handleCollision(int index)
		{
			float damping = 0.8f;
			if (hostPositionBuffer[index].x > halfSize.x)
			{
				hostPositionBuffer[index].x = halfSize.x;
				hostVelocityBuffer[index].x *= -1 * damping;
			}
			else if (hostPositionBuffer[index].x < -halfSize.x)
			{
				hostPositionBuffer[index].x = -halfSize.x;
				hostVelocityBuffer[index].x *= -1 * damping;
			}
			if (hostPositionBuffer[index].y > halfSize.y)
			{
				hostPositionBuffer[index].y = halfSize.y;
				hostVelocityBuffer[index].y *= -1 * damping;
			}
			else if (hostPositionBuffer[index].y < -halfSize.y)
			{
				hostPositionBuffer[index].y = -halfSize.y;
				hostVelocityBuffer[index].y *= -1 * damping;
			}
		}

		float spikyPow2Kernel(float dist)
		{
			if (dist > interactionRadius) return 0.0f;
			float coeff = 6 / (Mathf.PI * Mathf.Pow(interactionRadius, 4));
			return coeff * Mathf.Pow(interactionRadius - dist, 2);
		}

		float2 spikyPow2Gradient(float2 vec, float dist)
		{
			if (dist > interactionRadius) return new float2(0, 0);
			float coeff = -12 / (Mathf.PI * Mathf.Pow(interactionRadius, 4));
			return coeff * (interactionRadius - dist) * (vec / (dist));
		}

		float spikyPow3Kernel(float dist)
		{
			if (dist > interactionRadius) return 0.0f;
			float coeff = 10 / (Mathf.PI * Mathf.Pow(interactionRadius, 5));
			return coeff * Mathf.Pow(interactionRadius - dist, 3);
		}

		float2 spikyPow3Gradient(float2 vec, float dist)
		{
			if (dist > interactionRadius ) return new float2(0, 0);
			float coeff = -30 / (Mathf.PI * Mathf.Pow(interactionRadius, 4));
			return coeff * Mathf.Pow(interactionRadius - dist, 2) * (vec / (dist));
		}

		float viscositySmoothingKernel(float dist)
		{
			if (dist > interactionRadius) return 0.0f;
			float volume = Mathf.PI * Mathf.Pow(interactionRadius, 8) / 4;
			float value = interactionRadius * interactionRadius - dist * dist;
			return value * value * value / volume;
		}

		Mesh createMesh()
		{
			// init meshe
			Mesh m = new Mesh();
			m.name = "FluidQuadMesh";
			Vector3[] vertices = new Vector3[6]
			   {
					new Vector3(-1, -1, 0), // 0
					new Vector3( 1, -1, 0), // 1
					new Vector3(-1,  1, 0), // 2

					new Vector3(-1,  1, 0), // 2
					new Vector3( 1, -1, 0), // 1
					new Vector3( 1,  1, 0)  // 3
			   };

			Vector2[] uv = new Vector2[6]
				{
					new Vector2(0, 0), // 0
					new Vector2(1, 0), // 1
					new Vector2(0, 1), // 2

					new Vector2(0, 1), // 2
					new Vector2(1, 0), // 1
					new Vector2(1, 1)  // 3
				};
			int[] indices = new int[6] { 0, 1, 2, 3, 4, 5 };

			m.vertices = vertices;
			m.uv = uv;
			m.SetIndices(indices, MeshTopology.Triangles, 0);
			m.RecalculateNormals();
			m.RecalculateBounds();

			return m;
		}
	}
}