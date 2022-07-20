using System;
using System.Collections.Generic;

public class Node : IComparable<Node>
{
    public int x, y;
    public float value;
    public Node previousNode = null;
    public (int, int) previousNodeCoords;

    public Node(int x, int y, float value)
    {
        this.x = x;
        this.y = y;
        this.value = value;
        previousNode = null;
        previousNodeCoords = (-1, -1);
    }
    public Node(int x, int y, float value, (int, int) previousNodeCoords) : this(x, y, value)
    {
        this.x = x;
        this.y = y;
        this.value = value;
        this.previousNodeCoords = previousNodeCoords;
    }
    public Node(int x, int y, float value, Node previousNode) : this(x, y, value)
    {
        this.x = x;
        this.y = y;
        this.value = value;
        this.previousNode = previousNode;
    }
    public int CompareTo(Node other)
    {
        if (value <= other.value)
        {
            if (value == 0)
                return 1;
            return -1;
        }
        return 1;
    }
}
public class SortedList
{
    public int Count { get; private set; } = 0;
    readonly List<Node> sortedList = new List<Node>();
    readonly List<Node> newElements = new List<Node>();

    public SortedList() { }
    public SortedList(Node startNode)
    {
        sortedList.Add(startNode);
        Count = 1;
    }
    public void Add(Node newElement)
    {
        newElements.Add(newElement);
        Count += 1;
    }
    public void RemoveAtZero()
    {
        sortedList.RemoveAt(0);
        Count -= 1;
    }
    public Node GetAtZero()
    {
        return sortedList[0];
    }
    public void Sort()
    {
        foreach (Node element in newElements)
        {
            int index = sortedList.BinarySearch(element);
            if (index < 0)
                index = -index - 1;
            sortedList.Insert(index, element);
        }
        newElements.Clear();
    }
}