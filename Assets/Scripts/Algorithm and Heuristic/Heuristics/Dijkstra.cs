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
	/// <param name="x"> X coordinate of the node </param>
	/// <param name="y"> Y coordinate of the node </param>
	/// <param name="parentNode"> Parent node </param>
	/// <returns> Node's base cost </returns>
	public override float GetPassageCost(int x, int y, Node parentNode)
	{
		// Get offsets
		int xOffset = Mathf.Abs(x - parentNode.x);
		int yOffset = Mathf.Abs(y - parentNode.y);
		
		// Return root of the sum of their pows
		int pow = (xOffset * xOffset) + (yOffset * yOffset);
		
		// If lookup array contains that index, use it
		if (pow < sqrts.Length)
			return sqrts[pow];
			
		// Otherwise, caluclate it
		return Mathf.Sqrt(pow);
	}
}
