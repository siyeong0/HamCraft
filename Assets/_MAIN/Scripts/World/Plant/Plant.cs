using UnityEngine;
using LindenmayerSystem;
using System.Collections.Generic;

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

	//List<(string, string)> rules = new List<(string, string)>
	//	{
	//		("L(g)",		"fwd(1,1)" +
	//						"[rot(randrange(-50,-40))L(g)]" +
	//						"rot(randrange(40,50))L(g)"),
	//		("fwd(a,b)",    "fwd(a,b)fwd(a,b)")
	//	};

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
