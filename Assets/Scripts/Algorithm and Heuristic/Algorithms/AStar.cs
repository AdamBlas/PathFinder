using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NodeType;

public class AStar : Algorithm
{
	/// <summary>
	/// Constructor
	/// </summary>
	public AStar(params Heuristic[] heuristics) : base(heuristics)
	{
		// Set name and description
		name = "A*";
		description = "A* - Algorithm selects the best node from the available pool and adds all adjecent nodes to the pool.\nIt guarantees to find a solution if such exists and that solution will be the best one possible.";
	}
	
	/// <summary>
	/// Solves current problem using A* algorithm
	/// </summary>
	/// <param name="heuristic"> Heuristic used to calculate node's cost </param>
	public override IEnumerator Solve(Heuristic heuristic)
	{
		// Create stopwatch object and start time measuring
		System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
		timer.Start();
		
		// Create open list and initialize it with start node
		Node node = new Node(StartGoalManager.startCol, StartGoalManager.startRow, 0, null);
		NodeSortedList list = new NodeSortedList(node);
		
		// Prepare array with information if node was visited before
		bool[,] wasNodeVisited = new bool[Map.width, Map.height];
		wasNodeVisited[StartGoalManager.startCol, StartGoalManager.startRow] = true;
		
		// Prepare variables to store result
		int nodesToAnalyze = 0;
		int nodesAnalyzed = 0;
		bool pathFound = false;
		string resultMessage = "Path not found";
		
		// Prepare variables that will hold new nodes' coordinates
		int x, y;

		// This counter will secure loop to run infinitely
		int securityCounter = 0;
		int mapSize = Map.width * Map.height;

		// Start loop
		while (true)
		{
			// Check security counter
			if (securityCounter++ >= mapSize)
			{
				Debug.LogWarning("WARNING: Solving loop stopped for security reasons");
				resultMessage = "Maximum loop iterations reached.";
				break;
			}

			// Check if there is node to analyze
			if (list.count == 0)
				break;
			
			// Get best node from list
			node = list.PopAtZero();
			nodesAnalyzed++;
			
			// Check if this is goal node
			if (node.x == StartGoalManager.goalCol && node.y == StartGoalManager.goalRow)
			{
				pathFound = true;
				resultMessage = "Path was found";
				break;
			}
			
			// Check if we can go in given direction
			bool canGoUp = node.y != 0;
			bool canGoRight = node.x != Map.width - 1;
			bool canGoDown = node.y != Map.height - 1;
			bool canGoLeft = node.x != 0;
			
			// Expand the node in every direction
			
			// Check NORTH DIRECTIONS
			if (canGoUp)
			{
				// Y coordoinate will be common for all three directions, so we can calculate it beforehand
				y = node.y - 1;
				
				// Check NORTH-WEST
				if (canGoLeft)
				{
					// Calculate X coordinate
					x = node.x - 1;

					// If can expand in that direction, do it
					if (wasNodeVisited[x, y] == false && Map.map[y, x] == FREE)
					{
						list.Add(new Node(x, y, node, heuristic));
						wasNodeVisited[x, y] = true;
						nodesToAnalyze++;
						Displayer.Instance.PaintPath(x, y, Displayer.Instance.toAnalyzeColor);
					}
				}
				
				// Check NORTH
				// If can expand in that direction, do it
				if (wasNodeVisited[node.x, y] == false && Map.map[y, node.x] == FREE)
				{
					list.Add(new Node(node.x, y, node, heuristic));
					wasNodeVisited[node.x, y] = true;
					nodesToAnalyze++;
					Displayer.Instance.PaintPath(node.x, y, Displayer.Instance.toAnalyzeColor);
				}
				
				// Check NORTH-EAST
				if (canGoRight)
				{
					// Calculate X coordinate
					x = node.x + 1;
					
					// If can expand in that direction, do it
					if (wasNodeVisited[x, y] == false && Map.map[y, x] == FREE)
					{
						list.Add(new Node(x, y, node, heuristic));
						wasNodeVisited[x, y] = true;
						nodesToAnalyze++;
						Displayer.Instance.PaintPath(x, y, Displayer.Instance.toAnalyzeColor);
					}
				}
			}
			
			// Check EAST
			if (canGoRight)
			{
				// Calculate X coordinate
				x = node.x + 1;
				
				// If can expand in that direction, do it
				if (wasNodeVisited[x, node.y] == false && Map.map[node.y, x] == FREE)
				{
					list.Add(new Node(x, node.y, node, heuristic));
					wasNodeVisited[x, node.y] = true;
					nodesToAnalyze++;
					Displayer.Instance.PaintPath(x, node.y, Displayer.Instance.toAnalyzeColor);
				}
			}
			
			// Chceck SOUTH DIRECTIONS
			if (canGoDown)
			{
				// Y coordoinate will be common for all three directions, so we can calculate it beforehand
				y = node.y + 1;
				
				// Check SOUTH-EAST
				if (canGoRight)
				{
					// Calculate X coordinate
					x = node.x + 1;
					
					// If can expand in that direction, do it
					if (wasNodeVisited[x, y] == false && Map.map[y, x] == FREE)
					{
						list.Add(new Node(x, y, node, heuristic));
						wasNodeVisited[x, y] = true;
						nodesToAnalyze++;
						Displayer.Instance.PaintPath(x, y, Displayer.Instance.toAnalyzeColor);
					}
				}
				
				// Check SOUTH
				// If can expand in that direction, do it
				if (wasNodeVisited[node.x, y] == false && Map.map[y, node.x] == FREE)
				{
					list.Add(new Node(node.x, y, node, heuristic));
					wasNodeVisited[node.x, y] = true;
					nodesToAnalyze++;
					Displayer.Instance.PaintPath(node.x, y, Displayer.Instance.toAnalyzeColor);
				}
				
				// Check SOUTH-WEST
				if (canGoLeft)
				{
					// Calculate X coordinate
					x = node.x - 1;
					
					// If can expand in that direction, do it
					if (wasNodeVisited[x, y] == false && Map.map[y, x] == FREE)
					{
						list.Add(new Node(x, y, node, heuristic));
						wasNodeVisited[x, y] = true;
						nodesToAnalyze++;
						Displayer.Instance.PaintPath(x, y, Displayer.Instance.toAnalyzeColor);
					}
				}
			}
			
			// Check WEST
			if (canGoLeft)
			{
				// Calculate X coordinate
				x = node.x - 1;
				
				// If can expand in that direction, do it
				if (wasNodeVisited[x, node.y] == false && Map.map[node.y, x] == FREE)
				{
					list.Add(new Node(x, node.y, node, heuristic));
					wasNodeVisited[x, node.y] = true;
					nodesToAnalyze++;
					Displayer.Instance.PaintPath(x, node.y, Displayer.Instance.toAnalyzeColor);
				}
			}
			
			// Expanded in all possible directions
			// Sort list to put newly added nodes in order
			list.Sort();
			
			// Update displayer
			Displayer.Instance.PaintPath(node.x, node.y, Displayer.Instance.analyzedColor);
			
			// If solver is supposed to be animated, pause it for some time
			// Pause only every 10 iterations, as doing it every frame will be slow, even with skipping only one frame
			if (Solver.animateSolvingProcess && securityCounter % 10 == 0)
			{
				// Pause timer
				timer.Stop();
				
				// Pause program for a short duration
				yield return new WaitForSeconds(Solver.delay);
				
				// Resume timer
				timer.Start();
			}
		}
		
		// Stop the timer
		timer.Stop();
		
		// Loop has ended, either path was found or all nodes were analyzed and no path was found
		if (pathFound == false)
		{
			// Path was not found
			// Display proper message and end method
			ResultDisplayer.SetText(1, "FAILURE\n" + resultMessage);
			yield break;
		}
		
		// Path was found
		
		// Display message
		ResultDisplayer.SetText(1, "SUCCESS\n" + resultMessage);
		
		// Paint all pixels on displayer and calculate path's length
		PrintPath(node, out int nodesLength, out float pathLength);
		
		// Display statistics
		ResultDisplayer.SetText(2, "TIME\n" + timer.Elapsed.TotalMilliseconds + " ms\n\nNODES\nTo Analyze:\t" + nodesToAnalyze + "\nAnalyzed:\t" + nodesAnalyzed);
		ResultDisplayer.SetText(3, "LENGTH\nNodes:\t" + nodesLength + "\nDistance:\t" + pathLength.ToString("f2"));
	}
}
