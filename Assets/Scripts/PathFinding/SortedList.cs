using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IComparable<Node>
{
    public int x, y;
    public float value;
    public (int, int) previousNode;

    public Node(int x, int y, float value)
    {
        this.x = x;
        this.y = y;
        this.value = value;
        previousNode = (-1, -1);
    }
    public Node(int x, int y, float value, (int, int) previousNode) : this(x, y, value)
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