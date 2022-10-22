using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dijkstra : Heuristic
{
	public static Dijkstra Instance;
	
	
	
	
	/// <summary>
	/// Constructor
	/// </summary>
	public Dijkstra()
	{
		// Create singleton
		Instance = this;
		
		// Set name and description
		name = "Dijkstra";
		description = "Dijkstra - Dijkstra's algorithm is not actually a heuristic, but it works as an A* algorithm with specific heuristic.\nThe best node is the one closest to the start node. Distance is calulated using Pythagorean theorem.";
	}
	
	/// <summary>
	/// Calculates cost of the node based on distance to the start node
	/// </summary>
	/// <param name="node"></param>
	public override void CalculateCost(Node node)
	{
		// Calculate distance from the start ndoe
		// No need to calculate square root since comparing pows will give the same results
		node.cost = Mathf.Pow(node.x - StartGoalManager.startCol, 2) + Mathf.Pow(node.y - StartGoalManager.startRow, 2);
	}
}
