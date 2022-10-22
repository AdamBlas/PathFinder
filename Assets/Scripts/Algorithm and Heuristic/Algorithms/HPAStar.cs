using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPAStar : Algorithm
{
	/// <summary>
	/// Constructor
	/// </summary>
	public HPAStar(params Heuristic[] heuristics) : base(heuristics)
	{
		// Set name and description
		name = "HPA*";
		description = "HPA* (Hierarchical PathFinding A*) - Algorithm splits map into smaller chunks, uses A* to find path from chunk with start node to chunk with goal node and then uses A* to find path only inside chunks in path.";
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
