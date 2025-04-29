using UnityEngine;

namespace HamCraft
{
	public class FluidIntensityRenderer : IFluidRenderer
	{
		[SerializeReference, SubclassPicker] FluidCircleRenderer fluidCircleRenderer;
		public Color positiveIntensityColor = new Color(0.83f, 0.3f, 0.2f);
		public Color negativeIntensityColor = new Color(0.13f, 0.53f, 0.7f);
		public Color zeroIntensityColor = Color.white;

		public override void Initialize(FluidSimulationGPU sim)
		{
			this.sim = sim;
			material.SetBuffer("positionBuffer", sim.devicePositionBuffer);
			material.SetBuffer("velocityBuffer", sim.deviceVelocityBuffer);

			if (fluidCircleRenderer == null)
			{
				fluidCircleRenderer = new FluidCircleRenderer();
				fluidCircleRenderer.material = Resources.Load<Material>("Fluid/FluidCircelMaterial");
			}
			fluidCircleRenderer.Initialize(sim);
		}

		public override void Draw()
		{ 
			material.SetInt("numParcels", sim.numParcels);
			material.SetFloat("smoothingRadius", sim.interactionRadius);
			material.SetFloat("targetDensity", sim.targetDensity);
			material.SetColor("positiveColor", positiveIntensityColor);
			material.SetColor("negativeColor", negativeIntensityColor);
			material.SetColor("zeroColor", zeroIntensityColor);
			Graphics.DrawProcedural(material, new Bounds(Vector3.zero, Vector3.one * 1000f), MeshTopology.Triangles, 6, 1);

			fluidCircleRenderer.Draw();
		}

		public override void CleanUp()
		{
			fluidCircleRenderer.CleanUp();
		}
	}
}
