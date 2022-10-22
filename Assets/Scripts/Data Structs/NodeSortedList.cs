using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeSortedList
{
	[Tooltip("Amount of elements in the list")]
	public int count => sortedList.Count + newElements.Count;
	
	[Tooltip("List containing already sorted elements")]
	List<Node> sortedList = new List<Node>();
	
	[Tooltip("List containing new elements that need to be sorted before adding them to the main list")]
	List<Node> newElements = new List<Node>();
	
	
	
	
	
	
	/// <summary>
	/// Constructor
	/// </summary>
	public NodeSortedList() { }

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="initialNode"> Initial node to add at the begining </param>
	public NodeSortedList(Node initialNode)
	{
		sortedList.Add(initialNode);
	}
	
	/// <summary>
	/// Adds new element to list. Need to be sorted before next read.
	/// </summary>
	/// <param name="newElement"></param>
	public void Add (Node newElement)
	{
		newElements.Add(newElement);
	}

	/// <summary>
	/// Returns element at zeroth index and removes it from list
	/// </summary>
	/// <returns> Node with lowest cost </returns>
	public Node PopAtZero()
	{
		Node nodeAtZero = sortedList[0];
		sortedList.RemoveAt(0);
		return nodeAtZero;
	}
	
	/// <summary>
	/// Puts every recently added element into main list in a position that results in sorted list
	/// </summary>
	public void Sort()
	{
		// Iterate through all recently added elements
		foreach (Node element in newElements)
		{
			// FInd index for the element
			int index = sortedList.BinarySearch(element);
			
			// Don't know what this part is doing, to verify later
			// if (index < 0) index = -index - 1;
			
			// Insert element into calculated position
			sortedList.Insert(index, element);
		}
		
		// Clear buffer for new elements
		newElements.Clear();
	}
}
