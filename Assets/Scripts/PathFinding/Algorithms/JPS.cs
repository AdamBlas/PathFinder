using System.Collections;
using System.Text;
using UnityEngine;
using System.Diagnostics;
using static PathFinder;
using System.Collections.Generic;
using System.Linq;

public class JPS : Algorithm
{
    class DirectionBox
    {
        public bool N = false;
        public bool E = false;
        public bool S = false;
        public bool W = false;

        public bool GetDirection(int x, int y)
        {
            if (x == 0 && y == 1)
                return N;
            if (x == 0 && y == -1)
                return S;
            if (x == 1 && y == 0)
                return E;
            if (x == -1 && y == 0)
                return W;

            throw new System.ArgumentException("Either x or y should be 1 or -1, other should be 0. Given args: x = " + x + ", y = " + y);
        }
    }
    class DistanceBox
    {
        public int N;
        public int NE;
        public int E;
        public int SE;
        public int S;
        public int SW;
        public int W;
        public int NW;

        public int GetDistance(int x, int y)
        {
            if (x == 0 && y == 1)
                return N;
            if (x == 1 && y == 1)
                return NE;
            if (x == 1 && y == 0)
                return E;
            if (x == 1 && y == -1)
                return SE;
            if (x == 0 && y == -1)
                return S;
            if (x == -1 && y == -1)
                return SW;
            if (x == -1 && y == 0)
                return W;
            if (x == -1 && y == 1)
                return NW;

            throw new System.ArgumentException("X and Y must be from range (-1, 1) and only one can be 0. Given args: X=" + x + ", Y=" + y);
        }
    }

    Node lastNode;
    bool[,] isJumpPoint;

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
        yield return null;

        Stopwatch sw = new Stopwatch();
        DirectionBox[,] directions = null;
        DistanceBox[,] distances = null;

        isJumpPoint = new bool[Map.Width, Map.Height];
        for (int i = 0; i < Map.Width; i++)
            for (int j = 0; j < Map.Height; j++)
                isJumpPoint[i, j] = false;


        sw.Start();
        PrecalcuateMap(ref directions, ref distances);
        Node[,] nodes = new Node[Map.Width, Map.Height];
        for (int x = 0; x < Map.Width; x++)
        {
            for (int y = 0; y < Map.Height; y++)
            {
                nodes[x, y] = new Node(x, y, 0);
            }
        }
        sw.Stop();
        long precalculation = sw.ElapsedMilliseconds;

        sw.Start();
        Solve(distances, start, end, heuristic, ref nodes);
        sw.Stop();
        long solving = sw.ElapsedMilliseconds;

        PrintMap(directions, distances);

        CreatePath(lastNode, nodes);
        PrintOutputData(lastNode, nodes, precalculation, solving);
        //DrawLines(lastNode, nodes);
    }

    void PrecalcuateMap(ref DirectionBox[,] directions, ref DistanceBox[,] distances)
    {
        FindPrimaryJumpPoints(ref directions, ref distances);
        FindStraightJumpPoints(ref directions, ref distances);
        FindDiagonalJumpPoints(ref directions, ref distances);
    }

    void FindPrimaryJumpPoints(ref DirectionBox[,] directions, ref DistanceBox[,] distances)
    {
        directions = new DirectionBox[Map.Width, Map.Height];
        for (int x = 0; x < Map.Width; x++)
        {
            for (int y = 0; y < Map.Height; y++)
            {
                Map.Node node = Map.RecentMap[x, y];

                if (node != Map.Node.Obstacle)
                {
                    // X   = Column
                    // Y   = Row
                    // X+1 = Column on Right    |   X-1 = Column on Left
                    // Y+1 = Row Above          |   Y-1 = Row Below

                    DirectionBox box = new DirectionBox();

                    // TopRight
                    CheckForPrimaryJumpPoint(x, y, x + 1, y + 1, ref box.N, ref box.E);

                    // BottomRight
                    CheckForPrimaryJumpPoint(x, y, x + 1, y - 1, ref box.S, ref box.E);

                    // BottomLeft
                    CheckForPrimaryJumpPoint(x, y, x - 1, y - 1, ref box.S, ref box.W);

                    // TopLeft
                    CheckForPrimaryJumpPoint(x, y, x - 1, y + 1, ref box.N, ref box.W);

                    if (box.N == false && box.E == false && box.S == false && box.W == false)
                        directions[x, y] = null;
                    else
                    {
                        directions[x, y] = box;
                        isJumpPoint[x, y] = true;
                    }
                }
            }
        }
    }
    void CheckForPrimaryJumpPoint(int x, int y, int xWithOffset, int yWithOffset, ref bool flag1, ref bool flag2)
    {
        if (xWithOffset >= 0 && xWithOffset < Map.Width &&
            yWithOffset >= 0 && yWithOffset < Map.Height &&
            Map.RecentMap[xWithOffset, yWithOffset] == Map.Node.Obstacle)
        {
            // Diagonal neighbour is obstacle

            if (Map.RecentMap[x, yWithOffset] == Map.Node.Free &&
                Map.RecentMap[xWithOffset, y] == Map.Node.Free)
            {
                // Diagonal move is possible
                flag1 = true;
                flag2 = true;
            }
        }
    }

    void FindStraightJumpPoints(ref DirectionBox[,] directions, ref DistanceBox[,] distances)
    {
        distances = new DistanceBox[Map.Width, Map.Height];
        for (int x = 0; x < Map.Width; x++)
        {
            for (int y = 0; y < Map.Height; y++)
            {
                if (Map.RecentMap[x, y] == Map.Node.Obstacle)
                    continue;

                DistanceBox dist = new DistanceBox();

                // Toward positive X
                CheckForStraightJumpPoint(x, y, 1, 0, ref dist.E, directions);

                // Toward positive Y
                CheckForStraightJumpPoint(x, y, 0, 1, ref dist.N, directions);

                // Toward negative X
                CheckForStraightJumpPoint(x, y, -1, 0, ref dist.W, directions);

                // Toward negative Y
                CheckForStraightJumpPoint(x, y, 0, -1, ref dist.S, directions);

                distances[x, y] = dist;
            }
        }
    }
    void CheckForStraightJumpPoint(int x, int y, int xOffset, int yOffset, ref int flag, DirectionBox[,] directions)
    {
        int dx = 0;
        int dy = 0;
        while (true)
        {
            int newX = x + dx + xOffset;
            int newY = y + dy + yOffset;

            if (newX < 0 ||
                newX >= Map.Width ||
                newY < 0 ||
                newY >= Map.Height ||
                Map.RecentMap[newX, newY] == Map.Node.Obstacle)
            {
                flag = -Mathf.Abs(dx + dy);
                break;
            }

            DirectionBox dir = directions[newX, newY];
            if (dir != null &&
                ((dir.N && newY < y) ||
                 (dir.E && newX < x) ||
                 (dir.S && newY > y) ||
                 (dir.W && newX > x)))
            {
                flag = Mathf.Abs(dx + dy) + 1;
                isJumpPoint[x, y] = true;
                break;
            }

            dx += xOffset;
            dy += yOffset;
        }
    }

    void FindDiagonalJumpPoints(ref DirectionBox[,] directions, ref DistanceBox[,] distances)
    {
        for (int x = 0; x < Map.Width; x++)
        {
            for (int y = 0; y < Map.Height; y++)
            {
                if (Map.RecentMap[x, y] == Map.Node.Obstacle)
                    continue;

                DistanceBox dist = distances[x, y];
                if (dist == null)
                    dist = new DistanceBox();

                // Toward positive X, positive Y
                CheckForDiagonalJumpPoint(x, y, 1, 1, ref dist.NE, directions, distances);

                // Toward negative X, positive Y
                CheckForDiagonalJumpPoint(x, y, -1, 1, ref dist.NW, directions, distances);

                // Toward negative X, negative Y
                CheckForDiagonalJumpPoint(x, y, -1, -1, ref dist.SW, directions, distances);

                // Toward positive X, negative Y
                CheckForDiagonalJumpPoint(x, y, 1, -1, ref dist.SE, directions, distances);

                distances[x, y] = dist;
            }
        }
    }
    void CheckForDiagonalJumpPoint(int x, int y, int xOffset, int yOffset, ref int flag, DirectionBox[,] directions, DistanceBox[,] distances)
    {
        int dx = 0;
        int dy = 0;
        while (true)
        {
            int newX = x + dx;
            int newY = y + dy;

            if (newX < 0 ||
                newX >= Map.Width ||
                newX + xOffset < 0 ||
                newX + xOffset >= Map.Width ||
                newY < 0 ||
                newY >= Map.Height ||
                newY + yOffset < 0 ||
                newY + yOffset >= Map.Height ||
                Map.RecentMap[newX + xOffset, newY + yOffset] == Map.Node.Obstacle ||
                Map.RecentMap[newX + xOffset, newY] == Map.Node.Obstacle ||
                Map.RecentMap[newX, newY + yOffset] == Map.Node.Obstacle)
            {
                // Abs(dx) and Abs(dy) are identical, so it doesn't matter which value we use
                flag = -Mathf.Abs(dx);
                break;
            }

            // If distance box is not null (we met jump point) and movement in that direction or cardinal directions is possible
            DistanceBox dist = distances[newX + xOffset, newY + yOffset];
            if (dist != null && (dist.GetDistance(xOffset, 0) > 0 || dist.GetDistance(0, yOffset) > 0))
            {
                flag = Mathf.Abs(dx) + 1;
                break;
            }

            dx += xOffset;
            dy += yOffset;
        }
    }

    void Solve(DistanceBox[,] distances, Vector2Int start, Vector2Int end, Heuristic heuristic, ref Node[,] nodes)
    {
        SortedList list = new SortedList(new Node(start.x, start.y, 0));
        List<(int, int)> visitedNodes = new List<(int, int)>();
        bool pathFound = false;
        Node previousNode = new Node(-1, -1, -1);
        Node currentNode = null;

        int i = 0;

        while (list.Count != 0)
        {
            if (i == Map.Size)
                break;

            list.Sort();
            currentNode = list.GetAtZero();
            nodes[currentNode.x, currentNode.y] = currentNode;

            if (i != 0)
                ImageDisplayer.SetPixel(currentNode.x, currentNode.y, Map.Node.Searched);
            
            list.RemoveAtZero();

            if (currentNode.x == end.x && currentNode.y == end.y)
            {
                pathFound = true;
                break;
            }
            DistanceBox distanceBox = distances[currentNode.x, currentNode.y];

            int xDiff = currentNode.x - end.x;
            int yDiff = currentNode.y - end.y;
            int xDiffAbs = Mathf.Abs(xDiff);
            int yDiffAbs = Mathf.Abs(yDiff);

            // Check end node position
            if (currentNode.y == end.y)
            {
                // We are in the same row as the end node
                if (Mathf.Abs(distanceBox.E) >= xDiffAbs || Mathf.Abs(distanceBox.W) >= xDiffAbs)
                {
                    // Distance to the end is less than distance to the wall
                    pathFound = true;
                    break;
                }
            }
            else if (currentNode.x == end.x)
            {
                // We are in the same column as the end node
                if (Mathf.Abs(distanceBox.N) >= yDiffAbs || Mathf.Abs(distanceBox.S) >= yDiffAbs)
                {
                    // Distance to the end is less than distance to the wall
                    pathFound = true;
                    break;
                }
            }
            else if (xDiffAbs == yDiffAbs)
            {
                // End node is in the diagonal direction

                if (xDiff > 0)
                {
                    // End node is in the east

                    if (yDiff > 0)
                    {
                        // End node is in the north-east

                        if (Mathf.Abs(distanceBox.NE) >= xDiffAbs)
                        {
                            // There is no wall between end node and current position
                            pathFound = true;
                            break;
                        }
                    }
                    else
                    {
                        // End node is in the south-east

                        if (Mathf.Abs(distanceBox.SE) >= xDiffAbs)
                        {
                            // There is no wall between end node and current position
                            pathFound = true;
                            break;
                        }
                    }
                }
                else
                {
                    // End node is in the west

                    if (yDiff > 0)
                    {
                        // End node is in the north-west

                        if (Mathf.Abs(distanceBox.NW) >= xDiffAbs)
                        {
                            // There is no wall between end node and current position
                            pathFound = true;
                            break;
                        }
                    }
                    else
                    {
                        // End node is in the south-west

                        if (Mathf.Abs(distanceBox.SW) >= xDiffAbs)
                        {
                            // There is no wall between end node and current position
                            pathFound = true;
                            break;
                        }
                    }
                }
            }

            float value;
            (int, int) currNodeCoords = (currentNode.x, currentNode.y);
            Node newNode;

            if (distanceBox.N > 0)
            {
                value = currentNode.value + heuristic.GetNodeValue(currentNode, currentNode.x, currentNode.y + distanceBox.N, distanceBox.N);
                newNode = new Node(currentNode.x, currentNode.y + distanceBox.N, value, currNodeCoords);

                if (visitedNodes.Any(node => node.Item1 == newNode.x && node.Item2 == newNode.y) == false)
                {
                    visitedNodes.Add((newNode.x, newNode.y));
                    list.Add(newNode);
                }
            }
            else
            {
                // Create target jump points
                if (yDiffAbs <= Mathf.Abs(distanceBox.NE))
                {
                    value = currentNode.value + heuristic.GetNodeValue(currentNode, currentNode.x + yDiffAbs, currentNode.y + yDiffAbs, yDiffAbs);
                    newNode = new Node(currentNode.x + yDiffAbs, currentNode.y + yDiffAbs, value, currNodeCoords);
                    if (visitedNodes.Any(node => node.Item1 == newNode.x && node.Item2 == newNode.y) == false)
                    {
                        visitedNodes.Add((newNode.x, newNode.y));
                        list.Add(newNode);
                    }
                }
                if (yDiffAbs <= Mathf.Abs(distanceBox.NW))
                {
                    value = currentNode.value + heuristic.GetNodeValue(currentNode, currentNode.x - yDiffAbs, currentNode.y + yDiffAbs, yDiffAbs);
                    newNode = new Node(currentNode.x - yDiffAbs, currentNode.y + yDiffAbs, value, currNodeCoords);
                    if (visitedNodes.Any(node => node.Item1 == newNode.x && node.Item2 == newNode.y) == false)
                    {
                        visitedNodes.Add((newNode.x, newNode.y));
                        list.Add(newNode);
                    }
                }
            }


            if (distanceBox.NE > 0)
            {
                value = currentNode.value + heuristic.GetNodeValue(currentNode, currentNode.x + distanceBox.NE, currentNode.y + distanceBox.NE, distanceBox.NE);
                newNode = new Node(currentNode.x + distanceBox.NE, currentNode.y + distanceBox.NE, value, currNodeCoords);

                if (visitedNodes.Any(node => node.Item1 == newNode.x && node.Item2 == newNode.y) == false)
                {
                    visitedNodes.Add((newNode.x, newNode.y));
                    list.Add(newNode);
                }
            }


            if (distanceBox.E > 0)
            {
                value = currentNode.value + heuristic.GetNodeValue(currentNode, currentNode.x + distanceBox.E, currentNode.y, distanceBox.E);
                newNode = new Node(currentNode.x + distanceBox.E, currentNode.y, value, currNodeCoords);

                if (visitedNodes.Any(node => node.Item1 == newNode.x && node.Item2 == newNode.y) == false)
                {
                    visitedNodes.Add((newNode.x, newNode.y));
                    list.Add(newNode);
                }
            }
            else
            {
                // Create target jump points
                if (yDiffAbs <= Mathf.Abs(distanceBox.NE))
                {
                    value = currentNode.value + heuristic.GetNodeValue(currentNode, currentNode.x + yDiffAbs, currentNode.y + yDiffAbs, yDiffAbs);
                    newNode = new Node(currentNode.x + yDiffAbs, currentNode.y + yDiffAbs, value, currNodeCoords);
                    if (visitedNodes.Any(node => node.Item1 == newNode.x && node.Item2 == newNode.y) == false)
                    {
                        visitedNodes.Add((newNode.x, newNode.y));
                        list.Add(newNode);
                    }
                }
                if (yDiffAbs <= Mathf.Abs(distanceBox.SE))
                {
                    value = currentNode.value + heuristic.GetNodeValue(currentNode, currentNode.x + yDiffAbs, currentNode.y - yDiffAbs, yDiffAbs);
                    newNode = new Node(currentNode.x + yDiffAbs, currentNode.y - yDiffAbs, value, currNodeCoords);
                    if (visitedNodes.Any(node => node.Item1 == newNode.x && node.Item2 == newNode.y) == false)
                    {
                        visitedNodes.Add((newNode.x, newNode.y));
                        list.Add(newNode);
                    }
                }
            }


            if (distanceBox.SE > 0)
            {
                value = currentNode.value + heuristic.GetNodeValue(currentNode, currentNode.x + distanceBox.SE, currentNode.y - distanceBox.SE, distanceBox.SE);
                newNode = new Node(currentNode.x + distanceBox.SE, currentNode.y - distanceBox.SE, value, currNodeCoords);

                if (visitedNodes.Any(node => node.Item1 == newNode.x && node.Item2 == newNode.y) == false)
                {
                    visitedNodes.Add((newNode.x, newNode.y));
                    list.Add(newNode);
                }
            }


            if (distanceBox.S > 0)
            {
                value = currentNode.value + heuristic.GetNodeValue(currentNode, currentNode.x, currentNode.y - distanceBox.S, distanceBox.S);
                newNode = new Node(currentNode.x, currentNode.y - distanceBox.S, value, currNodeCoords);

                if (visitedNodes.Any(node => node.Item1 == newNode.x && node.Item2 == newNode.y) == false)
                {
                    visitedNodes.Add((newNode.x, newNode.y));
                    list.Add(newNode);
                }
            }
            else
            {
                // Create target jump points
                if (yDiffAbs <= Mathf.Abs(distanceBox.SE))
                {
                    value = currentNode.value + heuristic.GetNodeValue(currentNode, currentNode.x + yDiffAbs, currentNode.y - yDiffAbs, yDiffAbs);
                    newNode = new Node(currentNode.x + yDiffAbs, currentNode.y - yDiffAbs, value, currNodeCoords);
                    if (visitedNodes.Any(node => node.Item1 == newNode.x && node.Item2 == newNode.y) == false)
                    {
                        visitedNodes.Add((newNode.x, newNode.y));
                        list.Add(newNode);
                    }
                }
                if (yDiffAbs <= Mathf.Abs(distanceBox.SW))
                {
                    value = currentNode.value + heuristic.GetNodeValue(currentNode, currentNode.x - yDiffAbs, currentNode.y - yDiffAbs, yDiffAbs);
                    newNode = new Node(currentNode.x - yDiffAbs, currentNode.y - yDiffAbs, value, currNodeCoords);
                    if (visitedNodes.Any(node => node.Item1 == newNode.x && node.Item2 == newNode.y) == false)
                    {
                        visitedNodes.Add((newNode.x, newNode.y));
                        list.Add(newNode);
                    }
                }
            }


            if (distanceBox.SW > 0)
            {
                value = currentNode.value + heuristic.GetNodeValue(currentNode, currentNode.x - distanceBox.SW, currentNode.y - distanceBox.SW, distanceBox.SW);
                newNode = new Node(currentNode.x - distanceBox.SW, currentNode.y - distanceBox.SW, value, currNodeCoords);

                if (visitedNodes.Any(node => node.Item1 == newNode.x && node.Item2 == newNode.y) == false)
                {
                    visitedNodes.Add((newNode.x, newNode.y));
                    list.Add(newNode);
                }
            }


            if (distanceBox.W > 0)
            {
                value = currentNode.value + heuristic.GetNodeValue(currentNode, currentNode.x - distanceBox.E, currentNode.y, distanceBox.W);
                newNode = new Node(currentNode.x - distanceBox.W, currentNode.y, value, currNodeCoords);

                if (visitedNodes.Any(node => node.Item1 == newNode.x && node.Item2 == newNode.y) == false)
                {
                    visitedNodes.Add((newNode.x, newNode.y));
                    list.Add(newNode);
                }
            }
            else
            {
                // Create target jump points
                if (yDiffAbs <= Mathf.Abs(distanceBox.NW))
                {
                    value = currentNode.value + heuristic.GetNodeValue(currentNode, currentNode.x - yDiffAbs, currentNode.y + yDiffAbs, yDiffAbs);
                    newNode = new Node(currentNode.x - yDiffAbs, currentNode.y + yDiffAbs, value, currNodeCoords);
                    if (visitedNodes.Any(node => node.Item1 == newNode.x && node.Item2 == newNode.y) == false)
                    {
                        visitedNodes.Add((newNode.x, newNode.y));
                        list.Add(newNode);
                    }
                }
                if (yDiffAbs <= Mathf.Abs(distanceBox.SW))
                {
                    value = currentNode.value + heuristic.GetNodeValue(currentNode, currentNode.x - yDiffAbs, currentNode.y - yDiffAbs, yDiffAbs);
                    newNode = new Node(currentNode.x - yDiffAbs, currentNode.y - yDiffAbs, value, currNodeCoords);
                    if (visitedNodes.Any(node => node.Item1 == newNode.x && node.Item2 == newNode.y) == false)
                    {
                        visitedNodes.Add((newNode.x, newNode.y));
                        list.Add(newNode);
                    }
                }
            }


            if (distanceBox.NW > 0)
            {
                value = currentNode.value + heuristic.GetNodeValue(currentNode, currentNode.x - distanceBox.NW, currentNode.y + distanceBox.NW, distanceBox.NW);
                newNode = new Node(currentNode.x - distanceBox.NW, currentNode.y + distanceBox.NW, value, currNodeCoords);

                if (visitedNodes.Any(node => node.Item1 == newNode.x && node.Item2 == newNode.y) == false)
                {
                    visitedNodes.Add((newNode.x, newNode.y));
                    list.Add(newNode);
                }
            }
        }

        if (pathFound)
        {
            lastNode = currentNode;
        }
    }

    /// <summary>
    /// Saves Map to .txt file
    /// </summary>
    void PrintMap(DirectionBox[,] directions, DistanceBox[,] map)
    {
        string[,] stringArray = new string[Map.Height * 4, Map.Width * 4];

        for (int y = 0; y < Map.Height * 4; y++)
        {
            for (int x = 0; x < Map.Width * 4; x++)
            {
                int mapX = x / 4;
                int mapY = y / 4;
                int modX = x % 4;
                int modY = y % 4;

                if (modX == 3)
                {
                    stringArray[y, x] = " || ";
                }
                else if (modY == 3)
                {
                    stringArray[y, x] = "====";
                }
                else
                {
                    if (map[mapX, mapY] == null)
                        stringArray[y, x] = "XXX\t";
                    else
                    {
                        if (modX == 0)
                        {
                            if (modY == 0)
                                stringArray[y, x] = map[mapX, mapY].SW.ToString() + "\t";
                            else if (modY == 1)
                                stringArray[y, x] = map[mapX, mapY].W.ToString() + "\t";
                            else
                                stringArray[y, x] = map[mapX, mapY].NW.ToString() + "\t";
                        }
                        else if (modX == 1)
                        {
                            if (modY == 0)
                                stringArray[y, x] = map[mapX, mapY].S.ToString() + "\t";
                            else if (modY == 1)
                            {
                                if (isJumpPoint[mapX, mapY])
                                    stringArray[y, x] = "JP\t";
                                else
                                    stringArray[y, x] = " \t";
                            }
                            else
                                stringArray[y, x] = map[mapX, mapY].N.ToString() + "\t";
                        }
                        else
                        {
                            if (modY == 0)
                                stringArray[y, x] = map[mapX, mapY].SE.ToString() + "\t";
                            else if (modY == 1)
                                stringArray[y, x] = map[mapX, mapY].E.ToString() + "\t";
                            else
                                stringArray[y, x] = map[mapX, mapY].NE.ToString() + "\t";
                        }
                    }
                }
            }
        }

        StringBuilder content = new StringBuilder();
        for (int x = (Map.Height * 4) - 1; x >= 0; x--)
        {
            for (int y = 0; y < Map.Width * 4; y++)
            {
                content.Append(stringArray[x, y]);
            }
            content.AppendLine();
        }

        System.IO.File.WriteAllText("map.txt", content.ToString());
        System.Diagnostics.Process.Start("map.txt");
    }
}
