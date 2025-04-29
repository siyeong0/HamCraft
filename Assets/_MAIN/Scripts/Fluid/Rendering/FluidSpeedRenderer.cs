using UnityEngine;

namespace HamCraft
{
	public class FluidSpeedRenderer : IFluidRenderer
	{
		public float circleRadius = 0.05f;
		public float maxSpeed = 10f;
		public Gradient speedColorMap = new Gradient()
		{
			colorKeys = new GradientColorKey[] { new GradientColorKey(Color.blue, 0f), new GradientColorKey(new Color(0.32f, 1.0f, 0.57f), 0.5f), new GradientColorKey(Color.yellow, 0.65f), new GradientColorKey(Color.red, 1f) },
			alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 1f), new GradientAlphaKey(1f, 1f), new GradientAlphaKey(1f, 1f), new GradientAlphaKey(1f, 1f) }
		};

		ComputeBuffer argsBuffer;
		Mesh mesh;

		public override void Initialize(FluidSimulationGPU sim)
		{
			this.sim = sim;
			material.SetBuffer("positionBuffer", sim.devicePositionBuffer);
			material.SetBuffer("velocityBuffer", sim.deviceVelocityBuffer);

			Texture2D speedColorTexture = new Texture2D(256, 1, TextureFormat.RGBA32, false);
			for (int i = 0; i < speedColorTexture.width; ++i)
			{
				float t = i / (float)(speedColorTexture.width - 1);
				Color color = speedColorMap.Evaluate(t);
				speedColorTexture.SetPixel(i, 0, color);
			}
			speedColorTexture.Apply();
			material.SetTexture("speedColorMap", speedColorTexture);

			mesh = createQuadMesh();

			uint[] args = new uint[5] { mesh.GetIndexCount(0), 0, 0, 0, 0 };
			argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
		}

		public override void Draw()
		{
			uint[] args = new uint[5] { mesh.GetIndexCount(0), (uint)sim.numParcels, 0, 0, 0 };
			argsBuffer.SetData(args);

			material.SetFloat("radius", circleRadius);
			material.SetFloat("maxSpeed", maxSpeed);

			Graphics.DrawMeshInstancedIndirect(mesh, 0, material, new Bounds(Vector3.zero, Vector3.one * 1000f), argsBuffer);
		}

		public override void CleanUp()
		{
			argsBuffer?.Release();
		}
	}
}
