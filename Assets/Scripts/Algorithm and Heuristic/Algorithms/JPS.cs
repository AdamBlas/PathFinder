using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JPS : Algorithm
{
	/// <summary>
	/// Constructor
	/// </summary>
	public JPS(params Heuristic[] heuristics) : base(heuristics)
	{
		// Set name and description
		name = "JPS";
		description = "JPS (Jump Point Search) - Algorithm first precalculates map creating data structure containing information of points that can be part of the path and then uses it it find the final path.";
	}
	
	/// <summary>
	/// Solves current problem using A* algorithm
	/// </summary>
	/// <param name="heuristic"> Heuristic used to calculate node's cost </param>
	public override IEnumerator Solve(Heuristic heuristic)
	{
		throw new System.NotImplementedException();
	}
}
