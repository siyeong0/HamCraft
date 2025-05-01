using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace HamCraft
{
	[System.Serializable]
	public class FluidSimulationGPU
	{
		[Header("Comput Shader")]
		public ComputeShader simulationComputeShader;
		public ComputeShader bitonicSortComputeShader;

		[Header("Simulation")]
		public float interactionRadius = 0.25f;
		public float targetDensity = 200f;
		public float pressureStiffness = 150f;
		public float nearPressureStiffness = 10f;
		public float viscosityStrength = 0.075f;
		[Range(0,1)]public float collisionDamping = 0.2f;
		public float gravity = 9f;
		public uint subStepCount = 4;

		[Header("Input Interaction")]
		public float controlRadius = 3f;
		public float controlStregth = 35f;

		[Header("Initial Placement")]
		public int numParcels = 20000;
		public float spacing = 0.008f;

		// compute shader kernels
		[HideInInspector] public int applyExternalForcesKernel;
		[HideInInspector] public int updateSpatialEntriesKernel;
		[HideInInspector] public int sortKernel;
		[HideInInspector] public int updateSpatialOffsetsKernel;
		[HideInInspector] public int updateDensitiesKernel;
		[HideInInspector] public int applyPressureForcesKernel;
		[HideInInspector] public int applyViscocityForcesKernel;
		[HideInInspector] public int updatePositionsKernel;

		// parcel data buffer
		[HideInInspector] public float2[] hostPositionBuffer;
		[HideInInspector] public float2[] hostVelocityBuffer;

		// compute buffers
		[HideInInspector] public ComputeBuffer devicePositionBuffer;
		[HideInInspector] public ComputeBuffer devicePredictedPositionBuffer;
		[HideInInspector] public ComputeBuffer deviceVelocityBuffer;
		[HideInInspector] public ComputeBuffer deviceDensityBuffer;
		[HideInInspector] public ComputeBuffer deviceSpatialEntryBuffer;
		[HideInInspector] public ComputeBuffer deviceSpatialOffsetBuffer;

		// other values
		[HideInInspector] public int numEntries;
		[HideInInspector] public Vector2 halfSize;

		public void Initialize()
		{
			numEntries = getNextPow2(numParcels);
			halfSize = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));

			// init kernels
			applyExternalForcesKernel = simulationComputeShader.FindKernel("ApplyExternalForces");
			updateSpatialEntriesKernel = simulationComputeShader.FindKernel("UpdateSpatialEntries");
			sortKernel = simulationComputeShader.FindKernel("Sort");
			updateSpatialOffsetsKernel = simulationComputeShader.FindKernel("UpdateSpatialOffsets");
			updateDensitiesKernel = simulationComputeShader.FindKernel("UpdateDensities");
			applyPressureForcesKernel = simulationComputeShader.FindKernel("ApplyPressureForces");
			applyViscocityForcesKernel = simulationComputeShader.FindKernel("ApplyViscocityForces");
			updatePositionsKernel = simulationComputeShader.FindKernel("UpdatePositions");

			// init host buffers
			hostPositionBuffer = new float2[numParcels];
			hostVelocityBuffer = new float2[numParcels];

			// init device buffers
			devicePositionBuffer = new ComputeBuffer(numParcels, Marshal.SizeOf<float2>());
			devicePredictedPositionBuffer = new ComputeBuffer(numParcels, Marshal.SizeOf<float2>());
			deviceVelocityBuffer = new ComputeBuffer(numParcels, Marshal.SizeOf<float2>());
			deviceDensityBuffer = new ComputeBuffer(numParcels, Marshal.SizeOf<float2>());
			deviceSpatialEntryBuffer = new ComputeBuffer(numEntries, Marshal.SizeOf<uint3>());
			deviceSpatialOffsetBuffer = new ComputeBuffer(numEntries, Marshal.SizeOf<uint>());

			// set compute shader buffers
			simulationComputeShader.SetBuffer(applyExternalForcesKernel, "positionBuffer", devicePositionBuffer);
			simulationComputeShader.SetBuffer(applyExternalForcesKernel, "predictedPositionBuffer", devicePredictedPositionBuffer);
			simulationComputeShader.SetBuffer(applyExternalForcesKernel, "velocityBuffer", deviceVelocityBuffer);

			simulationComputeShader.SetBuffer(updateSpatialEntriesKernel, "predictedPositionBuffer", devicePredictedPositionBuffer);
			simulationComputeShader.SetBuffer(updateSpatialEntriesKernel, "spatialEntryBuffer", deviceSpatialEntryBuffer);
			simulationComputeShader.SetBuffer(updateSpatialEntriesKernel, "spatialOffsetBuffer", deviceSpatialOffsetBuffer);

			simulationComputeShader.SetBuffer(sortKernel, "spatialEntryBuffer", deviceSpatialEntryBuffer);

			simulationComputeShader.SetBuffer(updateSpatialOffsetsKernel, "spatialEntryBuffer", deviceSpatialEntryBuffer);
			simulationComputeShader.SetBuffer(updateSpatialOffsetsKernel, "spatialOffsetBuffer", deviceSpatialOffsetBuffer);

			simulationComputeShader.SetBuffer(updateDensitiesKernel, "predictedPositionBuffer", devicePredictedPositionBuffer);
			simulationComputeShader.SetBuffer(updateDensitiesKernel, "densityBuffer", deviceDensityBuffer);
			simulationComputeShader.SetBuffer(updateDensitiesKernel, "spatialEntryBuffer", deviceSpatialEntryBuffer);
			simulationComputeShader.SetBuffer(updateDensitiesKernel, "spatialOffsetBuffer", deviceSpatialOffsetBuffer);

			simulationComputeShader.SetBuffer(applyPressureForcesKernel, "predictedPositionBuffer", devicePredictedPositionBuffer);
			simulationComputeShader.SetBuffer(applyPressureForcesKernel, "velocityBuffer", deviceVelocityBuffer);
			simulationComputeShader.SetBuffer(applyPressureForcesKernel, "densityBuffer", deviceDensityBuffer);
			simulationComputeShader.SetBuffer(applyPressureForcesKernel, "spatialEntryBuffer", deviceSpatialEntryBuffer);
			simulationComputeShader.SetBuffer(applyPressureForcesKernel, "spatialOffsetBuffer", deviceSpatialOffsetBuffer);

			simulationComputeShader.SetBuffer(applyViscocityForcesKernel, "predictedPositionBuffer", devicePredictedPositionBuffer);
			simulationComputeShader.SetBuffer(applyViscocityForcesKernel, "velocityBuffer", deviceVelocityBuffer);
			simulationComputeShader.SetBuffer(applyViscocityForcesKernel, "spatialEntryBuffer", deviceSpatialEntryBuffer);
			simulationComputeShader.SetBuffer(applyViscocityForcesKernel, "spatialOffsetBuffer", deviceSpatialOffsetBuffer);

			simulationComputeShader.SetBuffer(updatePositionsKernel, "positionBuffer", devicePositionBuffer);
			simulationComputeShader.SetBuffer(updatePositionsKernel, "velocityBuffer", deviceVelocityBuffer);

			// init parcels
			int numParcelsPerLine = Mathf.CeilToInt(Mathf.Sqrt(numParcels));
			float basePos = -numParcelsPerLine / 2 * spacing;
			for (int i = 0; i < numParcels; i++)
			{
				float x = i % numParcelsPerLine * spacing + basePos;
				float y = i / numParcelsPerLine * spacing + basePos;
				hostPositionBuffer[i] = new float2(x, y);
				hostVelocityBuffer[i] = float2.zero;
			}

			devicePositionBuffer.SetData(hostPositionBuffer);
			devicePredictedPositionBuffer.SetData(hostPositionBuffer);
			deviceVelocityBuffer.SetData(hostVelocityBuffer);

			// init spatial grid
			uint3[] spatialEntryBuffer = new uint3[numEntries];
			uint[] spatialOffsetBuffer = new uint[numEntries];
			for (int i = 0; i < numEntries; i++)
			{
				spatialEntryBuffer[i] = new uint3(uint.MaxValue, uint.MaxValue, uint.MaxValue);
				spatialOffsetBuffer[i] = (uint)numEntries;
			}

			deviceSpatialEntryBuffer.SetData(spatialEntryBuffer);
			deviceSpatialOffsetBuffer.SetData(spatialOffsetBuffer);
		}

		public void Step()
		{
			updateConstants();

			for (int i = 0; i < subStepCount; ++i)
			{
				simulationStep();
			}
		}

		void simulationStep()
		{
			// dispatch
			simulationComputeShader.Dispatch(applyExternalForcesKernel, Mathf.CeilToInt(numParcels / 1024f), 1, 1);
			simulationComputeShader.Dispatch(updateSpatialEntriesKernel, Mathf.CeilToInt(numParcels / 1024f), 1, 1);
			for (int k = 2; k <= numEntries; k *= 2)
			{
				for (int j = k / 2; j > 0; j /= 2)
				{
					simulationComputeShader.SetInt("k", k);
					simulationComputeShader.SetInt("j", j);
					simulationComputeShader.Dispatch(sortKernel, Mathf.CeilToInt(numEntries / 256f), 1, 1);
				}
			}
			simulationComputeShader.Dispatch(updateSpatialOffsetsKernel, Mathf.CeilToInt(numParcels / 1024f), 1, 1);
			simulationComputeShader.Dispatch(updateDensitiesKernel, Mathf.CeilToInt(numParcels / 1024f), 1, 1);
			simulationComputeShader.Dispatch(applyPressureForcesKernel, Mathf.CeilToInt(numParcels / 1024f), 1, 1);
			simulationComputeShader.Dispatch(applyViscocityForcesKernel, Mathf.CeilToInt(numParcels / 1024f), 1, 1);
			simulationComputeShader.Dispatch(updatePositionsKernel, Mathf.CeilToInt(numParcels / 1024f), 1, 1);
		}

		public void CleanUp()
		{
			devicePositionBuffer?.Release();
			deviceVelocityBuffer?.Release();
			devicePredictedPositionBuffer?.Release();
			deviceDensityBuffer?.Release();
			deviceSpatialEntryBuffer?.Release();
			deviceSpatialOffsetBuffer?.Release();
		}

		void updateConstants()
		{
			float fixedDeltaTime = 1f / 120f;
			float predictDeltaTime = 1f / 240f;

			// update constant values
			simulationComputeShader.SetFloat("deltaTime", fixedDeltaTime);
			simulationComputeShader.SetFloat("predictDeltaTime", predictDeltaTime);

			simulationComputeShader.SetFloat("interactionRadius", interactionRadius);
			simulationComputeShader.SetFloat("targetDensity", targetDensity);
			simulationComputeShader.SetFloat("pressureStiffness", pressureStiffness);
			simulationComputeShader.SetFloat("nearPressureStiffness", nearPressureStiffness);
			simulationComputeShader.SetFloat("viscosityStrength", viscosityStrength);
			simulationComputeShader.SetFloat("collisionDamping", collisionDamping);
			simulationComputeShader.SetVector("gravity", new Vector2(0, -gravity));
			simulationComputeShader.SetVector("bounds", new Vector4(-halfSize.x, -halfSize.y, halfSize.x, halfSize.y));

			(var inputPosition, var interactionInputStrength) = getMouseInput();
			simulationComputeShader.SetFloat("inputControlRadius", controlRadius);
			simulationComputeShader.SetVector("inputPosition", inputPosition);
			simulationComputeShader.SetFloat("inputStrength", interactionInputStrength);

			simulationComputeShader.SetInt("numParcels", numParcels);
			simulationComputeShader.SetInt("numEntries", numEntries);
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

		int getNextPow2(int n)
		{
			int power = 1;
			while (power < n)
				power *= 2;
			return power;
		}
	}
}
