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
	
	[Tooltip("Node's cost")]
	public float cost;
	
	
	
	
	
	
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
		this.cost = cost;
	}
	
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="x"> X coord </param>
	/// <param name="y"> Y coord </param>
	/// <param name="parentNode"> Parent node </param>
	public Node(int x, int y, Node parentNode)
	{
		this.x = x;
		this.y = y;
		this.parentNode = parentNode;
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
		this.cost = cost;
		this.parentNode = parentNode;
	}
	
	/// <summary>
	/// IComparable<Node> interface implementation
	/// </summary>
	/// <param name="other"> Other node to compare with </param>
	/// <returns> -1, 0 or 1, depending on nodes' costs difference </returns>
	public int CompareTo(Node other)
	{
		if (cost == other.cost)
			return 0;
			
		if (cost > other.cost)
			return 1;
			
		return -1;
	}
}
