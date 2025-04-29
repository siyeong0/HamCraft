using UnityEngine;

namespace HamCraft
{
	[System.Serializable]
	public class FluidCircleRenderer : IFluidRenderer
	{
		public float circleRadius = 0.05f;
		public Color circleColor = Color.white;

		ComputeBuffer argsBuffer;
		Mesh mesh;

		public override void Initialize(FluidSimulationGPU sim)
		{
			this.sim = sim;
			material.SetBuffer("positionBuffer", sim.devicePositionBuffer);

			mesh = createQuadMesh();

			uint[] args = new uint[5] { mesh.GetIndexCount(0), 0, 0, 0, 0 };
			argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
		}

		public override void Draw()
		{
			uint[] args = new uint[5] { mesh.GetIndexCount(0), (uint)sim.numParcels, 0, 0, 0 };
			argsBuffer.SetData(args);

			material.SetFloat("radius", circleRadius);
			material.SetColor("color", circleColor);

			Graphics.DrawMeshInstancedIndirect(mesh, 0, material, new Bounds(Vector3.zero, Vector3.one * 1000f), argsBuffer);
		}

		public override void CleanUp()
		{
			argsBuffer?.Release();
		}
	}
}
