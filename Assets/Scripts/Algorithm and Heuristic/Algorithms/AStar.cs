using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : Algorithm
{
	/// <summary>
	/// Constructor
	/// </summary>
	public AStar(params Heuristic[] heuristics) : base(heuristics)
	{
		// Set name and description
		name = "A*";
		description = "A* - Algorithm selects the best node from the available pool and adds all adjecent nodes to the pool.\nIt guarantees to find a solution if such exists and that solution will be the best one possible.";
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
