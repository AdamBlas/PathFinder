using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceToGoal : Heuristic
{
	public static DistanceToGoal Instance;
	
	
	
	
	/// <summary>
	/// Constructor
	/// </summary>
	public DistanceToGoal()
	{
		// Create singleton
		Instance = this;
		
		// Set name and description
		name = "Distance To Goal";
		description = "Distance To Goal - Original heuristic.\nThe best node is the one closest to the goal node. Distance is calculated using Pythagorean theorem.";
	}
	
	/// <summary>
	/// Calculates cost of the node based on distance to the start node
	/// </summary>
	/// <param name="node"> Node which cost has to be calculated </param>
	public override void CalculateCost(Node node)
	{
		// Calculate distance to the goal ndoe
		// No need to calculate square root since comparing pows will give the same results
		node.baseCost = Mathf.Pow(node.x - StartGoalManager.goalCol, 2) + Mathf.Pow(node.y - StartGoalManager.goalRow, 2);
	
		// Apply goal bounding
		ApplyGoalBounding(node);
	}
}
