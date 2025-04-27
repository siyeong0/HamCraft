
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace HamCraft
{
	[System.Serializable]
	public class FluidSimulation
	{
		[Header("Comput Shader")]
		public ComputeShader simulationComputeShader;

		[Header("Simulation")]
		public int numParcels = 4000;
		public float interactionRadius = 0.35f;
		public float targetDensity = 60f;
		public float pressureCoefficient = 150f;
		public float nearPressureCoefficient = 50f;
		public float viscosityStrength = 0.1f;
		public float gravity = 9f;
		public int subStepCout = 1;

		[Header("Input Interaction")]
		[SerializeField] float controlRadius = 3f;
		[SerializeField] float controlStregth = 35f;

		// compute shader kernels
		[HideInInspector] public int solveKernel;
		// parcel data buffer
		[HideInInspector] public float2[] hostPositionBuffer;
		[HideInInspector] public float2[] hostVelocityBuffer;
		[HideInInspector] public ComputeBuffer devicePositionBuffer;
		[HideInInspector] public ComputeBuffer deviceVelocityBuffer;
		[HideInInspector] public float2[] hostPredictedPositionBuffer;
		[HideInInspector] public float2[] hostDensityBuffer;
		// spatial grid
		[HideInInspector] public SpatialGrid fixedRadiusNeighbourSearch;

		[HideInInspector] public Vector2 halfSize;

		public void Initialize()
		{
			halfSize = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));

			// init kernels
			solveKernel = simulationComputeShader.FindKernel("Solve");

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
			float spacePerParcel = ((2f + 0.5f) * 0.05f);
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

			// set compute shader buffers
			simulationComputeShader.SetBuffer(solveKernel, "positionBuffer", devicePositionBuffer);
			simulationComputeShader.SetBuffer(solveKernel, "velocityBuffer", deviceVelocityBuffer);

			// init spatial grid
			fixedRadiusNeighbourSearch = new SpatialGrid(numParcels);
		}

		public void Step()
		{
			//fluidComputeShader.SetFloat("deltaTime", Time.deltaTime);
			//fluidComputeShader.SetFloat("radius", interactionRadius);
			//fluidComputeShader.SetInt("numParcels", numParcels);
			//fluidComputeShader.Dispatch(solveKernel, Mathf.CeilToInt(numParcels / 1024f), 1, 1);
			//devicePositionBuffer.GetData(hostPositionBuffer);

			for (int i = 0; i < subStepCout; ++i)
			{
				simulationStep();
			}

			devicePositionBuffer.SetData(hostPositionBuffer);
			deviceVelocityBuffer.SetData(hostVelocityBuffer);
		}

		void simulationStep()
		{
			float fixedDeltaTime = 1f / 60f;
			float predictDeltaTime = 1f / 120f;

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

		public void CleanUp()
		{
			devicePositionBuffer?.Release();
			deviceVelocityBuffer?.Release();
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
			if (dist > interactionRadius) return new float2(0, 0);
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
	}
}
