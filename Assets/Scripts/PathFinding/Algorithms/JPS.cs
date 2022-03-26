using System.Collections;
using UnityEngine;
using static PathFinder;

public class JPS : Algorithm
{
    public JPS()
    {
        Name = "JPS";
        Description = "Jump Point Search - precalculates corner points and uses them as nodes. Comparing to A*, skips all nodes between two points if there is nothing between them.";
        AvaliableHeuristics = new Heuristic[]
        {
            Dijkstra.Instance,
            InversedDijkstra.Instance,
        };
    }

    public override IEnumerator Solve(Heuristic heuristic, Vector2Int start, Vector2Int end)
    {
        throw new System.NotImplementedException();
    }
}
