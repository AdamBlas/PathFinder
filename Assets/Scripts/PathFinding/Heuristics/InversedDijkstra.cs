using UnityEngine;
using static PathFinder;

public class InversedDijkstra : Heuristic
{
    public static InversedDijkstra Instance { get; private set; }

    public InversedDijkstra() : base()
    {
        Name = "Inversed Dijkstra";
        Description = "The best node is the one closest to the end.";
        Instance = this;
    }

    public override float GetNodeValue(Node previousNode, int x, int y, float offset)
    {
        return Mathf.Pow(EndCoordinate.x - x, 2) + Mathf.Pow(EndCoordinate.y - y, 2);
    }
}
