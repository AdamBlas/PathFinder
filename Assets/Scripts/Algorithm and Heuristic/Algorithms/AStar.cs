using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NodeType;

public class AStar : Algorithm
{
	[Tooltip("Singleton")]
	public static AStar Instance;
	
	
	
	
	/// <summary>
	/// Constructor
	/// </summary>
	public AStar(params Heuristic[] heuristics) : base(heuristics)
	{
		// Create singleton
		Instance = this;
		
		// Set name and description
		name = "A*";
		description = "A* - Algorithm selects the best node from the available pool and adds all adjecent nodes to the pool.\nIt guarantees to find a solution if such exists and that solution will be the best one possible.";
	}
	
	/// <summary>
	/// Try to expand in a given direction
	/// </summary>
	/// <param name="x"> X coordinate to expand </param>
	/// <param name="y"> Y coordinate to expand </param>
	/// <param name="node"> Parent node </param>
	public void TryToExpand(int x, int y, Node node)
	{
		// Check diagonal condition (node can expand diagonally only if movement in both cardinal directions for that diagonal is possible)
		if (node.x - x != 0 && node.y - y != 0)
		{
			// Movement is diagonal, check if movement in cardinals is possible
			if (Map.map[node.y, x] != FREE || Map.map[y, node.x] != FREE)
			{
				// Movement in cardinals is not possible
				return;
			}
		}
		
		// If can expand in that direction, do it
		if (nodesVisited[x, y] != null)
		{
			// This node was visited, check if there is a need to overwrite cost
			if (CostOverwriteManager.shouldOverwrite)
			{
				// Calculate new costs
				heuristic.GetCosts(x, y, node, out float baseCost, out float goalBoundCost);

				// If new goal bounded cost is lower than previous one, overwrite old values
				if (baseCost * CostOverwriteManager.errorMargin < nodesVisited[x, y].baseCost)
				{
					// Overwrite costs
					nodesVisited[x, y].baseCost = baseCost;
					nodesVisited[x, y].goalBoundCost = goalBoundCost;
					
					// Overwrite parent
					nodesVisited[x, y].parentNode = node;
					
					// Add that node to list once again
					list.Add(nodesVisited[x, y]);
				}
			}
		}
		else if (Map.map[y, x] == FREE)
		{
			// Node wasn't visited before, create new one and add it to list
			Node newNode = new Node(x, y, node, heuristic);
			list.Add(newNode);
						
			// Mark spot as visited
			nodesVisited[x, y] = newNode;
						
			// Paint pixel
			Displayer.Instance.PaintPath(x, y, Displayer.Instance.toAnalyzeColor);
		}
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
		
		// Save heuristic
		this.heuristic = heuristic;
		
		// Create open list and initialize it with start node
		Node node = new Node(StartGoalManager.startCol, StartGoalManager.startRow, 0, null);
		list = new NodeSortedList(node);
		
		// Prepare array with information of previously visited nodes
		nodesVisited = new Node[Map.width, Map.height];
		nodesVisited[StartGoalManager.startCol, StartGoalManager.startRow] = node;
		
		// Prepare variables to store result
		nodesAnalyzed = 0;
		bool pathFound = false;
		string resultMessage = "Path not found";
		
		// Prepare variable that will hold new nodes' coordinates
		int y;

		// This counter will secure loop from running infinitely
		int securityCounter = 0;
		int mapSize = Map.width * Map.height * 2;

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
					TryToExpand(node.x - 1, y, node);
				
				// Check NORTH
				TryToExpand(node.x, y, node);
				
				// Check NORTH-EAST
				if (canGoRight)
					TryToExpand(node.x + 1, y, node);
			}
			
			// Check EAST
			if (canGoRight)
				TryToExpand(node.x + 1, node.y, node);
			
			// Chceck SOUTH DIRECTIONS
			if (canGoDown)
			{
				// Y coordoinate will be common for all three directions, so we can calculate it beforehand
				y = node.y + 1;
				
				// Check SOUTH-EAST
				if (canGoRight)
					TryToExpand(node.x + 1, y, node);
					
				// Check SOUTH
				TryToExpand(node.x, y, node);

				// Check SOUTH-WEST
				if (canGoLeft)
					TryToExpand(node.x - 1, y, node);
			}
			
			// Check WEST
			if (canGoLeft)
				TryToExpand(node.x - 1, node.y, node);
			
			// Expanded in all possible directions
			// Sort list to put newly added nodes in order
			list.Sort();
			
			// Update displayer
			Displayer.Instance.PaintPath(node.x, node.y, Displayer.Instance.analyzedColor);
			
			// If solver is supposed to be animated, pause it for some time
			if (Solver.animateSolvingProcess)
			{
				// If delay is small enough, animate only every 10th frame, otherwise even one frame delay will take long
				if (Solver.delay > 0.2f || securityCounter % 10 == 0)
				{
					// Pause timer
					timer.Stop();
					
					// Pause program for a short duration
					yield return new WaitForSeconds(Solver.delay);
					
					// Resume timer
					timer.Start();
				}
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
			ResultDisplayer.SetText(2, string.Empty);
			ResultDisplayer.SetText(3, string.Empty);
			yield break;
		}
		// Path was found
		
		// Paint all pixels on displayer and calculate path's length
		PrintPath(node, out int nodesLength, out float pathLength);
		
		// Display message and statistics
		
		// Prepare message for displayer 1
		string msg1 = "TIME";
		msg1 += "\n" + timer.Elapsed.TotalMilliseconds + " ms";
		msg1 += "\n\nPATH LENGTH";
		msg1 += "\nNodes:\t" + nodesLength;
		msg1 += "\nDistance:\t" + pathLength.ToString("f2");
		
		// Prepare message for displayer 2
		int nodesAllocated = Displayer.GetAmountOfNodesAllocated();
		string msg2 = "NODES AMOUNT";
		msg2 +=	"\nAnalyzed:\t\t" + nodesAnalyzed;
		msg2 += "\nAllocated:\t\t" + nodesAllocated;

		// Prepare message for displayer 3
		string msg3 = "MEMORY USAGE";
		msg3 += "\nUsage per node:\t" + Node.MEMORY_USAGE + " B";
		msg3 += "\nTotal memory:\t" + (nodesAllocated * Node.MEMORY_USAGE) + " B";
		
		// Display prepared messages
		ResultDisplayer.SetText(1, msg1);
		ResultDisplayer.SetText(2, msg2);
		ResultDisplayer.SetText(3, msg3);
	}
}
