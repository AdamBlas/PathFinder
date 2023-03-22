using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ChunkSizeManager;
using static NodeType;

public class HPAStar : Algorithm
{
	/// <summary>
	/// Class represents a single chunk
	/// </summary>
	public class Chunk : Node
	{
		[Tooltip("Constant value. Amount of memory used by single chunk.")]
		public new const int MEMORY_USAGE = Node.MEMORY_USAGE + 8;
		// Base usage
		// bool = 1 byte
		
		// Flags that indicate whether or not you can go from this chunk in given direction
		public bool N = false;
		public bool NE = false;
		public bool E = false;
		public bool SE = false;
		public bool S = false;
		public bool SW = false;
		public bool W = false;
		public bool NW = false;
		
		
		
		
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="x"> X coordinate of the chunk. Must be multiple of chunk size. </param>
		/// <param name="y"> Y coordinate of the chunk. Must be multiple of chunk size. </param>
		/// <param name="cost"> Cost of the chunk </param>
		/// <param name="parentChunk"> Previous chunk in the path </param>
		public Chunk(int x, int y, float cost, Chunk parentChunk) : base(x, y, cost, parentChunk) { }
		
		public override string ToString() => "X=" + x + ", Y=" + y + ", GridX=" + (int)(x / ChunkSize) + ", GridY=" + (int)(y / ChunkSize) + ", Cost=" + goalBoundCost + ", Parent={" + (parentNode == null ? "None" : (parentNode.x + ", " + parentNode.y)) + "}";
	
		/// <summary>
		/// Returns this chunk in a string array version
		/// </summary>
		public string[] ToStringArray()
		{
			// Prepare array of chars, increase size by four for borders and passage signs
			char[,] charArr = new char[ChunkSize + 1, ChunkSize + 1];
			
			// Initialize values
			for (int i = 0; i < ChunkSize + 1; i++)
				for (int j = 0; j < ChunkSize + 1; j++)
					charArr[i, j] = '▓';

			// Fill middle part with chunk info
			for (int i = 0; i < ChunkSize; i++)
				for (int j = 0; j < ChunkSize; j++)
					charArr[j, i] = (x + i < Map.width && y + j < Map.height && Map.map[y + j, x + i] == FREE) ? '░' : '█';

			// Fill passages
			if (E)
				for (int i = 0; i < ChunkSize; i++)
					charArr[i, ChunkSize] = '↔';
					
			if (N)
				for (int i = 0; i < ChunkSize; i++)
					charArr[ChunkSize, i] = '↕';
					
			if (NE)
				charArr[ChunkSize, ChunkSize] = 'X';
						
			// Convert char 2D array to string array
			string[] result = new string[ChunkSize + 1];
			for (int i = 0; i < ChunkSize + 1; i++)
			{
				result[i] = string.Empty;
				for (int j = 0; j < ChunkSize + 1; j++)
					result[i] += charArr[i, j];
			}
			
			return result;
		}
	}
	
	
	
	
	
	[Tooltip("Singleton")]
	public static HPAStar Instance;
	
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
	
	[Tooltip("Time required for finding path inside chunks")]
	float timeToFindFinalPath;
	
	[Tooltip("Array of already visited chunks")]
	public Array2D<Chunk> visitedChunks;
	
	[Tooltip("Chunk where path was not found")]
	Chunk chunkWithNoPath;

	
	
	
	
	
	/// <summary>
	/// Constructor
	/// </summary>
	public HPAStar(params Heuristic[] heuristics) : base(heuristics)
	{
		// Create singleton
		Instance = this;

		// Set name and description
		name = "HPA*";
		description = "HPA* (Hierarchical PathFinding A*) - Algorithm splits map into smaller chunks, uses A* to find path from chunk with start node to chunk with goal node and then uses A* to find path only inside chunks in path.";
	
		// Set path to file with statistics
		statsFileName = "Statistics/HPAStar_Statistics.csv";
	}
	
	/// <summary>
	/// Solves current problem using A* algorithm
	/// </summary>
	/// <param name="heuristic"> Heuristic used to calculate node's cost </param>
	/// <param name="howMuchToRemove"> How many worst records has to be deleted before averaging </param>
	public override IEnumerator Solve(Heuristic heuristic, int iterations, int howMuchToRemove)
	{
		// Set flag
		Solver.isRunning = true;
		
		// Prepare variables that will store results
		bool pathFound = false;
		float timeToGenerateMap = 0;
		int nodesLength = 0;
		float pathLength = 0;
		int chunksAllocated = 0;
		int nodesAllocated = 0;
		
		// Reset global variables
		finalChunk = null;
		grid = null;
		amountOfChunksToAnalyze = 0;
		amountOfChunksAnalyzed = 0;
		amountOfChunksReanalyzed = 0;
		timeToFindPathInChunkGrid = 0;
		finalNode = null;
		timeToFindFinalPath = 0;
		visitedChunks = null;
		chunkWithNoPath = null;
		
		for (int i = 0; i < iterations; i++)
		{
			// Prepare timer object
			System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
			timer.Start();
			
			// Create chunk grid
			grid = CreateGrid(out int width, out int height);
	
			// Get precalculation time
			timer.Stop();		
			timeToGenerateMap = (float)timer.Elapsed.TotalMilliseconds;
			timer.Reset();
			
			// Set details button action to create file with detailed data
			DetailsButtonManager.EnableButton();
			DetailsButtonManager.ButtonComponent.onClick.RemoveAllListeners();
			DetailsButtonManager.ButtonComponent.onClick.AddListener(() => PrintChunkMap(grid, width, height));
			
			// Find path in this grid
			yield return ApplyAStartOnGrid(grid, width, height, heuristic);
			
			// Check if path was found
			if (finalChunk == null)
			{
				ResultDisplayer.SetText(1, "FAILURE:\nPath not found");
				ResultDisplayer.SetText(2, "FAILURE:\nNo path was found using chunk grid");
				ResultDisplayer.SetText(3, string.Empty);
			}
			else
			{
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
					// Set flag
					pathFound = false;
					
					// Display message about failure
					ResultDisplayer.SetText(1, "FAILURE\nPath not found");
					ResultDisplayer.SetText(2, "FAILURE\nNo path was found inside the chunk (" + chunkWithNoPath.x + ", " + chunkWithNoPath.y + ")");
					ResultDisplayer.SetText(3, string.Empty);
				}
				else
				{
					// Set flag
					pathFound = true;
					
					// Paint all pixels on displayer and calculate path's length
					PrintPath(finalNode, out nodesLength, out pathLength);
					
					// Prepare messages to display
					string msg1 = "TIME";
					msg1 += "\nPrecalc:\t" + timeToGenerateMap + " ms";
					msg1 += "\nChunk path:\t" + timeToFindPathInChunkGrid + " ms";
					msg1 += "\nFinal path:\t" + timeToFindFinalPath + " ms";
					msg1 += "\nTotal:\t\t" + (timeToGenerateMap + timeToFindPathInChunkGrid + timeToFindFinalPath) + " ms";
					msg1 += "\n\nPATH LENGTH";
					msg1 += "\nNodes:\t" + nodesLength;
					msg1 += "\nDistance:\t" + pathLength.ToString("f2");
					
					chunksAllocated = Displayer.GetAmountOfChunksAllocated();
					string msg2 = "NODES AMOUNT";
					msg2 += "\nChunks:";
					msg2 += "\n\tTo Analyze:\t" + amountOfChunksToAnalyze;
					msg2 += "\n\tAnalyzed:\t" + amountOfChunksAnalyzed;
					msg2 += "\n\tAllocated:\t" + chunksAllocated;
					msg2 += "\n\tIn Path:\t" + amountOfChunksInPath;
					msg2 += "\nNodes:";
					msg2 += "\n\tAnalyzed:\t" + nodesAnalyzed;
					msg2 += "\n\tAllocated:\t" + ChunkSize * ChunkSize;
					
					string msg3 = "MEMORY USAGE";
					msg3 += "\nUsage per chunk:\t" + Chunk.MEMORY_USAGE + " B";
					msg3 += "\nUsage for nodes:\t" + (ChunkSize * ChunkSize * Node.MEMORY_USAGE) + " B";
					msg3 += "\nTotal memory:\t" + (chunksAllocated * Chunk.MEMORY_USAGE) + (ChunkSize * ChunkSize * Node.MEMORY_USAGE) + " B";
					
					// Print statistics
					ResultDisplayer.SetText(1, msg1);
					ResultDisplayer.SetText(2, msg2);
					ResultDisplayer.SetText(3, msg3);
					
					// Save stats for later average
					AddValuesToAverage(timeToGenerateMap, timeToFindPathInChunkGrid, timeToFindFinalPath, timeToFindPathInChunkGrid + timeToFindFinalPath);
				}
			}
		}
		// All iterations done
	
		// First, check if file exists
		if (System.IO.File.Exists(statsFileName) == false)
		{
			// File does not exist, create it and add headers
			SaveToCsv(
				"Map Name",
				"Start Coords",
				"Goal Coords",
				"Chunk Size",
				"Heuristic",
				"Path Found",
				"Precalc Time",
				"Search Time",
				"Total Time",
				"Path Length (Node)",
				"Path Length (Distance)",
				"Chunks Analyzed",
				"Chunks Allocated",
				"Nodes Analyzed",
				"Nodes Allocated",
				"Memory (B)",
				"Goal Bounding",
				"Cost Overwrite",
				"Error Margin"
			);
		}
			
		if (pathFound == false)
		{
			// If path was not found, write just data about map
			SaveToCsv(
				LoadMap.mapName,
				StartGoalManager.StartToString(),
				StartGoalManager.GoalToString(),
				ChunkSize,
				AlgorithmSelector.GetHeuristic().name,
				false,
				"---", "---", "---", "---", "---", "---", "---", "---", "---", "---", "---", "---", "---"
			);
		}
		else
		{
			// Get avg values
			float[] times = AverageValues(howMuchToRemove, 3);
			timeToGenerateMap = times[0];
			timeToFindPathInChunkGrid = times[1];
			timeToFindFinalPath = times[2];
			float totalPathFindTime = times[3];
			
			// If path was found, save all statistics
			SaveToCsv(
				LoadMap.mapName,
				StartGoalManager.StartToString(),
				StartGoalManager.GoalToString(),
				ChunkSize,
				AlgorithmSelector.GetHeuristic().name,
				true,
				timeToGenerateMap,
				timeToFindPathInChunkGrid + timeToFindFinalPath,
				timeToGenerateMap + timeToFindPathInChunkGrid + timeToFindFinalPath,
				nodesLength,
				pathLength,
				amountOfChunksAnalyzed,
				chunksAllocated,
				nodesAnalyzed,
				ChunkSize * ChunkSize + nodesLength,
				(chunksAllocated * Chunk.MEMORY_USAGE) + ((ChunkSize * ChunkSize + nodesLength) + Node.MEMORY_USAGE),
				GoalBoundingManager.shouldApply ? GoalBoundingManager.strength : "---",
				GoalBoundingManager.shouldApply ? CostOverwriteManager.shouldOverwrite : "---",
				GoalBoundingManager.shouldApply && CostOverwriteManager.shouldOverwrite ? (CostOverwriteManager.errorMargin - 1) * 100 + "%" : "---"
			);
		}
		
		// Set flag
		Solver.isRunning = false;
	}
	
	/// <summary>
	/// Tries to expand the node towards given coordinates
	/// </summary>
	/// <param name="x"> X coordinate of the expansion </param>
	/// <param name="y"> Y coordinate of the expansion </param>
	/// <param name="parentNode"> Expanding node </param>
	void TryToExpandNode(int x, int y, int chunkX, int chunkY, Node parentNode)
	{
		// Check if node is free
		if (Map.map[y, x] == OBSTACLE)
			return;
		
		// Check if movement if diagonal
		if (parentNode.x != x && parentNode.y != y)
		{
			// Movement is diagonal, check if movement in both cardinal directions is possible
			if (Map.map[parentNode.y, x] == OBSTACLE || Map.map[y, parentNode.x] == OBSTACLE)
				return;
		}
		
		// Check if node was visited
		if (nodesVisited[chunkX, chunkY] == null)
		{
			// Node is free, expand in that direction
			Node newNode = new Node(x, y, parentNode, heuristic, false);
			
			// Add that node to list
			list.Add(newNode);
			
			// Mark spot as visited
			nodesVisited[chunkX, chunkY] = newNode;
		}
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
		nodesAnalyzed = 0;
		
		// Iterate as long as there are chunks in list
		while (chunk != null)
		{
			// Prepare array with info of whether or not node was visited
			nodesVisited = new Array2D<Node>(ChunkSize, ChunkSize);
			nodesVisited[node.x % ChunkSize, node.y % ChunkSize] = node;
			
			// Prepare list with visited chunks
			list = new NodeSortedList(node);

			// Prepare output variables
			bool pathFoundInsideChunk = false;
			
			// Get the direction we want to move, this determines end condition
			int xOffset = chunk.parentNode == null ? 0 : chunk.parentNode.x - chunk.x;
			int yOffset = chunk.parentNode == null ? 0 : chunk.parentNode.y - chunk.y;
			
			/*
			if (xOffset == 0 && yOffset > 0)		Debug.Log("Going SOUTH");
			else if (xOffset == 0 && yOffset < 0)	Debug.Log("Going NORTH");
			else if (xOffset > 0 && yOffset == 0)	Debug.Log("Going EAST");
			else if (xOffset < 0 && yOffset == 0)	Debug.Log("Going WEST");
			else if (xOffset > 0 && yOffset > 0)	Debug.Log("Going SOUTH-EAST");
			else if (xOffset > 0 && yOffset < 0)	Debug.Log("Going NORTH-EAST");
			else if (xOffset < 0 && yOffset > 0)	Debug.Log("Going SOUTH-WEST");
			else if (xOffset < 0 && yOffset < 0)	Debug.Log("Going NORTH-WEST");
			*/
			
			// Iterate as long as there are nodes in list
			while (list.count != 0)
			{
				// Get node from the list
				node = list.PopAtZero();
				nodesAnalyzed++;
				
				// Debug.Log("Analyzing node: " + node + "\n Analyzing chunk: " + chunk);

				// Get node's coords inside chunk
				int nodeChunkX = node.x - chunk.x;
				int nodeChunkY = node.y - chunk.y;
				
				// Debug.Log("Chunk Node coords: (" + nodeChunkX + ", " + nodeChunkY + ")");

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
	
						// Exit coroutine
						yield break;
					}
				}
				else
				{
					if (xOffset == 0)
					{
						// Goal chunk is in the same column
						if (yOffset > 0)
						{
							// Goal chunk is on the SOUTH
							if (nodeChunkY == ChunkSize - 1 && Map.map[node.y + 1, node.x] == FREE)
							{
								// Debug.Log("Passage found: (" + node.x + ", " + node.y + ") -> (" + node.x + ", " + (node.y + 1) + ")");
								
								// Neighbour node is free, set it as new node
								node = new Node(node.x, node.y + 1, node, heuristic);
								
								// Set flag
								pathFoundInsideChunk = true;
							}
						}
						else
						{
							// Goal chunk is on the NORTH
							if (nodeChunkY == 0 && Map.map[node.y - 1, node.x] == FREE)
							{
								// Debug.Log("Passage found: (" + node.x + ", " + node.y + ") -> (" + node.x + ", " + (node.y + 1) + ")");
								
								// Neighbour node is free, set it as new node
								node = new Node(node.x, node.y - 1, node, heuristic);
								
								// Set flag
								pathFoundInsideChunk = true;
							}
						}
					}
					else if (yOffset == 0)
					{
						// Goal chunk is in the same row
						if (xOffset > 0)
						{
							// Goal chunk is on the EAST
							if (nodeChunkX == ChunkSize - 1 && Map.map[node.y, node.x + 1] == FREE)
							{
								// Debug.Log("Passage found: (" + node.x + ", " + node.y + ") -> (" + (node.x + 1) + ", " + node.y + ")");
								
								// Neighbour node is free, set it as new node
								node = new Node(node.x + 1, node.y, node, heuristic);
								
								// Set flag
								pathFoundInsideChunk = true;
							}
						}
						else
						{
							// Goal chunk is on the WEST
							if (nodeChunkX == 0 && Map.map[node.y, node.x - 1] == FREE)
							{
								// Debug.Log("Passage found: (" + node.x + ", " + node.y + ") -> (" + (node.x - 1) + ", " + node.y + ")");
								
								// Neighbour node is free, set it as new node
								node = new Node(node.x - 1, node.y, node, heuristic);
								
								// Set flag
								pathFoundInsideChunk = true;
							}
						}
					}
					else
					{
						// Movement is diagonal
						if (xOffset > 0)
						{
							// Goal chunk is on the EAST
							if (yOffset > 0)
							{
								// Goal chunk is on the SOUTH-EAST
								// No need to check if neighbour is free, if it weren't, connection would not be made
								if (nodeChunkX == ChunkSize - 1 && nodeChunkY == ChunkSize - 1)
								{
									// Debug.Log("Passage found: (" + node.x + ", " + node.y + ") -> (" + (node.x + 1) + ", " + (node.y + 1) + ")");
								
									// Neighbour node is free, set it as new node
									node = new Node(node.x + 1, node.y + 1, node, heuristic);
								
									// Set flag
									pathFoundInsideChunk = true;
								}
							}
							else
							{
								// Goal chunk is on the NORTH-EAST
								// No need to check if neighbour is free, if it weren't, connection would not be made
								if (nodeChunkX == ChunkSize - 1 && nodeChunkY == 0)
								{
									// Debug.Log("Passage found: (" + node.x + ", " + node.y + ") -> (" + (node.x + 1) + ", " + (node.y - 1) + ")");
								
									// Neighbour node is free, set it as new node
									node = new Node(node.x + 1, node.y - 1, node, heuristic);
								
									// Set flag
									pathFoundInsideChunk = true;
								}
							}
						}
						else
						{
							// Goal chunk is on the WEST
							if (yOffset > 0)
							{
								// Goal chunk is on the SOUTH-WEST
								// No need to check if neighbour is free, if it weren't, connection would not be made
								if (nodeChunkX == 0 && nodeChunkY == ChunkSize - 1)
								{
									// Debug.Log("Passage found: (" + node.x + ", " + node.y + ") -> (" + (node.x - 1) + ", " + (node.y + 1) + ")");
								
									// Neighbour node is free, set it as new node
									node = new Node(node.x - 1, node.y + 1, node, heuristic);
								
									// Set flag
									pathFoundInsideChunk = true;
								}
							}
							else
							{
								// Goal chunk is on the NORTH-WEST
								// No need to check if neighbour is free, if it weren't, connection would not be made
								if (nodeChunkX == 0 && nodeChunkY == 0)
								{
									// Debug.Log("Passage found: (" + node.x + ", " + node.y + ") -> (" + (node.x - 1) + ", " + (node.y - 1) + ")");
								
									// Neighbour node is free, set it as new node
									node = new Node(node.x - 1, node.y - 1, node, heuristic);
								
									// Set flag
									pathFoundInsideChunk = true;
								}
							}
						}
					}
				}
				
				// If path was found, don't expand and go to the next chunk
				if (pathFoundInsideChunk)
				{
					// Clear list from all nodes from current chunk
					list.Clear();
					
					// Add newly found node
					list.Add(node);
					
					// Exit loop
					break;
				}
				
				// End conditions were not met, expand the node

				bool canGoNorth = node.y != Map.height - 1 && nodeChunkY != ChunkSize - 1;
				bool canGoEast = node.x != Map.width - 1 && nodeChunkX != ChunkSize - 1;
				bool canGoSouth = nodeChunkY != 0;
				bool canGoWest = nodeChunkX != 0;

				// Expand in NORTH directions if possible
				if (canGoNorth)
				{
					// Try to expand in NORTH-WEST
					if (canGoWest)
						TryToExpandNode(node.x - 1, node.y + 1, nodeChunkX - 1, nodeChunkY + 1, node);
						
					// Try to expand in NORTH
					TryToExpandNode(node.x, node.y + 1, nodeChunkX, nodeChunkY + 1, node);
					
					// Try to expand in NORTH-EAST
					if (canGoEast)
						TryToExpandNode(node.x + 1, node.y + 1, nodeChunkX + 1, nodeChunkY + 1, node);
				}
				
				// Try to expand in EAST
				if (canGoEast)
					TryToExpandNode(node.x + 1, node.y, nodeChunkX + 1, nodeChunkY, node);
				
				// Expand in SOUTH directions if possible
				if (canGoSouth)
				{
					// Try to expand SOUTH-EAST
					if (canGoEast)
						TryToExpandNode(node.x + 1, node.y - 1, nodeChunkX + 1, nodeChunkY - 1, node);
						
					// Try to expand SOUTH
					TryToExpandNode(node.x, node.y - 1, nodeChunkX, nodeChunkY - 1, node);
					
					// Try to expand SOUTH-WEST
					if (canGoWest)
						TryToExpandNode(node.x - 1, node.y - 1, nodeChunkX - 1, nodeChunkY - 1, node);
				}

				// Try to expand in W
				if (canGoWest)
					TryToExpandNode(node.x - 1, node.y, nodeChunkX - 1, nodeChunkY, node);
				
				// Node expanded in every possible direction, sort list and go to the next node
				list.Sort();
			}

			// Check if path was found
			if (pathFoundInsideChunk == false)
			{
				// Path was not found, set chunk and end method
				chunkWithNoPath = chunk;
				yield break;
			}
			
			// Path was found inside chunk, switch chunk
			chunk = (Chunk)chunk.parentNode;
		}
	}
	
	/// <summary>
	/// Tries to expand chunk in given direction
	/// </summary>
	/// <return> Whether or not chunk was expaneded </return>
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
					visitedChunks[gridX, gridY].parentNode = chunk;
					
					// Mask spot as visited
					grid[gridX, gridY] = (Chunk)visitedChunks[gridX, gridY];
					
					// Add that chunk to list once again
					list.Add(visitedChunks[gridX, gridY]);
					
					// Increase counter
					amountOfChunksReanalyzed++;
				}
			}
		}
		else
		{
			// Skip obstacle nodes
			
			// Chunk wasn't visited before, add it to list
			list.Add(grid[gridX, gridY]);

			// Set parent
			grid[gridX, gridY].parentNode = chunk;

			// Calculate its cost
			heuristic.CalculateCost(grid[gridX, gridY]);

			// Increase amount of chunks to analyze
			amountOfChunksToAnalyze++;
			
			// Mark chunk as visited
			visitedChunks[gridX, gridY] = grid[gridX, gridY];
			
			// Paint chunk as 'To Analyze'
			PaintChunk(grid[gridX, gridY], Displayer.Instance.toAnalyzeColor);
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
		
		// Save heuristic for later
		this.heuristic = heuristic;
		
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
		int gridSize = width * height * 10;
		
		// Prepare variable that will store result
		bool pathFound = false;
		amountOfChunksToAnalyze = 0;
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
			
			// Mark chunk as visited
			visitedChunks[gridX, gridY] = grid[gridX, gridY];
			amountOfChunksAnalyzed++;
			
			// Add neighbour chunks to the list
			
			// Check NORTH neighbour
			TryToExpandChunk(gridX, gridY + 1, chunk.N, chunk);
			
			// Check NORTH-EAST neighbour
			TryToExpandChunk(gridX + 1, gridY + 1, chunk.NE, chunk);
			
			// Check EAST
			TryToExpandChunk(gridX + 1, gridY, chunk.E, chunk);
			
			// Check SOUTH-EAST
			TryToExpandChunk(gridX + 1, gridY - 1, chunk.SE, chunk);
			
			// Check SOUTH
			TryToExpandChunk(gridX, gridY - 1, chunk.S, chunk);
			
			// Check SOUTH-WEST
			TryToExpandChunk(gridX - 1, gridY - 1, chunk.SW, chunk);
			
			// Check WEST
			TryToExpandChunk(gridX - 1, gridY, chunk.W, chunk);
			
			// Check NORTH-WEST
			TryToExpandChunk(gridX - 1, gridY + 1, chunk.NW, chunk);
			
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
		// Ietarte through all chunks
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				// Precalculate map values
				int mapX = x * ChunkSize;
				int mapY = y * ChunkSize;
				
				// Check if can go NE
				if (x + 1 < width && y + 1 < height)
				{
					// Check if there is passage between chunks
					if (Map.map[mapY + ChunkSize - 1, mapX + ChunkSize - 1] == FREE &&
						Map.map[mapY + ChunkSize - 1, mapX + ChunkSize] == FREE &&
						Map.map[mapY + ChunkSize, mapX + ChunkSize - 1] == FREE &&
						Map.map[mapY + ChunkSize, mapX + ChunkSize] == FREE)
					{
						// Passage exists
						grid[x, y].NE = true;
						grid[x + 1, y + 1].SW = true;
					}
				}
				
				// Check if can go E
				if (x + 1 < width)
				{
					for (int yy = 0; yy < ChunkSize; yy++)
					{
						// Check if we didn't go out of map
						if (mapY + yy >= Map.height)
							break;
							
						// Check if there is passage
						if (Map.map[mapY + yy, mapX + ChunkSize - 1] == FREE &&
							Map.map[mapY + yy, mapX + ChunkSize] == FREE)
						{
							grid[x, y].E = true;
							grid[x + 1, y].W = true;
							break;
						}
					}
				}
				
				// Check if can no SE
				if (x + 1 < width && y - 1 >= 0)
				{
					// Check if there is passage between chunks
					if (Map.map[mapY, mapX + ChunkSize - 1] == FREE &&
						Map.map[mapY - 1, mapX + ChunkSize - 1] == FREE &&
						Map.map[mapY, mapX + ChunkSize] == FREE &&
						Map.map[mapY - 1, mapX + ChunkSize] == FREE)
					{
						grid[x, y].SE = true;
						grid[x + 1, y - 1].NW = true;
					}
				}
				
				// Check if can go S
				if (y - 1 >= 0)
				{
					for (int xx = 0; xx < ChunkSize; xx++)
					{
						// Check if we didn't get out of map
						if (mapX + xx >= Map.width)
							break;
							
						// Check if there is passage
						if (Map.map[mapY, mapX + xx] == FREE &&
							Map.map[mapY - 1, mapX + xx] == FREE)
						{
							grid[x, y].S = true;
							grid[x, y - 1].N = true;
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
		// Prepare output array
		string[,] output = new string[width, height * (ChunkSize + 1)];
		
		// Iterate through map
		for (int row = 0; row < height; row++)
			for (int col = 0; col < width; col++)
			{
				// Get chunk as string array
				string[] chunk = grid[col, row].ToStringArray();

				// Use this array to fill results
				for (int i = 0; i < ChunkSize + 1; i++)
					output[col, (row * (ChunkSize + 1)) + i] = chunk[i];
			}			
		
		// Convert string array to a single string
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		for (int row = 0; row < height * (ChunkSize + 1); row++)
		{
			for (int col = 0; col < width; col++)
				sb.Append(output[col, row]);
			sb.AppendLine();
		}
		
		// Save that data to file and open it
		System.IO.File.WriteAllText("ChunkMap.txt", sb.ToString());
		
		// Open newly generated file
		System.Diagnostics.Process.Start("ChunkMap.txt");
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
				//if (chunk.y + y < Map.height & chunk.x + x < Map.width && Map.map[chunk.y + y, chunk.x + x] == FREE)
				if (chunk.y + y < Map.height & chunk.x + x < Map.width)
					Displayer.Instance.PaintPath(chunk.x + x, chunk.y + y, chunkColor.Value);
	}
}
