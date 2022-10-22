using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Algorithm
{
	[Tooltip("Name of the algorithm")]
	public string name;
	
	[Tooltip("Description of the algorithm")]
	public string description;
	
	[Tooltip("Heuristics available for this algorithm")]
	public Heuristic[] heuristics;
	
	
	
	
	
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="heuristics"> Heuristics available for this algorithm </param>
	public Algorithm(params Heuristic[] heuristics)
	{
		this.heuristics = heuristics;
	}
	
	/// <summary>
	/// Solves algorithm
	/// </summary>
	/// <param name="heuristic"> Heuristic used to calculate node's value </param>
	public abstract IEnumerator Solve(Heuristic heuristic);
}
