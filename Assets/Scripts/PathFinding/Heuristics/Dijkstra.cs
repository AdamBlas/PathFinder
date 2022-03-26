using static PathFinder;

public class Dijkstra : Heuristic
{
    public static Dijkstra Instance { get; private set; }

    public Dijkstra() : base()
    {
        Name = "Dijkstra";
        Description = "The best node is the one closest to the start. Distance is calculated geometrically.";
        Instance = this;
    }

    public override float GetNodeValue(Node previousNode, int x, int y, float offset)
    {
        return previousNode.value + offset;
    }
}
