using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Heuristic
{
	[Tooltip("Name of the heuristic")]
	public string name;
	
	[Tooltip("Description of the heuristic")]
	public string description;
	
	
	
	
	
	/// <summary>
	/// Calculates node's cost
	/// </summary>
	/// <param name="node"> Node to calculate cost </param>
	public abstract void CalculateCost(Node node);
}
