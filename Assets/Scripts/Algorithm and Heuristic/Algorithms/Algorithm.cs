using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class Algorithm
{
	[Tooltip("Name of the algorithm")]
	public string name;
	
	[Tooltip("Description of the algorithm")]
	public string description;
	
	[Tooltip("Heuristics available for this algorithm")]
	public Heuristic[] heuristics;
	
	[Tooltip("Array of visited nodes")]
	public Array2D<Node> nodesVisited;
	
	[Tooltip("List of nodes to analyze")]
	public NodeSortedList list;
	
	[Tooltip("Selected heursitic")]
	public Heuristic heuristic;
	
	[Tooltip("Amount of nodes analyzed")]
	public int nodesAnalyzed;
	
	[Tooltip("Name of the file that will contain statistics")]
	public string statsFileName;

	[Tooltip("Array of values to average")]
	protected List<float[]> statsToAverage = new List<float[]>();
	

	
	
	
	
	
	
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
	/// <param name="howManyTimes"> How many times algorithm has to solve problem before getting average results </param>
	/// <param name="howMuchToRemove"> How many worst records has to be deleted before averaging </param>
	public abstract IEnumerator Solve(Heuristic heuristic, int howManyTimes, int howMuchToRemove);
	
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
	
	/// <summary>
	/// Saves given stats for later to average them
	/// </summary>
	protected void AddValuesToAverage(params float[] values)
	{
		statsToAverage.Add(values);
	}
	
	/// <summary>
	/// Sorts values stored in statsToAverage, removes worst results and averages rest
	/// </summary>
	/// <param name="howMuchToRemove"> How much worst records has to be removed </param>
	/// <param name="indexToSortBy"> Index in the array to sort list by </param>
	/// <returns> Array of averaged values </returns>
	protected float[] AverageValues(int howMuchToRemove, int indexToSortBy)
	{
		// If there are no values to average, return null
		if (statsToAverage.Count == 0)
			return null;
		
		// Sort list using custom comparer
		statsToAverage = statsToAverage.OrderBy((arr) => arr[indexToSortBy]).ToList();

		// Remove the worst results
		if (howMuchToRemove < statsToAverage.Count)
			statsToAverage.RemoveRange(statsToAverage.Count - howMuchToRemove, howMuchToRemove);
		else
			Debug.LogWarning("WARNING: Not removing border values because there are not enough records");
		
		// Average the rest
		float[] avg = new float[statsToAverage[0].Length];
		
		// Sum stats
		foreach (var record in statsToAverage)
			for (int i = 0; i < record.Length; i++)
				avg[i] += record[i];

		// Average sums
		for (int i = 0; i < avg.Length; i++)
			avg[i] /= statsToAverage.Count;
			
		// Clear list for future operations
		statsToAverage.Clear();
		
		// Return averaged values
		return avg;
	}
	
	/// <summary>
	/// Saves data to CSV file
	/// </summary>
	/// <param name="data"> Data to save </param>
	protected void SaveToCsv(params object[] data)
	{
		// Check if file path was set
		if (string.IsNullOrWhiteSpace(statsFileName))
			return;
		
		// Check if directory exists
		System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(statsFileName));

		// Open .csv file to append data
		System.IO.File.AppendAllText(statsFileName, string.Join("\t", data) + "\n");
	}
}
