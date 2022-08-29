using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Algorithm
{
    public string Name { get; protected set; }
    public string Description { get; protected set; }
    public Heuristic[] AvailableHeuristics { get; protected set; }
    GameObject pathLinesRenderObject;

    public abstract IEnumerator Solve(Heuristic heuristic, Vector2Int start, Vector2Int end);

    // Output data
    public void CreatePath(Node endNode, Node[,] map)
    {
        if (endNode == null)
            return;

        Node node = endNode;
        while (node.previousNodeCoords.Item1 != -1)
        {
            // Mark nodes between current node and previous one as path
            Node prev = map[node.previousNodeCoords.Item1, node.previousNodeCoords.Item2];
            int xOffset = prev.x - node.x;
            int yOffset = prev.y - node.y;
            int xNormalized = xOffset != 0 ? xOffset / Mathf.Abs(xOffset) : 0;
            int yNormalized = yOffset != 0 ? yOffset / Mathf.Abs(yOffset) : 0;

            int x = node.x;
            int y = node.y;
            while (x != prev.x || y != prev.y)
            {
                Map.RecentMap[x, y] = Map.Node.Path;
                ImageDisplayer.RefreshPixel(x, y);
                x += xNormalized;
                y += yNormalized;
            }

            node = map[node.previousNodeCoords.Item1, node.previousNodeCoords.Item2];
        }
    }
    public void PrintOutputData(Node endNode, Node[,] map, long? precalculation, long calulation)
    {
        if (endNode == null)
        {
            OutputMessageManager.SetMessage("Path not found!", column: 1);
            OutputMessageManager.SetMessage(string.Empty, column: 2);
            return;
        }

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
            int xDiff = node.x - node.previousNodeCoords.Item1;
            int yDiff = node.y - node.previousNodeCoords.Item2;
            length += Mathf.Sqrt(xDiff * xDiff + yDiff * yDiff);
            node = map[node.previousNodeCoords.Item1, node.previousNodeCoords.Item2];

            if (node.previousNodeCoords.Item1 == -1)
                break;
        }

        OutputMessageManager.SetMessage(
            "Path found!\n" +
            "Precalculatoin:\n" +
            "Calculation:\n" + 
            "Length:\n" +
            "Nodes in Path: \n" +
            "Nodes Searched:",
            column: 1);

        OutputMessageManager.SetMessage(
            "\n" +
            (precalculation.HasValue ? precalculation.ToString() : "None") + " ms\n" +
            calulation + " ms\n" +
            length + "\n" +
            path + "\n" +
            searched,
            column: 2);
    }
    public void DrawLines(Node endNode, Node[,] map)
    {
        if (endNode == null)
            return;

        if (pathLinesRenderObject != null)
            Object.Destroy(pathLinesRenderObject);

        List<Vector3> points = new List<Vector3>();
        while (true)
        {
            Vector3 point = Paint.PixelToWorldCoordinates(endNode.x, endNode.y);
            point.z = 1;
            points.Add(point);
            endNode = map[endNode.previousNodeCoords.Item1, endNode.previousNodeCoords.Item2];

            if (endNode.previousNodeCoords.Item1 == -1)
                break;
        }

        pathLinesRenderObject = new GameObject("Line Renderer");
        LineRenderer line = pathLinesRenderObject.AddComponent<LineRenderer>();

        line.SetPositions(points.ToArray());
    }
}
