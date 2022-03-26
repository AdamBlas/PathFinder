using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Heuristic
{
    public string Name { get; protected set; }
    public string Description { get; protected set; }

    public abstract float GetNodeValue(PathFinder.Node previousNode, int x, int y, float offset);
}
