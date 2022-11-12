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
	/// <param name="x"> X coordinate of the node </param>
	/// <param name="y"> Y coordinate of the node </param>
	/// <param name="parentNode"> Parent node </param>
	/// <returns> Node's base cost </returns>
	public override float GetPassageCost(int x, int y, Node parentNode)
	{
		// Calculate absolute values of the node-parent offsets
		int xOffset = Mathf.Abs(x - parentNode.x);
		int yOffset = Mathf.Abs(y - parentNode.y);
		
		// Get base cost
		return xOffset + yOffset;
	}
}
