﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChunkSizeManager;
using static NodeType;

public class HPAStar : Algorithm
{
	/// <summary>
	/// Class represents a single chunk
	/// </summary>
	class Chunk : Node
	{
		[Tooltip("Parent chunk in chunk path")]
		public Chunk parentChunk;
		
		// Flags that indicate whether or not you can go from this chunk in given direction
		public bool? canGoUp = null;
		public bool? canGoDown = null;
		public bool? canGoLeft = null;
		public bool? canGoRight = null;
		public bool? canGoUpLeft = null;
		public bool? canGoUpRight = null;
		public bool? canGoDownLeft = null;
		public bool? canGoDownRight = null;
		
		
		
		
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="x"> X coordinate of the chunk. Must be multiple of chunk size. </param>
		/// <param name="y"> Y coordinate of the chunk. Must be multiple of chunk size. </param>
		/// <param name="cost"> Cost of the chunk </param>
		/// <param name="parentChunk"> Previous chunk in the path </param>
		public Chunk(int x, int y, float cost, Chunk parentChunk) : base(x, y, cost, parentChunk) { }
		
		public override string ToString()
		{
			return "X=" + x + ", Y=" + y + ", GridX=" + (int)(x / ChunkSize) + ", GridY=" + (int)(y / ChunkSize) + ", Cost=" + goalBoundCost + ", N=" + canGoUp.Value + ", NE=" + canGoUpRight.Value + ", E=" + canGoRight.Value + ", SE=" + canGoDownRight.Value + ", S=" + canGoDown.Value + ", SW=" + canGoDownLeft.Value + ", W=" + canGoLeft.Value + ", NW=" + canGoUpLeft.Value;
		}
	}
	
	
	// Variables that will store the values that has to be passed from Coroutines (can't use ref or out)
	
	[Tooltip("Final chunk in the chunk path")]
	Chunk finalChunk;
	
	[Tooltip("Chunks grid")]
	Chunk[,] grid;
	
	[Tooltip("Amount of chunks to analyze by algorithm")]
	int amountOfChunksToAnalyze;
	
	[Tooltip("Amount of chunks analyzed by algorithm")]
	int amountOfChunksAnalyzed;
	
	[Tooltip("Amount of chunks with overwritten cost and reanalyzed")]
	int amountOfChunksReanalyzed;
	
	[Tooltip("Time required for finding path inside chunk grid")]
	float timeToFindPathInChunkGrid;
	
	[Tooltip("Final node in the node path. Since path si searched inside chunks, this node actually equals to start coordinates.")]
	Node finalNode;
	 
	[Tooltip("Amount of nodes analyzed by algorithm")]
	int amountOfNodesAnalyzed;
	
	[Tooltip("Time required for finding path inside chunks")]
	float timeToFindFinalPath;
	
	[Tooltip("Array of already visited chunks")]
	Array2D<Chunk> visitedChunks;
	
	
	
	
	
	
	/// <summary>
	/// Constructor
	/// </summary>
	public HPAStar(params Heuristic[] heuristics) : base(heuristics)
	{
		// Set name and description
		name = "HPA*";
		description = "HPA* (Hierarchical PathFinding A*) - Algorithm splits map into smaller chunks, uses A* to find path from chunk with start node to chunk with goal node and then uses A* to find path only inside chunks in path.";
	}
	
	/// <summary>
	/// Solves current problem using A* algorithm
	/// </summary>
	/// <param name="heuristic"> Heuristic used to calculate node's cost </param>
	public override IEnumerator Solve(Heuristic heuristic)
	{
		// Prepare timer object
		System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
		timer.Start();
		
		// Create chunk grid
		grid = CreateGrid(out int width, out int height);

		// Get precalculation time
		timer.Stop();		
		float precalculation = (float)timer.Elapsed.TotalMilliseconds;
		timer.Reset();
		
		// Save grid to file
		PrintChunkMap(grid, width, height);
		
		// Find path in this grid
		yield return ApplyAStartOnGrid(grid, width, height, heuristic);
		
		// Check if path was found
		if (finalChunk == null)
		{
			ResultDisplayer.SetText(1, "FAILURE:\nPath not found");
			ResultDisplayer.SetText(2, "FAILURE:\nNo path was found using chunk grid");
			ResultDisplayer.SetText(3, string.Empty);
			yield break;
		}
				
		// Paint all chunks that connect start and goal node
		Chunk chunk = finalChunk;
		int amountOfChunksInPath = 0;
		while (chunk != null)
		{
			// Paint chunk
			PaintChunk(chunk);
			
			// Increase counter
			amountOfChunksInPath++;
			
			// Go to the next chunk
			chunk = (Chunk)chunk.parentNode;
		}

		// Apply A* on every chunk in the path
		yield return ApplyAStarOnChunksList(finalChunk, heuristic);
		
		// Check if path was found
		if (finalNode == null)
		{
			ResultDisplayer.SetText(1, "FAILURE\nPath not found");
			ResultDisplayer.SetText(2, "FAILURE\nNo path was found inside the chunk");
			ResultDisplayer.SetText(3, string.Empty);
			yield break;
		}
		
		// Paint all pixels on displayer and calculate path's length
		PrintPath(finalNode, out int nodesLength, out float pathLength);
		
		// Print statistics
		ResultDisplayer.SetText(1, "SUCCESS\nPath was found!");
		ResultDisplayer.SetText(2, "TIME\nPrecalc:\t" + precalculation + " ms\nChunks:\t" + timeToFindPathInChunkGrid + " ms\nNodes:\t" + timeToFindFinalPath + " ms\nTotal:\t\t" + (precalculation + timeToFindPathInChunkGrid + timeToFindFinalPath) + " ms");
		ResultDisplayer.SetText(3, "MEMORY\nChunks:\n\tAnalyzed:\t" + amountOfChunksAnalyzed + "\n\tIn Path:\t" + amountOfChunksInPath + "\nNodes:\n\tAnazlyzed:\t" + amountOfNodesAnalyzed + "\n\tIn Path:\t" + nodesLength + "\nPath length:\t" + pathLength);
	}
	
	/// <summary>
	/// Applies A* on every chunk in the hierarchy
	/// </summary>
	/// <param name="finalChunk"> Final chunk in the path </param>
	/// <param name="heuristic"> heuristic used to calculate nodes' costs </param>
	IEnumerator ApplyAStarOnChunksList(Chunk finalChunk, Heuristic heuristic)
	{
		// This A* is inversed, we go from goal to start
		
		// Create and start timer
		var timer = new System.Diagnostics.Stopwatch();
		timer.Start();
		
		// Get goal node
		Node node = new Node(StartGoalManager.goalCol, StartGoalManager.goalRow, 0, null);
		
		// Prepare chunk variable
		Chunk chunk = finalChunk;
		
		// Prepare result variable
		amountOfNodesAnalyzed = 0;
		
		// Iterate as long as there are chunks in list
		while (chunk != null)
		{
			// Debug.Log("Analyzing chunk " + chunk);
			
			// Prepare array with info of whether or not node was visited
			Array2D<bool> wasNodeVisited = new Array2D<bool>(ChunkSize, ChunkSize);
			wasNodeVisited[node.x % ChunkSize, node.y % ChunkSize] = true;
			
			// Prepare list with visited chunks
			NodeSortedList list = new NodeSortedList(node);

			// Prepare output variables
			bool pathFoundInsideChunk = false;

			// Iterate as long as there are nodes in list
			while (list.count != 0)
			{
				// Get node from the list
				node = list.PopAtZero();
				
				// Debug.Log("Analyzing node " + node);

				// Paint analyzed node
				// Displayer.Instance.PaintPath(node.x, node.y, Displayer.Instance.subAnalyzedColor);
				amountOfNodesAnalyzed++;

				// Get node's coords inside chunk
				int nodeChunkX = node.x - chunk.x;
				int nodeChunkY = node.y - chunk.y;

				// Check if node meets end conditions
				if (chunk.parentNode == null)
				{
					// This chunk contains start node, check if node coords mach start coords
					if (node.x == StartGoalManager.startCol && node.y == StartGoalManager.startRow)
					{
						// Extract elapsed time
						timer.Stop();
						timeToFindFinalPath = (float)timer.Elapsed.TotalMilliseconds;
			
						// Save final node
						finalNode = node;
	
						yield break;
					}
				}
				else
				{
					// Get the direction we want to move, this determines end condition
					int xOffset = chunk.x - chunk.parentNode.x;
					int yOffset = chunk.y - chunk.parentNode.y;
					
					if (xOffset == 0)
					{
						// Goal chunk in in the same column
						if (yOffset > 0)
						{
							// Goal chunk is below, node must be in bottom row
							if (nodeChunkY == 0)
							{
								// Row is correct, check if that node has connection to free node in parent chunk
								if (Map.map[node.y + 1, node.x] == FREE)
								{
									// Assign that neighbout node as new node to analyze
									node = new Node(node.x, node.y - 1, node, heuristic);
									
									// Set flag and exit loop
									pathFoundInsideChunk = true;
									break;
								}
							}
						}
						else
						{
							// Goal chunk is above, node must be in the top row
							if (nodeChunkY == ChunkSize - 1)
							{
								// Row is correct, check if that node has connection to free node in parent chunk
								if (Map.map[node.y - 1, node.x] == FREE)
								{
									// Assign that neighbout node as new node to analyze
									node = new Node(node.x, node.y + 1, node, heuristic);
									
									// Set flag and exit loop
									pathFoundInsideChunk = true;
									break;
								}
							}
						}
					}
					else if (yOffset == 0)
					{
						// Goal chunk is in the same row
						if (xOffset > 0)
						{
							// Goal chunk is on the left, node must be in the left column
							if (nodeChunkX == 0)
							{
								// Column is correct, check if that node has connection to free node in parent chunk
								if (Map.map[node.y, node.x - 1] == FREE)
								{
									// Assign that neighbout node as new node to analyze
									node = new Node(node.x - 1, node.y, node, heuristic);
									
									// Set flag and exit loop
									pathFoundInsideChunk = true;
									break;
								}
							}
						}
						else
						{
							// Goal chunk is on the right, node must be in the right column
							if (nodeChunkX == ChunkSize - 1)
							{
								// Column is correct, check if that node has connection to free node in parent chunk
								if (Map.map[node.y, node.x + 1] == FREE)
								{
									// Assign that neighbout node as new node to analyze
									node = new Node(node.x + 1, node.y, node, heuristic);
									
									// Set flag and exit loop
									pathFoundInsideChunk = true;
									break;
								}
							}
						}
					}
					else
					{
						// Goal chunk is on diagonal
						if (xOffset > 0)
						{
							// Goal chunk is on the left
							if (yOffset > 0)
							{
								// Goal chunk is in the SW
								if (nodeChunkX == 0 && nodeChunkY == 0)
								{
									// Node is correct
									// Assign that neighbout node as new node to analyze
									node = new Node(node.x - 1, node.y - 1, node, heuristic);
									
									// Set flag and exit loop
									pathFoundInsideChunk = true;
									break;
								}
							}
							else
							{
								// Goal chunk is in the NW
								if (nodeChunkX == 0 && nodeChunkY == ChunkSize - 1)
								{
									// Node is correct
									// Assign that neighbout node as new node to analyze
									node = new Node(node.x - 1, node.y + 1, node, heuristic);
									
									// Set flag and exit loop
									pathFoundInsideChunk = true;
									break;
								}
							}
						}
						else
						{
							// Goal chunk is on the right
							if (yOffset > 0)
							{
								// Goal chunk is in the SE
								if (nodeChunkX == ChunkSize - 1 && nodeChunkY == 0)
								{
									// Node is correct
									// Assign that neighbout node as new node to analyze
									node = new Node(node.x + 1, node.y - 1, node, heuristic);
									
									// Set flag and exit loop
									pathFoundInsideChunk = true;
									break;
								}
							}
							else
							{
								// Goal chunk is in the NE
								if (nodeChunkX == ChunkSize - 1 && nodeChunkY == ChunkSize - 1)
								{
									// Node is correct
									// Assign that neighbout node as new node to analyze
									node = new Node(node.x + 1, node.y + 1, node, heuristic);
									
									// Set flag and exit loop
									pathFoundInsideChunk = true;
									break;
								}
							}
						}
					}
				}
				
				// End conditions were not met, expand the node

				// Expand in NORTH directions if possible
				if (nodeChunkY != ChunkSize - 1)
				{
					// Try to expand in NW
					if (node.x != 0 && Map.map[node.y + 1, node.x - 1] == FREE)
					{
						// Node is inside map boundaries
						
						if (nodeChunkX != 0 && wasNodeVisited[nodeChunkX - 1, nodeChunkY + 1] == false)
						{
							// Create new node, add it to the list and mark spot as visited
							Node newNode = new Node(node.x - 1, node.y + 1, node, heuristic);
							list.Add(newNode);
							wasNodeVisited[nodeChunkX - 1, nodeChunkY + 1] = true;
						}
					}
					
					// Try to expand in N
					if (Map.map[node.y + 1, node.x] == FREE)
					{
						// Node is inside map boundaries
						
						if (wasNodeVisited[nodeChunkX, nodeChunkY + 1] == false)
						{
							// Create new node, add it to the list and mark spot as visited
							Node newNode = new Node(node.x, node.y + 1, node, heuristic);
							list.Add(newNode);
							wasNodeVisited[nodeChunkX, nodeChunkY + 1] = true;
						}
					}
					
					// Try to expand in NE
					if (node.x + 1 != Map.width && Map.map[node.y + 1, node.x + 1] == FREE)
					{
						// Node is inside map boundaries
						
						if (nodeChunkX != ChunkSize - 1 && wasNodeVisited[nodeChunkX + 1, nodeChunkY + 1] == false)
						{
							// Create new node, add it to the list and mark spot as visited
							Node newNode = new Node(node.x + 1, node.y + 1, node, heuristic);
							list.Add(newNode);
							wasNodeVisited[nodeChunkX + 1, nodeChunkY + 1] = true;
						}
					}
				}
				
				// Try to expand in E
				if (node.x + 1 != Map.width && Map.map[node.y, node.x + 1] == FREE)
				{
					if (nodeChunkX != ChunkSize - 1 && wasNodeVisited[nodeChunkX + 1, nodeChunkY] == false)
					{
						// Create new node, add it to the list and mark spot as visited
						Node newNode = new Node(node.x + 1, node.y, node, heuristic);
						list.Add(newNode);
						wasNodeVisited[nodeChunkX + 1, nodeChunkY] = true;
					}
				}
				
				// Expand in SOUTH directions if possible
				if (nodeChunkY != 0 && node.y != 0)
				{
					// Try to expand in SW
					if (node.x != 0 && Map.map[node.y - 1, node.x - 1] == FREE)
					{
						// Node is inside map boundaries
						
						if (nodeChunkX != 0 && wasNodeVisited[nodeChunkX - 1, nodeChunkY - 1] == false)
						{
							// Create new node, add it to the list and mark spot as visited
							Node newNode = new Node(node.x - 1, node.y - 1, node, heuristic);
							list.Add(newNode);
							wasNodeVisited[nodeChunkX - 1, nodeChunkY - 1] = true;
						}
					}
					
					// Try to expand in S
					if (Map.map[node.y - 1, node.x] == FREE)
					{
						// Node is inside map boundaries
						
						if (wasNodeVisited[nodeChunkX, nodeChunkY - 1] == false)
						{
							// Create new node, add it to the list and mark spot as visited
							Node newNode = new Node(node.x, node.y - 1, node, heuristic);
							list.Add(newNode);
							wasNodeVisited[nodeChunkX, nodeChunkY - 1] = true;
						}
					}
					
					// Try to expand in SE
					if (node.x + 1 != Map.width && Map.map[node.y - 1, node.x + 1] == FREE)
					{
						if (nodeChunkX != ChunkSize - 1 && wasNodeVisited[nodeChunkX + 1, nodeChunkY - 1] == false)
						{
							// Create new node, add it to the list and mark spot as visited
							Node newNode = new Node(node.x + 1, node.y - 1, node, heuristic);
							list.Add(newNode);
							wasNodeVisited[nodeChunkX + 1, nodeChunkY - 1] = true;
						}
					}
				}
				
				// Try to expand in W
				if (nodeChunkX != 0 && Map.map[node.y, node.x - 1] == FREE && wasNodeVisited[nodeChunkX - 1, nodeChunkY] == false)
				{
					// Create new node, add it to the list and mark spot as visited
					Node newNode = new Node(node.x - 1, node.y, node, heuristic);
					list.Add(newNode);
					wasNodeVisited[nodeChunkX - 1, nodeChunkY] = true;
				}
			
				// Node expanded in every possible direction, sort list and go to the next node
				list.Sort();
				
				// If solving process is supposed to be animated, do short delay
				if (Solver.animateSolvingProcess)
				{
					// Stop timer so that delay won't count towards total time
					timer.Stop();
					
					// Pause program
					yield return new WaitForSeconds(Solver.delay);
					
					// Turn timer on again
					timer.Start();
				}
			}

			// Check if path was found
			if (pathFoundInsideChunk == false)
			{
				// Path was not found, set proper messages and end method
				ResultDisplayer.SetText(1, "FAILURE\nPath not found");
				ResultDisplayer.SetText(2, "On the path there is a chunk without passage to the goal chunk.");
				ResultDisplayer.SetText(3, string.Empty);
				yield break;
			}
			
			// Path was found inside chunk, switch chunk
			chunk = (Chunk)chunk.parentNode;
		}
	}
	
	/// <summary>
	/// Tries to expand chunk in given direction
	/// </summary>
	void TryToExpandChunk(int gridX, int gridY, bool passageFlag, Chunk chunk)
	{
		// Check if can expand in that direction
		if (passageFlag == false)
			return;
			
		// Check if chunk was already visited
		if (visitedChunks[gridX, gridY] != null)
		{
			// Chunk was visited, check if cost overwrite should be applied
			if (CostOverwriteManager.shouldOverwrite)
			{
				// Calculate new costs for the chunk
				heuristic.GetCosts(gridX * ChunkSize, gridY * ChunkSize, chunk, out float baseCost, out float goalBoundCost);
				
				// If new cost is inside error margin, update it
				if (baseCost * CostOverwriteManager.errorMargin < visitedChunks[gridX, gridY].baseCost)
				{
					// Overwrite costs
					visitedChunks[gridX, gridY].baseCost = baseCost;
					visitedChunks[gridX, gridY].goalBoundCost = goalBoundCost;
					
					// Overwrite parent
					nodesVisited[gridX, gridY].parentNode = chunk;
					
					// Add that chunk to list once again
					list.Add(visitedChunks[gridX, gridY]);
					
					// Increase counter
					amountOfChunksReanalyzed++;
				}
			}
		}
		else
		{
			// Chunk wasn't visited before, add it to list
			list.Add(grid[gridX, gridY]);
			
			// Mark spot as visited
			visitedChunks[gridX, gridY] = grid[gridX, gridY];
			
			// Increase amount of chunks to analyze
			amountOfChunksToAnalyze++;
		}
	}
	
	/// <summary>
	/// Applies A* on chunks' grid
	/// </summary>
	/// <param name="grid"> Chunks' map array </param>
	/// <param name="width"> Width of the array </param>
	/// <param name="height"> Height of the array </param>
	/// <param name="heuristic"> Heuristic used to calculate chunk's cost </param>
	IEnumerator ApplyAStartOnGrid(Chunk[,] grid, int width, int height, Heuristic heuristic)
	{
		// Prepare and start timer
		var timer = new System.Diagnostics.Stopwatch();
		timer.Start();
		
		// Get coordinates of the chunk with start and goal node
		int startChunkX = StartGoalManager.startCol / ChunkSize;
		int startChunkY = StartGoalManager.startRow / ChunkSize;
		int goalChunkX = StartGoalManager.goalCol / ChunkSize;
		int goalChunkY = StartGoalManager.goalRow / ChunkSize;
		
		// From this point on, chunks will be treated as nodes for overall A*
		
		// Get start chunk
		Chunk chunk = grid[startChunkX, startChunkY];
		
		// Create and initialize list
		list = new NodeSortedList(chunk);
		
		// Prepare lookup table to check if chunk was already visited
		visitedChunks = new Array2D<Chunk>(width, height);
		visitedChunks[startChunkX, startChunkY] = chunk;
		
		// Prepare security counter
		int securityCounter = 0;
		int gridSize = width * height;
		
		// Prepare variable that will store result
		bool pathFound = false;
		amountOfChunksAnalyzed = 0;
		amountOfChunksReanalyzed = 0;
		
		// Start A* loop		
		while (list.count != 0)
		{
			// Check security counter
			if (securityCounter++ >= gridSize)
			{
				Debug.LogWarning("WARNING: Security counter reached for chunk A*");
				yield break;
			}
			
			// Get the best chunk
			chunk = (Chunk)list.PopAtZero();
			
			// Increase counter
			amountOfChunksAnalyzed++;
			
			// Paint that chunk
			PaintChunk(chunk, Displayer.Instance.analyzedColor);
			
			// Get chunk coords in chunk grid
			int gridX = (int)(chunk.x / ChunkSize);
			int gridY = (int)(chunk.y / ChunkSize);
			
			// Check if this chunk is the one containing goal node
			if (gridX == goalChunkX && gridY == goalChunkY)
			{
				// Goal found, set flag and exit loop
				pathFound = true;
				break;
			}
			
			// Add neighbour chunks to the list
			
			// Check NORTH neighbour
			TryToExpandChunk(gridX, gridY + 1, chunk.canGoUp.Value, chunk);
			
			// Check NORTH-EAST neighbour
			TryToExpandChunk(gridX + 1, gridY + 1, chunk.canGoUpRight.Value, chunk);
			
			// Check EAST
			TryToExpandChunk(gridX + 1, gridY, chunk.canGoRight.Value, chunk);
			
			// Check SOUTH-EAST
			TryToExpandChunk(gridX + 1, gridY - 1, chunk.canGoDownRight.Value, chunk);
			
			// Check SOUTH
			TryToExpandChunk(gridX, gridY - 1, chunk.canGoDown.Value, chunk);
			
			// Check SOUTH-WEST
			TryToExpandChunk(gridX - 1, gridY - 1, chunk.canGoDownLeft.Value, chunk);
			
			// Check WEST
			TryToExpandChunk(gridX - 1, gridY, chunk.canGoLeft.Value, chunk);
			
			// Check NORTH-WEST
			TryToExpandChunk(gridX - 1, gridY + 1, chunk.canGoUpLeft.Value, chunk);
			
			// Sort list to include all newly added chunks
			list.Sort();
			
			// If solving process is supposed to be animated, delay for a while
			if (Solver.animateSolvingProcess)
			{
				// Stop timer so that delay won't affect final score
				timer.Stop();
				
				// Pause for a little bit
				yield return new WaitForSeconds(Solver.delay);
				
				// Enable timer once again
				timer.Start();
			}
		}
		
		// Extract time elapsed value
		timeToFindPathInChunkGrid = (float)timer.Elapsed.TotalMilliseconds;
		
		// If path was found, save last chunk, otherwise, save null
		if (pathFound)
			finalChunk = chunk;
		else
			finalChunk = null;
	}
	
	/// <summary>
	/// Divides map into smaller chunks
	/// </summary>
	/// <returns> Chunk array </returns>
	Chunk[,] CreateGrid(out int width, out int height)
	{
		// Calculate size of the chunk array
		width = Mathf.CeilToInt((float)Map.width / ChunkSize);
		height = Mathf.CeilToInt((float)Map.height / ChunkSize);
		
		Chunk[,] grid = new Chunk[width, height];
		
		// Initialize chunks' coordinates
		for (int x = 0; x < width; x++)
			for (int y = 0; y < height; y++)
				grid[x, y] = new Chunk(x * ChunkSize, y * ChunkSize, 0, null);
		
		// Generate passage data
		CalculateChunksPassages(grid, width, height);
	
		// Return prepared map
		return grid;
	}
	
	/// <summary>
	/// Calculates possible passages between chunks
	/// </summary>
	/// <param name="grid"> Chunks' map array </param>
	/// <param name="width"> Width of the array </param>
	/// <param name="height"> Height of the array </param>
	void CalculateChunksPassages(Chunk[,] grid, int width, int height)
	{
		// Iterate through enery chunk
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				// Calculate map positions of the bottom-left node inside the chunk
				int mapX = x * ChunkSize;
				int mapY = y * ChunkSize;
				
				// Check SOUTH PASSAGE
				if (grid[x, y].canGoDown.HasValue == false)
				{
					// Initialize value
					grid[x, y].canGoDown = false;
					
					// Check if there is chunk below
					if (y != 0)
					{
						// Initialize value of the bottom-neightbour chunk
						grid[x, y - 1].canGoUp = false;
						
						// Iterate through every node in bottom row of the chunk and check if it can connect...
						// ...to the node in the top row in the bottom neighbour
						for (int xx = 0; xx < ChunkSize; xx++)
						{
							// Check if we didn't go out of map
							if (mapX + xx >= Map.width)
								break;
								
							// Check if there is connection
							if (Map.map[mapY, mapX + xx] == FREE &&
								Map.map[mapY - 1, mapX + xx] == FREE)
							{
								// Update flags and stop loop (passage exists, no need to check further)
								grid[x, y].canGoDown = true;
								grid[x, y - 1].canGoUp = true;
								break;
							}
						}
					}
				}
				
				// Check NORTH PASSAGE
				if (grid[x, y].canGoUp.HasValue == false)
				{
					// Initialize value
					grid[x, y].canGoUp = false;
					
					// Check if there is chunk above
					if (y < height - 1)
					{
						// Initialize value of the top-neightbour chunk
						grid[x, y + 1].canGoDown = false;
						
						// Iterate through every node in top row of the chunk and check if it can connect...
						// ...to the node in the bottom row in the top neighbour
						for (int xx = 0; xx < ChunkSize; xx++)
						{
							// Check if we didn't go out of map
							if (mapX + xx >= Map.width)
								break;

							// Check if there is connection
							if (Map.map[mapY + ChunkSize - 1, mapX + xx] == FREE &&
								Map.map[mapY + ChunkSize, mapX + xx] == FREE)
							{
								// Update flags and stop loop (passage exists, no need to check further)
								grid[x, y].canGoUp = true;
								grid[x, y + 1].canGoDown = true;
								break;
							}
						}
					}
				}
				
				// Check EAST PASSAGE
				if (grid[x, y].canGoRight.HasValue == false)
				{
					// Initialize value
					grid[x, y].canGoRight = false;
					
					// Check if there is chunk on the right
					if (x < width - 1)
					{
						// Initialize value of the right-neighbour chunk
						grid[x + 1, y].canGoLeft = false;
						
						// Iterate through every node in right column of the chunk and check if it can connect...
						// ...to the node in the left column in the right neighbour
						for (int yy = 0; yy < ChunkSize; yy++)
						{
							// Check if we didn't go out of map
							if (mapY + yy >= Map.height)
								break;
								
							// Check if there is connection
							if (Map.map[mapY + yy, mapX + ChunkSize - 1] == FREE &&
								Map.map[mapY + yy, mapX + ChunkSize] == FREE)
							{
								// Update flags and stop loop (passage exists, no need to check further)
								grid[x, y].canGoRight = true;
								grid[x + 1, y].canGoLeft = true;
								break;
							}
						}
					}
				}
				
				// Check WEST PASSAGE
				if (grid[x, y].canGoLeft.HasValue == false)
				{
					// Initialize value
					grid[x, y].canGoLeft = false;
					
					// Check if there is chunk on the left
					if (x != 0)
					{
						// Initialize value of the left-neighbour chunk
						grid[x - 1, y].canGoRight = false;
						
						// Iterate through every node in left column of the chunk and check if it can connect...
						// ...to the node in the right column in the left neighbour
						for (int yy = 0; yy < ChunkSize; yy++)
						{
							// Check if we didn't go out of map
							if (mapY + yy >= Map.height)
								break;
								
							// Check if there is connection
							if (Map.map[mapY + yy, mapX] == FREE &&
								Map.map[mapY + yy, mapX - 1] == FREE)
							{
								// Update flags and stop loop (passage exists, no need to check further)
								grid[x, y].canGoLeft = true;
								grid[x - 1, y].canGoRight = true;
								break;
							}
						}
					}
				}
				
				// Check NORTH-EAST PASSAGE
				if (grid[x, y].canGoUpRight.HasValue == false)
				{
					// Initialize value
					grid[x, y].canGoUpRight = false;
					
					// Check if there is chunk in the NE direction
					if (x != width - 1 && y != height - 1)
					{
						// Initialize neighbour's value
						grid[x + 1, y + 1].canGoDownLeft = false;
						
						// Check if corners are free
						if (Map.map[mapY + ChunkSize - 1, mapX + ChunkSize - 1] == FREE &&
							Map.map[mapY + ChunkSize, mapX + ChunkSize] == FREE)
						{
							// Update values
							grid[x, y].canGoUpRight = true;
							grid[x + 1, y + 1].canGoDownLeft = true;
						}
					}
				}
				
				// Check SOUTH-EAST
				if (grid[x, y].canGoDownRight.HasValue == false)
				{
					// Initialize value
					grid[x, y].canGoDownRight = false;
					
					// Check if there is chunk in the SE direction
					if (x != width - 1 && y != 0)
					{
						// Initialize neighbour's value
						grid[x + 1, y - 1].canGoUpLeft = false;
						
						// Check if corners are free
						if (Map.map[mapY, mapX + ChunkSize - 1] == FREE &&
							Map.map[mapY - 1, mapX + ChunkSize] ==  FREE)
						{
							// Update values
							grid[x, y].canGoDownRight = true;
							grid[x + 1, y - 1].canGoUpLeft = true;
						}
					}
				}
				
				// Check SOUTH-WEST
				if (grid[x, y].canGoDownLeft.HasValue == false)
				{
					// Initialize value
					grid[x, y].canGoDownLeft = false;
					
					// Check if there is node in the SW direction
					if (x != 0 && y != 0)
					{
						// Initialize neighbour's value
						grid[x - 1, y - 1].canGoUpRight = false;
						
						// Check if corners are free
						if (Map.map[mapY, mapX] == FREE &&
							Map.map[mapY - 1, mapX - 1] == FREE)
						{
							// Update values
							grid[x, y].canGoDownLeft = true;
							grid[x - 1, y - 1].canGoUpRight = true;
						}
					}
				}
				
				// Check NORTH-WEST
				if (grid[x, y].canGoUpLeft.HasValue == false)
				{
					// Initialize value
					grid[x, y].canGoUpLeft = false;
					
					// Check if there is node in the NW direction
					if (x != 0 && y != height - 1)
					{
						// Initialize neighbour's value
						grid[x - 1, y + 1].canGoDownRight = false;
						
						// Check if corners are free
						if (Map.map[mapY + ChunkSize - 1, mapX] == FREE &&
							Map.map[mapY + ChunkSize, mapX - 1] == FREE)
						{
							grid[x, y].canGoUpLeft = true;
							grid[x - 1, y + 1].canGoDownRight = true;
						}
					}
				}
			}
		}
	}
	
	/// <summary>
	/// Prints chunk map as string into the .txt file
	/// </summary>
	/// <param name="grid"> Chunks' map array </param>
	/// <param name="width"> Width of the array </param>
	/// <param name="height"> height of the array </param>
	void PrintChunkMap(Chunk[,] grid, int width, int height)
	{
		// Prepare array that will hold data as chars
		string[,] charArray = new string[width * (ChunkSize + 2), height * (ChunkSize + 2)];
		
		// Iterate through grid
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				// Get coords in the char array
				int charX = x * (ChunkSize + 2);
				int charY = y * (ChunkSize + 2);
				
				// Fill char array with information whether or not node is free or not
				for (int xx = 0; xx < ChunkSize; xx++)
				{
					for (int yy = 0; yy < ChunkSize; yy++)
					{
						// If we got out of map, stop loop
						if (x * ChunkSize + xx >= Map.width || y * ChunkSize + yy >= Map.height)
							break;
							
						// Put info into array
						charArray[charX + xx + 1, charY + yy + 1] = Map.map[y * ChunkSize + yy, x * ChunkSize + xx] == FREE ? "O" : "#"; 
					}
				}
				
				// Fill array with information about passages
				if (grid[x, y].canGoUp.Value)
					for (int xx = 1; xx < ChunkSize + 1; xx++)
						charArray[charX + xx, charY + ChunkSize + 1] = "^";
						
				if (grid[x, y].canGoDown.Value)
					for (int xx = 1; xx < ChunkSize + 1; xx++)
						charArray[charX + xx, charY] = "v";
						
				if (grid[x, y].canGoLeft.Value)
					for (int yy = 1; yy < ChunkSize + 1; yy++)
						charArray[charX, charY + yy] = "<";
						
				if (grid[x, y].canGoRight.Value)
					for (int yy = 1; yy < ChunkSize + 1; yy++)
						charArray[charX + ChunkSize + 1, charY + yy] = ">";
						
				if (grid[x, y].canGoUpRight.Value)
					charArray[charX + ChunkSize + 1, charY + ChunkSize + 1] = "/";

				if (grid[x, y].canGoUpLeft.Value)
					charArray[charX, charY + ChunkSize + 1] = "\\";

				if (grid[x, y].canGoDownRight.Value)
					charArray[charX + ChunkSize + 1, charY] = "\\";

				if (grid[x, y].canGoDownLeft.Value)
					charArray[charX, charY] = "/";
			}
		}
		
		// Fill empty places with space char
		for (int i = 0; i < charArray.GetLength(0); i++)
			for (int j = 0; j < charArray.GetLength(1); j++)
				if (charArray[i, j] == null)
					charArray[i, j] = " ";
				
		// Create one big string with data
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		for (int y = 0; y < charArray.GetLength(1); y++)
		{
			for (int x = 0; x < charArray.GetLength(0); x++)
			{
				if (x % (ChunkSize + 2) == 0)
					sb.Append("  ");
				sb.Append(charArray[x, y]);
			}
			if (y % (ChunkSize + 2) == 0)
				sb.Append("\n");
			sb.AppendLine();
		}
		
		// Save that data to file and open it
		System.IO.File.WriteAllText("ChunkMap.txt", sb.ToString());
		
		// Uncomment line below if you want to automatically open generated file
		// System.Diagnostics.Process.Start("ChunkMap.txt");
	}
	
	/// <summary>
	/// Paints chunk on the displayer
	/// </summary>
	/// <param name="chunk"> Chunk to paint </param>
	void PaintChunk(Chunk chunk, Color? chunkColor = null)
	{
		// Get color of the chunk if not provided
		if (chunkColor.HasValue == false)
			chunkColor = (chunk.x + chunk.y) % (ChunkSize * 2) == 0 ? Displayer.Instance.subpathColor1 : Displayer.Instance.subpathColor2;
			
		// Paint every free node in this chunk
		for (int x = 0; x < ChunkSize; x++)
			for (int y = 0; y < ChunkSize; y++)
				if (chunk.y + y < Map.height & chunk.x + x < Map.width && Map.map[chunk.y + y, chunk.x + x] == FREE)
					Displayer.Instance.PaintPath(chunk.x + x, chunk.y + y, chunkColor.Value);
	}
}
