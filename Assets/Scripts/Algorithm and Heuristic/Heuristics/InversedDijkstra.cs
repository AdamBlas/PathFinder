using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InversedDijkstra : Heuristic
{
	public static InversedDijkstra Instance;
	
	
	
	
	/// <summary>
	/// Constructor
	/// </summary>
	public InversedDijkstra()
	{
		// Create singleton
		Instance = this;
		
		// Set name and description
		name = "Inversed Dijkstra";
		description = "Inversed Dijkstra - Original heuristic.\nThe best node is the one closest to the goal node. Distance is calculated using Pythagorean theorem.";
	}
	
	/// <summary>
	/// Calculates cost of the node based on distance to the start node
	/// </summary>
	/// <param name="node"></param>
	public override void CalculateCost(Node node)
	{
		// Calculate distance from the start ndoe
		// No need to calculate square root since comparing pows will give the same results
		node.cost = Mathf.Pow(node.x - StartGoalManager.goalCol, 2) + Mathf.Pow(node.y - StartGoalManager.goalRow, 2);
	}
}
