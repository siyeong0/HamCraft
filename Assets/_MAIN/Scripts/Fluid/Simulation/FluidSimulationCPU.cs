using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace HamCraft
{
	[System.Serializable]
	public class FluidSimulationCPU
	{
		[Header("Simulation")]
		public float interactionRadius = 0.35f;
		public float targetDensity = 60f;
		public float pressureStiffness = 150f;
		public float nearPressureStiffness = 50f;
		public float viscosityStrength = 0.1f;
		public float collisionDamping = 0.8f;
		public float gravity = 9f;
		public int subStepCount = 1;

		[Header("Input Interaction")]
		public float controlRadius = 3f;
		public float controlStregth = 35f;

		[Header("Initial Placement")]
		public int numParcels = 4000;
		public float spacing = 0.05f;

		// parcel data buffers
		[HideInInspector] public float2[] hostPositionBuffer;
		[HideInInspector] public float2[] hostPredictedPositionBuffer;
		[HideInInspector] public float2[] hostVelocityBuffer;
		[HideInInspector] public float2[] hostDensityBuffer;
		[HideInInspector] public uint3[] hostSpatialEntryBuffer;
		[HideInInspector] public uint[] hostSpatialOffsetBuffer;

		// compute buffer for rendering precess
		[HideInInspector] public ComputeBuffer devicePositionBuffer;
		[HideInInspector] public ComputeBuffer deviceVelocityBuffer;

		// spatial grid
		[HideInInspector] public SpatialGrid fixedRadiusNeighbourSearch;

		// other values
		[HideInInspector] public int numEntries;
		[HideInInspector] public Vector2 halfSize;

		public void Initialize()
		{
			numEntries = getNextPow2(numParcels);
			halfSize = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));

			// init host buffers
			hostPositionBuffer = new float2[numParcels];
			hostVelocityBuffer = new float2[numParcels];
			hostPredictedPositionBuffer = new float2[numParcels];
			hostDensityBuffer = new float2[numParcels];
			hostSpatialEntryBuffer = new uint3[numEntries];
			hostSpatialOffsetBuffer = new uint[numEntries];

			// init compute buffers
			devicePositionBuffer = new ComputeBuffer(numParcels, Marshal.SizeOf<float2>());
			deviceVelocityBuffer = new ComputeBuffer(numParcels, Marshal.SizeOf<float2>());

			// init spatial grid
			fixedRadiusNeighbourSearch = new SpatialGrid(numParcels);

			// init parcel position
			int numParcelsPerLine = Mathf.CeilToInt(Mathf.Sqrt(numParcels));
			float basePos = -numParcelsPerLine / 2 * spacing;
			for (int i = 0; i < numParcels; i++)
			{
				float x = i % numParcelsPerLine * spacing + basePos;
				float y = i / numParcelsPerLine * spacing + basePos;
				//float x = UnityEngine.Random.Range(-halfSize.x, halfSize.x);
				//float y = UnityEngine.Random.Range(-halfSize.y, halfSize.y);
				hostPositionBuffer[i] = new float2(x, y);
			}
		}

		public void Step()
		{
			for (int i = 0; i < subStepCount; ++i)
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

			// apply external forces, and predict positions
			(var inputPosition, var interactionInputStrength) = getMouseInput();
			Parallel.For(0, numParcels, i =>
			{
				float2 externalForce = calcExternalForce(i, inputPosition, interactionInputStrength);
				hostVelocityBuffer[i] += externalForce * fixedDeltaTime;
				hostPredictedPositionBuffer[i] = hostPositionBuffer[i] + hostVelocityBuffer[i] * predictDeltaTime;
			});

			// update spatial grid
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
				density += spikyPow2Kernel(dist, interactionRadius);
				nearDensity += spikyPow3Kernel(dist, interactionRadius);
			});

			return new float2(density, nearDensity);
		}

		float calcPressureFormDensity(float density)
		{
			float densityError = density - targetDensity;
			return pressureStiffness * densityError;
		}

		float calcNearPressureFormDensity(float nearDensity)
		{
			return nearPressureStiffness * nearDensity;
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
				pressureForce += pressureTerm * spikyPow2Gradient(vec, dist, interactionRadius);
				pressureForce += nearPressureTerm * spikyPow3Gradient(vec, dist, interactionRadius);
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
				float influence = viscositySmoothingKernel(dist, interactionRadius);
				viscosityForce += influence * (hostVelocityBuffer[j] - hostVelocityBuffer[sampleIdx]);
			});

			return viscosityStrength * viscosityForce;
		}

		void handleCollision(int index)
		{
			if (hostPositionBuffer[index].x > halfSize.x)
			{
				hostPositionBuffer[index].x = halfSize.x;
				hostVelocityBuffer[index].x *= -1 * collisionDamping;
			}
			else if (hostPositionBuffer[index].x < -halfSize.x)
			{
				hostPositionBuffer[index].x = -halfSize.x;
				hostVelocityBuffer[index].x *= -1 * collisionDamping;
			}
			if (hostPositionBuffer[index].y > halfSize.y)
			{
				hostPositionBuffer[index].y = halfSize.y;
				hostVelocityBuffer[index].y *= -1 * collisionDamping;
			}
			else if (hostPositionBuffer[index].y < -halfSize.y)
			{
				hostPositionBuffer[index].y = -halfSize.y;
				hostVelocityBuffer[index].y *= -1 * collisionDamping;
			}
		}

		(Vector2, float) getMouseInput()
		{
			bool bLeftButtonPressed = Input.GetMouseButton(0);
			bool bRightButtonPressed = Input.GetMouseButton(1);
			float inputStrength = 0f;
			Vector2 inputPosition = new Vector2(
					Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
					Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
			if (bLeftButtonPressed || bRightButtonPressed)
			{
				inputStrength += Input.GetMouseButton(0) ? controlStregth : 0f;
				inputStrength += Input.GetMouseButton(1) ? -controlStregth : 0f;
			}

			return (inputPosition, inputStrength);
		}

		static float spikyPow2Kernel(float dist, float radius)
		{
			if (dist > radius) return 0.0f;
			float coeff = 6 / (Mathf.PI * Mathf.Pow(radius, 4));
			return coeff * Mathf.Pow(radius - dist, 2);
		}

		static float2 spikyPow2Gradient(float2 vec, float dist, float radius)
		{
			if (dist > radius) return new float2(0, 0);
			float coeff = -12 / (Mathf.PI * Mathf.Pow(radius, 4));
			return coeff * (radius - dist) * (vec / (dist));
		}

		static float spikyPow3Kernel(float dist, float radius)
		{
			if (dist > radius) return 0.0f;
			float coeff = 10 / (Mathf.PI * Mathf.Pow(radius, 5));
			return coeff * Mathf.Pow(radius - dist, 3);
		}

		static float2 spikyPow3Gradient(float2 vec, float dist, float radius)
		{
			if (dist > radius) return new float2(0, 0);
			float coeff = -30 / (Mathf.PI * Mathf.Pow(radius, 4));
			return coeff * Mathf.Pow(radius - dist, 2) * (vec / (dist));
		}

		static float viscositySmoothingKernel(float dist, float radius)
		{
			if (dist > radius) return 0.0f;
			float volume = Mathf.PI * Mathf.Pow(radius, 8) / 4;
			float value = radius * radius - dist * dist;
			return value * value * value / volume;
		}

		static int getNextPow2(int n)
		{
			int power = 1;
			while (power < n)
				power *= 2;
			return power;
		}
	}
}
