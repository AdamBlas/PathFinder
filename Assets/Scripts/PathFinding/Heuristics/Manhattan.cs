using System;
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
        float xOffset = Math.Abs(previousNode.x - x);
        float yOffset = Math.Abs(previousNode.y - y);
        return previousNode.value + xOffset + yOffset;
    }
}
