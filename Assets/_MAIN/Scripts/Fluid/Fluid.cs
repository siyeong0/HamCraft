using UnityEngine;
using UnityEngine.Assertions;

namespace HamCraft
{
	public class Fluid : MonoBehaviour
	{
		[SerializeField] FluidSimulation simulation;
		[SerializeReference, SubclassPicker] IFluidRenderer rendering;

		private void Start()
		{
			// init sim
			simulation.Initialize();

			// init renderer
			rendering.Initialize(simulation);
		}

		private void Update()
		{
			rendering.Draw();
		}

		private void FixedUpdate()
		{
			simulation.Step();
		}

		private void OnDestroy()
		{
			rendering?.CleanUp();
			simulation?.CleanUp();
		}
	}
}