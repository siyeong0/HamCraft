using UnityEngine;
using LindenmayerSystem;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class Plant : MonoBehaviour
{
	[SerializeField] int iterations;

	string[] variables = new string[] { "L", "fwd", "rot" };
	string[] functions = new string[] { "randrange" };
	char[] constants = new char[] { '[', ']' };
	string axiom = "L(0)";
	List<(string, string)> rules = new List<(string, string)>
		{
			("L(g)",    "fwd(randrange(100/(g+1)-10, 100/(g+1)+10),5/(g+1))" +
						"rot(3/(g+1))" +
						"[rot(randrange(-25,-15))L(g+1)]" +
						"rot(3/(g+1))" +
						"[rot(randrange(15,25))L(g+1)]" +
						"rot(3/(g+1))"),
		};

	LSystem lsys;

	int prevIterations;

	void Start()
    {
		lsys = new LSystem(variables, functions, constants, new Behavior(transform));

		prevIterations = 0;
	}

    void Update()
    {
        if (iterations != prevIterations)
		{
			foreach (Transform child in transform)
			{
				Destroy(child.gameObject);
			}

			List<Token> expr = lsys.Build(axiom, rules, iterations);
			lsys.Execute(expr);

			prevIterations = iterations;
		}
    }

	public class Behavior
	{
		struct State
		{
			public Vector2 position;
			public float rotation;
			public Transform trunck;
		}

		State currState;
		Stack<State> stateStack;

		public Behavior(Transform root)
		{
			currState = new State() { position = new Vector2(0, 0), rotation = 0.0f, trunck = root };
			stateStack = new Stack<State>();
		}

		public void fwd(float length, float thickness)
		{
			GameObject branch = new GameObject("Branch");
			branch.transform.SetParent(currState.trunck);

			branch.transform.localPosition = currState.position;
			LineRenderer lineRenderer = branch.AddComponent<LineRenderer>();
			lineRenderer.positionCount = 2;
			lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
			lineRenderer.startColor = Color.black;
			lineRenderer.endColor = Color.green;
			lineRenderer.startWidth = thickness;
			lineRenderer.endWidth = thickness;
			lineRenderer.sortingOrder = 10;

			// draw line
			float radian = currState.rotation * Mathf.Deg2Rad;
			Vector2 direction = new Vector2(Mathf.Sin(radian), Mathf.Cos(radian));
			Vector2 end = currState.position + direction * length;

			lineRenderer.SetPosition(0, currState.position);
			lineRenderer.SetPosition(1, end);

			currState.position = end;
		}

		public void rot(float angle)
		{
			currState.rotation += angle;
		}

		public void L(int v)
		{
			
		}

		public int randrange(int min, int max)
		{
			return UnityEngine.Random.Range(min, max);
		}

		public void ParseConstant(char constant)
		{
			switch (constant)
			{
				case '[':
					stateStack.Push(currState);
					currState = new State() { position = currState.position, rotation = currState.rotation, trunck = currState.trunck };
					break;
				case ']':
					currState = stateStack.Pop();
					break;
				default:
					throw new Exception($"Unknown constant: {constant}");
			}
		}
	}
}
