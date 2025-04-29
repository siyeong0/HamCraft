using UnityEngine;

namespace HamCraft
{
	public abstract class IFluidRenderer
	{
		public Material material;

		protected FluidSimulationGPU sim;
		public virtual void Initialize(FluidSimulationGPU sim)
		{
			this.sim = sim;
		}

		public virtual void CleanUp()
		{

		}

		public abstract void Draw();

		protected Mesh createQuadMesh()
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
