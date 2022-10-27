using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manhattan : Heuristic
{
	public static Manhattan Instance;
	
	
	
	
	/// <summary>
	/// Constructor
	/// </summary>
	public Manhattan()
	{
		// Create singleton
		Instance = this;
		
		// Set name and description
		name = "Manhattan";
		description = "Manhattan - Cost is equal to sum of the distances from the start node to the current node in the X axis and the Y axis.";
	}
	
	/// <summary>
	/// Calculates cost of the node based on distance to the start node
	/// </summary>
	/// <param name="node"> Node which cost has to be calculated </param>
	public override void CalculateCost(Node node)
	{
		// Calculate absolute values of the offsets
		int xOffset = Mathf.Abs(node.x - StartGoalManager.startCol);
		int yOffset = Mathf.Abs(node.y - StartGoalManager.startRow);
		
		// Save sum
		node.baseCost = xOffset + yOffset;
		
		// Apply goal bounding
		ApplyGoalBounding(node);
	}
}
