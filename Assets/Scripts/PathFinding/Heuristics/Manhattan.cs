using static PathFinder;

public class Manhattan : Heuristic
{
    public static Manhattan Instance { get; private set; }

    public Manhattan() : base()
    {
        Name = "Manhattan";
        Description = "The best node is the one closest to the start. Distance is calculated along the edges.";
        Instance = this;
    }

    public override float GetNodeValue(Node previousNode, int x, int y, float offset)
    {
        offset = offset == 1 ? 1 : 2;
        return previousNode.value + offset;
    }
}
