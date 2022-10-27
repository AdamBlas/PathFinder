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
		description = "Dijkstra - Dijkstra's algorithm is not actually a heuristic, but it works as an A* algorithm with specific heuristic.\nThe best node is the one with shortest path to get to. Diagonal distance is calulated using Pythagoras theorem.";
	}
	
	/// <summary>
	/// Calculates cost of the node based on distance to the start node
	/// </summary>
	/// <param name="node"> Node which cost has to be calculated </param>
	public override void CalculateCost(Node node)
	{
		// Get offsets
		int xOffset = Mathf.Abs(node.x - node.parentNode.x);
		int yOffset = Mathf.Abs(node.y - node.parentNode.y);
		
		// If both of them are 1s, movement was diagonal
		// Add square root of two to cost
		if (xOffset == 1 && yOffset == 1)
			node.baseCost = node.parentNode.baseCost + 1.4142f;
		else
			node.baseCost = node.parentNode.baseCost + 1;
			
		// Apply goal bounding
		ApplyGoalBounding(node);
	}
}
