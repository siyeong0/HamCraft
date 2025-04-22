using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace HamCraft
{
	public class Fluid : MonoBehaviour
	{
		[SerializeField] ComputeShader fluidSimShader;
		[SerializeField] Material fluidMaterialShader;

		[SerializeField] int maxParcels = 100000;

		Parcel[] hostParcelBuffer;
		ComputeBuffer deviceParcelBuffer;

		ComputeBuffer argsBuffer;

		Mesh mesh;
		int kernel;

		void Start()
		{
			Debug.Assert(Marshal.SizeOf<Parcel>() == sizeof(float) * 4, "Parcel struct size is not correct!");

			// initialize the parcel data buffer
			hostParcelBuffer = new Parcel[maxParcels];
			for (int i = 0; i < maxParcels; i++)
			{
				hostParcelBuffer[i].Position = new float2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
				hostParcelBuffer[i].Velocity = new float2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
			}

			deviceParcelBuffer = new ComputeBuffer(maxParcels, Marshal.SizeOf<Parcel>());
			deviceParcelBuffer.SetData(hostParcelBuffer);

			// initialize shaders
			kernel = fluidSimShader.FindKernel("CSMain");
			fluidSimShader.SetBuffer(kernel, "parcelBuffer", deviceParcelBuffer);

			fluidMaterialShader.SetBuffer("parcelBuffer", deviceParcelBuffer);

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


			uint[] args = new uint[5] { mesh.GetIndexCount(0), (uint)maxParcels, 0, 0, 0 };
			argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
			argsBuffer.SetData(args);
		}

		private void Update()
		{
			fluidSimShader.SetFloat("deltaTime", Time.deltaTime);
			fluidSimShader.Dispatch(kernel, Mathf.CeilToInt(maxParcels / 256f), 1, 1);

			Graphics.DrawMeshInstancedIndirect(mesh, 0, fluidMaterialShader, new Bounds(Vector3.zero, Vector3.one * 100f), argsBuffer);

			deviceParcelBuffer.GetData(hostParcelBuffer);
		}

		void OnDestroy()
		{
			deviceParcelBuffer?.Release();
			argsBuffer?.Release();
		}
	}
}