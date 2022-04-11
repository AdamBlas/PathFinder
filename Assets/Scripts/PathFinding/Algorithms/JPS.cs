using System.Collections;
using System.Text;
using UnityEngine;
using System.Diagnostics;
using static PathFinder;

public class JPS : Algorithm
{
    class DirectionBox
    {
        public bool N = false;
        public bool E = false;
        public bool S = false;
        public bool W = false;
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
    }

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
        DistanceBox[,] distances= null;

        sw.Start();
        PrecalcuateMap(ref directions, ref distances);
        sw.Stop();
        double precalculation = sw.Elapsed.TotalMilliseconds;


        PrintMap(directions, distances);



        OutputMessageManager.SetMessage("Precalculation: " + precalculation + " ms");
    }

    void PrecalcuateMap(ref DirectionBox[,] directions, ref DistanceBox[,] distances)
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

                    // TopRight Neighbour
                    if (x + 1 < Map.Width && y + 1 < Map.Height &&
                        Map.RecentMap[x + 1, y + 1] == Map.Node.Obstacle)
                    {
                        // Diagonal neighbour is obstacle

                        if (Map.RecentMap[x, y + 1] == Map.Node.Free &&
                            Map.RecentMap[x + 1, y] == Map.Node.Free)
                        {
                            box.N = true;
                            box.E = true;
                        }
                    }

                    // BottomRight Neighbour
                    if (x + 1 < Map.Width && y - 1 >= 0 &&
                        Map.RecentMap[x + 1, y - 1] == Map.Node.Obstacle)
                    {
                        // Diagonal neighbour is obstacle

                        if (Map.RecentMap[x, y - 1] == Map.Node.Free &&
                            Map.RecentMap[x + 1, y] == Map.Node.Free)
                        {
                            box.S = true;
                            box.E = true;
                        }
                    }

                    // BottomLeft Neigbour
                    if (x - 1 >= 0 && y - 1 >= 0 &&
                        Map.RecentMap[x - 1, y - 1] == Map.Node.Obstacle)
                    {
                        // Diagonal neighbour is obstacle

                        if (Map.RecentMap[x, y - 1] == Map.Node.Free &&
                            Map.RecentMap[x - 1, y] == Map.Node.Free)
                        {
                            box.S = true;
                            box.W = true;
                        }
                    }

                    // TopLeft Neighbour
                    if (x - 1 >= 0 && y + 1 < Map.Height &&
                        Map.RecentMap[x - 1, y + 1] == Map.Node.Obstacle)
                    {
                        // Diagonal neighbour is obstacle

                        if (Map.RecentMap[x, y + 1] == Map.Node.Free &&
                            Map.RecentMap[x - 1, y] == Map.Node.Free)
                        {
                            box.N = true;
                            box.W = true;
                        }
                    }

                    if (box.N == false && box.E == false && box.S == false && box.W == false)
                        directions[x, y] = null;
                    else
                        directions[x, y] = box;
                }
            }
        }

        distances = new DistanceBox[Map.Width, Map.Height];
        for (int x = 0; x < Map.Width; x++)
        {
            for (int y = 0; y < Map.Height; y++)
            {
                if (Map.RecentMap[x, y] == Map.Node.Obstacle)
                    continue;

                DistanceBox dist = new DistanceBox();

                // Toward +X
                int d = 0;
                while (true)
                {
                    if (x + d + 1 >= Map.Width ||
                        Map.RecentMap[x + d + 1, y] == Map.Node.Obstacle)
                    {
                        dist.E = -d;
                        break;
                    }

                    DirectionBox dir = directions[x + d + 1, y];
                    if (dir != null &&
                        dir.W == true)
                    {
                        dist.E = d + 1;
                        break;
                    }

                    d += 1;
                }

                // Toward +X +Y
                d = 0;
                while (true)
                {
                    if (x + d + 1 >= Map.Width ||
                        y + d + 1 >= Map.Height ||
                        Map.RecentMap[x + d + 1, y + d + 1] == Map.Node.Obstacle)
                    {
                        dist.NE = -d;
                        break;
                    }

                    DirectionBox dir = directions[x + d + 1, y + d + 1];
                    if (dir != null)
                    {
                        dist.NE = d + 1;
                        break;
                    }

                    d += 1;
                }

                // Toward +Y
                d = 0;
                while (true)
                {
                    if (y + d + 1 >= Map.Height ||
                        Map.RecentMap[x, y + d + 1] == Map.Node.Obstacle)
                    {
                        dist.N = -d;
                        break;
                    }

                    DirectionBox dir = directions[x, y + d + 1];
                    if (dir != null &&
                        dir.S == true)
                    {
                        dist.N = d + 1;
                        break;
                    }

                    d += 1;
                }

                // Toward -X +Y
                d = 0;
                while (true)
                {
                    if (x - d - 1 < 0 ||
                        y + d + 1 >= Map.Height ||
                        Map.RecentMap[x - d - 1, y + d + 1] == Map.Node.Obstacle)
                    {
                        dist.NW = -d;
                        break;
                    }

                    DirectionBox dir = directions[x - d - 1, y + d + 1];
                    if (dir != null)
                    {
                        dist.NW = d + 1;
                        break;
                    }

                    d += 1;
                }

                // Toward -X
                d = 0;
                while (true)
                {
                    if (x - d - 1 < 0 ||
                        Map.RecentMap[x - d - 1, y] == Map.Node.Obstacle)
                    {
                        dist.W = -d;
                        break;
                    }

                    DirectionBox dir = directions[x - d - 1, y];
                    if (dir != null &&
                        dir.E == true)
                    {
                        dist.W = d + 1;
                        break;
                    }

                    d += 1;
                }

                // Toward -X -Y
                d = 0;
                while (true)
                {
                    if (x - d - 1 < 0 ||
                        y - d - 1 < 0 ||
                        Map.RecentMap[x - d - 1, y - d - 1] == Map.Node.Obstacle)
                    {
                        dist.SW = -d;
                        break;
                    }

                    DirectionBox dir = directions[x - d - 1, y - d - 1];
                    if (dir != null)
                    {
                        dist.SW = d + 1;
                        break;
                    }

                    d += 1;
                }

                // Toward -Y
                d = 0;
                while (true)
                {
                    if (y - d - 1 < 0 ||
                        Map.RecentMap[x, y - d - 1] == Map.Node.Obstacle)
                    {
                        dist.S = -d;
                        break;
                    }

                    DirectionBox dir = directions[x, y - d - 1];
                    if (dir != null &&
                        dir.N == true)
                    {
                        dist.S = d + 1;
                        break;
                    }

                    d += 1;
                }

                // Toward +X -Y
                d = 0;
                while (true)
                {
                    if (x + d + 1 >= Map.Width ||
                        y - d - 1 < 0 ||
                        Map.RecentMap[x + d + 1, y - d - 1] == Map.Node.Obstacle)
                    {
                        dist.SE = -d;
                        break;
                    }

                    DirectionBox dir = directions[x + d + 1, y - d - 1];
                    if (dir != null)
                    {
                        dist.SE = d + 1;
                        break;
                    }

                    d += 1;
                }

                distances[x, y] = dist;
            }
        }
    }
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
                                if (directions[mapX, mapY] == null)
                                    stringArray[y, x] = " \t";
                                else
                                    stringArray[y, x] = "FN\t";
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
        for (int x = 0; x < Map.Height * 4; x++)
        {
            for (int y = (Map.Width * 4) - 1; y >= 0; y--)
            {
                content.Append(stringArray[x, y]);
            }
            content.AppendLine();
        }

        System.IO.File.WriteAllText("map.txt", content.ToString());
        System.Diagnostics.Process.Start("map.txt");
    }
}
