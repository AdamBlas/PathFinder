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
	}
	
	
	
	
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
		Chunk[,] grid = CreateGrid();
		
		yield return null;
	}
	
	/// <summary>
	/// Applies A* on chunks' grid
	/// </summary>
	/// <param name="grid"> Chunks' map array </param>
	/// <param name="width"> Width of the array </param>
	/// <param name="height"> Height of the array </param>
	/// <return> Final chunk in the path </return>
	Chunk ApplyAStartOnGrid(Chunk[,] grid, int width, int height)
	{
		// Get coordinates of the chunk with start and goal node
		int startChunkX = StartGoalManager.startCol / ChunkSize;
		int startChunkY = StartGoalManager.startRow / ChunkSize;
		int goalChunkX = StartGoalManager.goalCol / ChunkSize;
		int goalChunkY = StartGoalManager.goalRow / ChunkSize;
		
		// From this point on, chunks will be treated as nodes for overall A*
		
		// Create and initialize list
		NodeSortedList list = new NodeSortedList(grid[startChunkX, startChunkY]);
		
		// Prepare lookup table to check if chunk was already visited
		bool[,] wasChunkVisited = new bool[width, height];
		wasChunkVisited[startChunkX, startChunkY] = true;
		
		// Prepare security counter
		int securityCounter = 0;
		int gridSize = width * height;
		
		// Start A* loop		
		while (list.count != 0)
		{
			// Check security counter
			if (securityCounter++ >= gridSize)
			{
				Debug.LogWarning("WARNING: Security counter reached for chunk A*");
				return null;
			}
			
			// Get the best chunk
			Chunk chunk = (Chunk)list.PopAtZero();
			
			// Check if this chunk contains 
		}
	}
	
	/// <summary>
	/// Divides map into smaller chunks
	/// </summary>
	/// <returns> Chunk array </returns>
	Chunk[,] CreateGrid()
	{
		// Calculate size of the chunk array
		int width = Mathf.CeilToInt((float)Map.width / ChunkSize);
		int height = Mathf.CeilToInt((float)Map.height / ChunkSize);
		
		Chunk[,] grid = new Chunk[width, height];
		
		// Initialize chunks' coordinates
		for (int x = 0; x < width; x++)
			for (int y = 0; y < height; y++)
				grid[x, y] = new Chunk(x * ChunkSize, y * ChunkSize, 0, null);
		
		// Generate passage data
		CalculateChunksPassages(grid, width, height);
		
		// Save grid to file
		PrintChunkMap(grid, width, height);
		
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
		System.Diagnostics.Process.Start("ChunkMap.txt");
	}
}
