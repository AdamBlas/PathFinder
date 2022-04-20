using System.Collections;
using UnityEngine;
using static PathFinder;

public class Hillclimb : Algorithm
{
    public Hillclimb()
    {
        Name = "Hillclimb";
        Description = "Checks neighbours and moves if found any node with better value. " +
                      "There is risk of getting stuck in local extremum.";
        AvaliableHeuristics = new Heuristic[]
        {
            InversedDijkstra.Instance,
        };
    }

    public override IEnumerator Solve(Heuristic heuristic, Vector2Int start, Vector2Int end)
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        Node lastNode = null;
        Node[,] nodes = new Node[Map.Width, Map.Height];

        for (int x = 0; x < Map.Width; x++)
            for (int y = 0; y < Map.Height; y++)
                nodes[x, y] = new Node(x, y, float.PositiveInfinity);

        nodes[StartCoordinate.x, StartCoordinate.y].previousNode = (-1, -1);
        bool endFound = false;

        Node node = nodes[StartCoordinate.x, StartCoordinate.y];
        while (true)
        {
            if (IsPaused)
            {
                yield return null;
                continue;
            }

            if (node.x == EndCoordinate.x && node.y == EndCoordinate.y)
                break;

            foreach (var dir in Directions)
            {
                int newX = node.x + dir.Item1;
                if (newX < 0 || newX >= Map.Width)
                    continue;

                int newY = node.y + dir.Item2;
                if (newY < 0 || newY >= Map.Height)
                    continue;

                Map.Node type = Map.RecentMap[newX, newY];
                if (type == Map.Node.Free)
                {
                    float value = heuristic.GetNodeValue(node, node.x + dir.Item1, node.y + dir.Item2, dir.Item3);
                    if (value < node.value)
                    {
                        Node newNode = new Node(newX, newY, value, (node.x, node.y));
                        nodes[newX, newY] = newNode;
                        node = newNode;

                        break;
                    }
                }
                else if (type == Map.Node.End)
                {
                    lastNode = node;
                    endFound = true;
                    break;
                }
            }

            if (!PathFinder.Instance.Fast)
            {
                Map.RecentMap[node.x, node.y] = Map.Node.Searched;
                ImageDisplayer.RefreshPixel(node.x, node.y);
                yield return new WaitForSeconds(PathFinder.Instance.Delay);
            }

            if (endFound)
            {
                break;
            }
        }
        sw.Stop();

        CreatePath(lastNode, nodes);
        PrintOutputData(lastNode, nodes, null, sw.ElapsedMilliseconds);
    }
}
