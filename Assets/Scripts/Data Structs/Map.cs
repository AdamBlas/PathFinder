using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NodeType;

/// <summary>
/// Enum that represents possible node states
/// </summary>
public enum NodeType
{
	FREE,
	OBSTACLE,
}

/// <summary>
/// Class that represents a map
/// </summary>
public static class Map
{
	[Tooltip("Most recently created map")]
	public static NodeType[,] map;
	
	// Dimensions of the map
	public static int width, height;



	
	
	
	/// <summary>
	/// Creates new map
	/// </summary>
	/// <param name="width"> Width of the map </param>
	/// <param name="height"> Height of the map </param>
	public static void CreateMap(int width, int height)
	{
		// Create new array
		map = new NodeType[height, width];
	
		// Save dimensions
		Map.width = width;
		Map.height = height;
	}
	
	/// <summary>
	/// Optimizes map by trimming obstacles around border
	/// </summary>
	public static void Trim()
	{
		// Prepare variables
		int xMin = 0;
		int xMax = width - 1;
		int yMin = 0;
		int yMax = height - 1;
		
		// Get first row with free node
		for (int row = 0; row < height; row++)
			for (int col = 0; col < width; col++)
				if (map[row, col] == FREE)
				{
					yMin = row;
					goto yMinFound;
				}
		yMinFound:
		
		// Get last row with free node
		for (int row = yMax; row >= 0; row--)
			for (int col = 0; col < width; col++)
				if (map[row, col] == FREE)
				{
					yMax = row;
					goto yMaxFound;
				}
		yMaxFound:
		
		// Get first column with free node
		for (int col = 0; col < width; col++)
			for (int row = 0; row < height; row++)
				if (map[row, col] == FREE)
				{
					xMin = col;
					goto xMinFound;
				}
		xMinFound:
			
		// Get last row with free cell	
		for (int col = xMax; col >= 0; col--)
			for (int row = 0; row < height; row++)
				if (map[row, col] == FREE)
				{
					xMax = col;
					goto xMaxFound;
				}
		xMaxFound:
		
		// =-=-=-=
		// Border values found. Now, trim map.
		// =-=-=-=
		
		// Get new dimensions
		width = xMax - xMin;
		height = yMax - yMin;
		
		// Create new array
		NodeType[,] newMap = new NodeType[height, width];
		
		// Copy content from current map
		for (int row = 0; row < height; row++)
			for (int col = 0; col < width; col++)
				newMap[row, col] = map[yMin + row, xMin + col];
				
		// Save new map
		map = newMap;
	}
	
	/// <summary>
	/// Returns map object as string
	/// </summary>
	/// <returns> Map as string </returns>
	public static string StaticToString()
	{
		string result = string.Empty;
		
		for (int i = 0; i < height; i++)
		{
			for (int j = 0 ; j < width; j++)
			{
				if (i == StartGoalManager.startRow && j == StartGoalManager.startCol)
					result += "S";
				else if (i == StartGoalManager.goalRow && j == StartGoalManager.goalCol)
					result += "G";
				else
					result += map[i, j] == FREE ? " " : "#";
			}
			result += "\n";
		}
		
		
		
		return result;
	}
}
