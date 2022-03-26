using System.Collections;
using UnityEngine;
using static PathFinder;

public abstract class Algorithm
{
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public Heuristic[] AvaliableHeuristics { get; protected set; }

    public abstract IEnumerator Solve(Heuristic heuristic, Vector2Int start, Vector2Int end);
    // Output data
    public void CreatePath(Node endNode, Node[,] map)
    {
        if (endNode == null)
            return;

        Node node = endNode;
        while (node.previousNode.Item1 != -1)
        {
            Map.RecentMap[node.x, node.y] = Map.Node.Path;
            ImageDisplayer.RefreshPixel(node.x, node.y);
            node = map[node.previousNode.Item1, node.previousNode.Item2];
        }
    }
    public void PrintOutputData(Node endNode, Node[,] map)
    {
        int searched = 0;
        int path = 0;
        int free = 0;

        for (int x = 0; x < Map.Width; x++)
        {
            for (int y = 0; y < Map.Height; y++)
            {
                switch (Map.RecentMap[x, y])
                {
                    case Map.Node.Free:
                        free++;
                        break;
                    case Map.Node.Searched:
                        searched++;
                        break;
                    case Map.Node.Path:
                        path++;
                        break;
                }
            }
        }

        if (path == 0)
        {
            OutputMessageManager.SetMessage("Path not found!");
            return;
        }

        float length = 0;
        Node node = endNode;
        while (true)
        {
            int xDiff = node.x - node.previousNode.Item1;
            int yDiff = node.y - node.previousNode.Item2;
            length += Mathf.Sqrt(xDiff * xDiff + yDiff * yDiff);
            node = map[node.previousNode.Item1, node.previousNode.Item2];

            if (node.previousNode.Item1 == -1)
                break;
        }

        OutputMessageManager.SetMessage(
                "Path found!\n" +
                "Length:\n" +
                "Nodes in Path: \n" +
                "Nodes Searched:",
                column: 1);

        OutputMessageManager.SetMessage(
            "\n" +
            length + "\n" +
            path + "\n" +
            searched,
            column: 2);
    }
}
