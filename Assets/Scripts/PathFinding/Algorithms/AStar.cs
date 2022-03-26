using System.Collections;
using UnityEngine;
using static PathFinder;

public class AStar : Algorithm
{
    public AStar()
    {
        Name = "A*";
        Description = "Calculates values of all neighbours and adds them to the ordered queue.";
        AvaliableHeuristics = new Heuristic[]
        {
            Dijkstra.Instance,
            InversedDijkstra.Instance,
            Manhattan.Instance,
        };
    }

    public override IEnumerator Solve(Heuristic heuristic, Vector2Int start, Vector2Int end)
    {
        Node lastNode = null;
        Node[,] nodes = new Node[Map.Width, Map.Height];
        PathFinder.SortedList list = new PathFinder.SortedList();

        for (int x = 0; x < Map.Width; x++)
            for (int y = 0; y < Map.Height; y++)
                nodes[x, y] = new Node(x, y, float.PositiveInfinity);

        list.Add(nodes[start.x, start.y]);
        nodes[start.x, start.y].previousNode = (-1, -1);
        bool endFound = false;
        int counter = 0;

        while (list.Count != 0)
        {
            if (IsPaused)
            {
                yield return null;
                continue;
            }

            list.Sort();
            Node node = list.GetAtZero();

            if (node.x == end.x && node.y == end.y)
                break;

            list.RemoveAtZero();
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
                    Map.RecentMap[newX, newY] = Map.Node.ToSearch;
                    float value = heuristic.GetNodeValue(node, node.x + dir.Item1, node.y + dir.Item2, dir.Item3);
                    Node newNode = new Node(newX, newY, value, (node.x, node.y));
                    list.Add(newNode);
                    nodes[newX, newY] = newNode;
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

                if (counter++ % 10 == 0)
                    yield return new WaitForSeconds(PathFinder.Instance.Delay);
            }

            if (endFound)
            {
                break;
            }
        }

        CreatePath(lastNode, nodes);
        PrintOutputData(lastNode, nodes);
    }
}
