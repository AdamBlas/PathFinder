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
		description = "Dijkstra - Dijkstra's algorithm is not actually a heuristic, but it works as an A* algorithm with specific heuristic.\nThe best node is the one with shortest path to get to. Diagonal distance is calulated using Pythagorean theorem.";
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
		
		// Get root of the sum of their pows
		float cost = sqrts[(xOffset * xOffset) + (yOffset * yOffset)];
		
		// Add that cost to the node's cost
		node.baseCost = node.parentNode.baseCost + cost;
			
		// Apply goal bounding
		ApplyGoalBounding(node);
	}
}
