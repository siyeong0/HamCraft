using Unity.Mathematics;
using UnityEngine;

using System.Runtime.InteropServices;

namespace HamCraft
{
	public class Fluid : MonoBehaviour
	{
		[Header("Simulation")]
		[SerializeField] int numParcels = 10000;
		[SerializeField] float interactionRadius = 1.0f;
		[SerializeField] float mass = 0.1f;
		[SerializeField] float targetDensity = 1.0f;

		[Header("Visualization")]
		[SerializeField] float radius = 1.0f;
		[SerializeField] Color color = Color.white;

		[Header("Shaders")]
		[SerializeField] ComputeShader fluidComputeShader;
		[SerializeField] Material fluidMaterialShader;

		[Header("Debug")]
		[SerializeField] Material visualizeDensityMaterialShader;
		[SerializeField] Color positiveDensityColor = Color.red;
		[SerializeField] Color negativeDensityColor = Color.blue;
		[SerializeField] Color zeroDensityColor = Color.white;

		// parcel data buffer
		Parcel[] hostParcelBuffer;
		ComputeBuffer deviceParcelBuffer;

		ComputeBuffer argsBuffer;

		int solveKernel;

		Mesh mesh;
		Vector2 halfSize;

		void Start()
		{
			Vector2 halfSize = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));

			// init kernels
			solveKernel = fluidComputeShader.FindKernel("Solve");

			// init parcels
			int numParcelsPerLine = Mathf.CeilToInt(Mathf.Sqrt(numParcels));
			float spacePerParcel = (2 * radius + 0.1f);
			float basePos = -numParcelsPerLine / 2 * spacePerParcel;
			hostParcelBuffer = new Parcel[numParcels];
			for (int i = 0; i < numParcels; i++)
			{
				//float x = i % numParcelsPerLine * spacePerParcel + basePos;
				//float y = i / numParcelsPerLine * spacePerParcel + basePos;
				float x = UnityEngine.Random.Range(-halfSize.x, halfSize.x);
				float y = UnityEngine.Random.Range(-halfSize.y, halfSize.y);
				hostParcelBuffer[i].Position = new float2(x, y);
			}

			// init buffers
			deviceParcelBuffer = new ComputeBuffer(numParcels, Marshal.SizeOf<Parcel>());
			deviceParcelBuffer.SetData(hostParcelBuffer);

			// set buffers
			fluidComputeShader.SetBuffer(solveKernel, "parcelBuffer", deviceParcelBuffer);
			fluidMaterialShader.SetBuffer("parcelBuffer", deviceParcelBuffer);

			// init other members
			mesh = createMesh();

			// init instance indirect args
			uint[] args = new uint[5] { mesh.GetIndexCount(0), (uint)numParcels, 0, 0, 0 };
			argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

			// debug
			visualizeDensityMaterialShader.SetBuffer("parcelBuffer", deviceParcelBuffer);
		}

		void Update()
		{
			uint[] args = new uint[5] { mesh.GetIndexCount(0), (uint)numParcels, 0, 0, 0 };
			argsBuffer.SetData(args);

			// draw debug info
			visualizeDensityMaterialShader.SetInt("numParcels", numParcels);
			visualizeDensityMaterialShader.SetFloat("smoothingRadius", interactionRadius);
			visualizeDensityMaterialShader.SetFloat("targetDensity", targetDensity);
			visualizeDensityMaterialShader.SetColor("positiveColor", positiveDensityColor);
			visualizeDensityMaterialShader.SetColor("negativeColor", negativeDensityColor);
			visualizeDensityMaterialShader.SetColor("zeroColor", zeroDensityColor);
			Graphics.DrawProcedural(visualizeDensityMaterialShader, new Bounds(Vector3.zero, Vector3.one * 1000f), MeshTopology.Triangles, 6, 1);

			// draw parcels
			fluidMaterialShader.SetFloat("radius", radius);
			fluidMaterialShader.SetColor("color", color);
			Graphics.DrawMeshInstancedIndirect(mesh, 0, fluidMaterialShader, new Bounds(Vector3.zero, Vector3.one * 1000f), argsBuffer);
		}

		void FixedUpdate()
		{
			fluidComputeShader.SetFloat("deltaTime", Time.deltaTime);
			fluidComputeShader.SetFloat("radius", interactionRadius);
			fluidComputeShader.SetFloat("mass", mass);
			fluidComputeShader.SetInt("numParcels", numParcels);

			fluidComputeShader.Dispatch(solveKernel, Mathf.CeilToInt(numParcels / 1024f), 1, 1);

			deviceParcelBuffer.GetData(hostParcelBuffer);
		}

		void OnDestroy()
		{
			deviceParcelBuffer?.Release();
			argsBuffer?.Release();
		}

		void OnGUI()
		{
			Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			float2 samplePoint = new float2(mousePosition.x, mousePosition.y);
			float density = sampleDensity(samplePoint);

			string message = "Density : " + density;

			GUIStyle style = new GUIStyle();
			style.fontSize = 96;
			style.normal.textColor = Color.black;
			GUI.Label(new Rect(10, 10, 300, 20), message, style);
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

		float sampleDensity(float2 samplePoint)
		{
			float density = 0.0f;
			float mass = 1.0f;
			foreach (var parcel in hostParcelBuffer)
			{
				float dist = math.length(parcel.Position - samplePoint);
				float influence = smoothKernel(dist, interactionRadius);
				density += mass * influence;
			}
			return density;
		}

		float smoothKernel(float dist, float radius)
		{
			float volume = 3.141592f * Mathf.Pow(radius, 8) / 4;
			float value = Mathf.Max(0, radius * radius - dist * dist);
			return value * value * value / volume;
		}
	}
}