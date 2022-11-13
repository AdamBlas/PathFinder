using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Algorithm
{
	[Tooltip("Name of the algorithm")]
	public string name;
	
	[Tooltip("Description of the algorithm")]
	public string description;
	
	[Tooltip("Heuristics available for this algorithm")]
	public Heuristic[] heuristics;
	
	[Tooltip("Array of visited nodes")]
	public Node[,] nodesVisited;
	
	[Tooltip("List of nodes to analyze")]
	public NodeSortedList list;
	
	[Tooltip("Selected heursitic")]
	public Heuristic heuristic;
	
	[Tooltip("Amount of nodes analyzed")]
	public int nodesAnalyzed;
	
	[Tooltip("Amount of nodes to analyze")]
	public int nodesToAnalyze;
	
	[Tooltip("Nodes to reanalyze due to cost overwriting")]
	public int nodesToReanalyze;
	
	
	
	
	
	
	
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="heuristics"> Heuristics available for this algorithm </param>
	public Algorithm(params Heuristic[] heuristics)
	{
		this.heuristics = heuristics;
	}
	
	/// <summary>
	/// Solves algorithm
	/// </summary>
	/// <param name="heuristic"> Heuristic used to calculate node's value </param>
	public abstract IEnumerator Solve(Heuristic heuristic);
	
	
	/// <summary>
	/// Prints path from final node back to the goal node
	/// </summary>
	/// <param name="node"> Final node in the path </param>
	/// <param name="nodesAmount"> Out parameter, amount of nodes in the path </param>
	/// <param name="pathLength"> Out parameter, length of the path </param>
	protected void PrintPath(Node node, out int nodesAmount, out float pathLength)
	{
		// Paint all pixels on displayer and calculate path's length
		nodesAmount = 0;
		pathLength = 0;
		
		while (node.parentNode != null)
		{
			// Paint pixel
			Displayer.Instance.PaintPath(node.x, node.y, Displayer.Instance.pathColor);
			
			// Add distances
			nodesAmount += 1;
			pathLength += Mathf.Sqrt(Mathf.Pow(node.x - node.parentNode.x, 2) + Mathf.Pow(node.y - node.parentNode.y, 2));
			
			// Switch node to parent
			node = node.parentNode;
		}
	}
}
