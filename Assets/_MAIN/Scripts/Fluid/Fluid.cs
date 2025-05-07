using UnityEngine;

namespace HamCraft
{
	public class Fluid : MonoBehaviour
	{
		[SerializeField] GameObject obj;
		[Space(20)]

		[SerializeField] FluidSimulationGPU simulation;
		[Space(10)]
		[SerializeReference, SubclassPicker] IFluidRenderer rendering;
		private void Awake() 
		{
			// init sim
			simulation.Initialize();

			// init renderer
			rendering.Initialize(simulation);
		}

		private void Update()
		{
			rendering.Draw();

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				simulation.CleanUp();
				rendering.CleanUp();
				simulation.Initialize();
				rendering.Initialize(simulation);
			}

			if (Input.GetKeyDown(KeyCode.Space))
			{
				obj.SetActive(true);
			}

			if (Input.GetKeyDown(KeyCode.R))
			{
				var rb = obj.GetComponent<Rigidbody2D>();
				rb.AddTorque(100);
			}

			bool bLeftButtonPressed = Input.GetMouseButton(0);
			bool bRightButtonPressed = Input.GetMouseButton(1);
			Vector2 inputPosition = new Vector2(
					Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
					Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

			obj.transform.position = inputPosition;
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

		private void OnDrawGizmos()
		{
			simulation.DrawGizmoPolygons();
		}
	}
}