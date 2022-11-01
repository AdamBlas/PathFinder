using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2D Array with details in case of IndexOutOfBoundsException
/// </summary>
public class Array2D<T>
{
	// Dimensions of the array
	public readonly int width, height;
	
	private T[,] array;
	
	
	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="width"> Width of the array </param>
	/// <param name="height"> Height of the array </param>
	public Array2D(int width, int height)
	{
		this.width = width;
		this.height = height;
		
		array = new T[width, height];
	}
	
	/// <summary>
	/// [,] operator
	/// </summary>
	public T this[int x, int y]
	{
		get 
		{
			if (x >= width || y >= height)
				throw new System.IndexOutOfRangeException("Get: X=" + x + "/" + width + ", Y=" + y + "/" + height);
			return array[x, y];
		}
		set
		{
			if (x >= width || y >= height)
				throw new System.IndexOutOfRangeException("Set: X=" + x + "/" + width + ", Y=" + y + "/" + height);
			array[x, y] = value;
		}
	}
}
