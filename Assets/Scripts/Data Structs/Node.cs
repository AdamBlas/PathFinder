using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : System.IComparable<Node>
{
	[Tooltip("X coord of the node")]
	public int x;
	
	[Tooltip("Y coord of the node")]
	public int y;
	
	[Tooltip("Parent node - previous one in the path")]
	public Node parentNode;
	
	[Tooltip("Node's cost before applying goal bounding")]
	public float baseCost;
	
	[Tooltip("Node's cost after applying goal bounding")]
	public float goalBoundCost;
	
	
	
	
	
	
	
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="x"> X coord </param>
	/// <param name="y"> Y coord </param>
	/// <param name="cost"> Node's cost </param>
	public Node(int x, int y, float cost)
	{
		this.x = x;
		this.y = y;
		this.baseCost = cost;
		this.goalBoundCost = cost;
	}
	
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="x"> X coord </param>
	/// <param name="y"> Y coord </param>
	/// <param name="parentNode"> Parent node </param>
	/// <param name="heuristic"> Heuristic thta will be used to calculate cost </param>
	public Node(int x, int y, Node parentNode, Heuristic heuristic)
	{
		this.x = x;
		this.y = y;
		this.parentNode = parentNode;
		
		// Calculate node's cost
		heuristic.CalculateCost(this);
	}
	
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="x"> X coord </param>
	/// <param name="y"> Y coord </param>
	/// <param name="cost"> Node's cost </param>
	/// <param name="parentNode"> Parent node </param>
	public Node(int x, int y, float cost, Node parentNode)
	{
		this.x = x;
		this.y = y;
		this.baseCost = cost;
		this.goalBoundCost = cost;
		this.parentNode = parentNode;
	}
	
	/// <summary>
	/// IComparable<Node> interface implementation
	/// </summary>
	/// <param name="other"> Other node to compare with </param>
	/// <returns> -1, 0 or 1, depending on nodes' costs difference </returns>
	public int CompareTo(Node other)
	{
		if (goalBoundCost == other.goalBoundCost)
			return 0;
			
		if (goalBoundCost > other.goalBoundCost)
			return 1;
			
		return -1;
	}
	
	public override string ToString()
	{
		return "X=" + x + ", Y=" + y + ", Base cost=" + baseCost + ", GoalBound cost=" + goalBoundCost + ", Parent={ " + (parentNode == null ? "NULL" : ("X=" + parentNode.x + ", Y=" + parentNode.y)) + " }";
	}
}
