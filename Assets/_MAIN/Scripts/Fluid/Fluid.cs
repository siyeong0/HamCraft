using System;
using Unity.Mathematics;
using UnityEngine;

namespace HamCraft
{
	public class Fluid : MonoBehaviour
	{
		[SerializeField] ComputeShader fluidShader;
		[SerializeField] Material fluidMaterial;

		[SerializeField] int MAX_PARCELS = 100000;

		ComputeBuffer devicePositionBuffer;
		float2[] hostPositionBuffer;
		ComputeBuffer deviceVelocityBuffer;
		float2[] hostVelocityBuffer;
		ComputeBuffer argsBuffer;

		Mesh mesh;
		int kernel;

		void Start()
		{
			hostPositionBuffer = new float2[MAX_PARCELS];
			hostVelocityBuffer = new float2[MAX_PARCELS];
			for (int i = 0; i < MAX_PARCELS; i++)
			{
				hostPositionBuffer[i] = new float2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
				hostVelocityBuffer[i] = new float2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
			}

			devicePositionBuffer = new ComputeBuffer(MAX_PARCELS, sizeof(float) * 2);
			devicePositionBuffer.SetData(hostPositionBuffer);
			deviceVelocityBuffer = new ComputeBuffer(MAX_PARCELS, sizeof(float) * 2);
			deviceVelocityBuffer.SetData(hostVelocityBuffer);

			kernel = fluidShader.FindKernel("CSMain");
			fluidShader.SetBuffer(kernel, "positionBuffer", devicePositionBuffer);
			fluidShader.SetBuffer(kernel, "velocityBuffer", deviceVelocityBuffer);

			fluidMaterial.SetBuffer("positions", devicePositionBuffer);

			mesh = new Mesh();
			mesh.name = "FluidQuadMesh";
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

			mesh.vertices = vertices;
			mesh.uv = uv;
			mesh.SetIndices(indices, MeshTopology.Triangles, 0);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();


			uint[] args = new uint[5] { mesh.GetIndexCount(0), (uint)MAX_PARCELS, 0, 0, 0 };
			argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
			argsBuffer.SetData(args);
		}

		private void Update()
		{
			fluidShader.SetFloat("deltaTime", Time.deltaTime);
			fluidShader.Dispatch(kernel, Mathf.CeilToInt(MAX_PARCELS / 256f), 1, 1);

			Graphics.DrawMeshInstancedIndirect(mesh, 0, fluidMaterial, new Bounds(Vector3.zero, Vector3.one * 100f), argsBuffer);

			devicePositionBuffer.GetData(hostPositionBuffer);
		}

		void OnDestroy()
		{
			devicePositionBuffer?.Release();
			deviceVelocityBuffer?.Release();
			argsBuffer?.Release();
		}
	}
}