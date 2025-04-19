using LindenmayerSystem;
using System.Collections.Generic;
using UnityEngine;

namespace HamCraft
{
	public class Leaf : MonoBehaviour
	{
		[SerializeField] int iterations;

		string[] variables = new string[] { "X", "fwd", "rot" };
		string[] functions = new string[] { };
		char[] constants = new char[] { '[', ']' };
		string axiom = "X";
		//List<(string, string)> rules = new List<(string, string)>
		//	{
		//		("X", "fwd(5,1)[rot(25)X][rot(-25)X][rot(50)X][rot(-50)X]fwd(5,1)X"),
		//		("fwd(5,1)", "fwd(5,1)fwd(5,1)")
		//	};
		List<(string, string)> rules = new List<(string, string)>
		{
			("X", "fwd(5,1)[rot(25)X]fwd(5,1)[rot(-25)X]fwd(5,1)X"),
			("fwd(5,1)", "fwd(5,1)fwd(5,1)")
		};

		LSystem lsys;

		int prevIterations;

		void Start()
		{
			lsys = new LSystem(variables, functions, constants, new LindenmayerSystem.Behavior.DefaultBehavior(transform));

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
	}
}