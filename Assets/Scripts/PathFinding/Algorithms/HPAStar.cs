using System.Collections;
using System.Text;
using UnityEngine;
using System.Diagnostics;
using static PathFinder;
using System.Collections.Generic;
using System.Linq;


public class HPAStar : Algorithm
{
    int chunkSize = 4;

    class Chunk
    {
        public int x;
        public int y;

        public bool? canGoUp = null;
        public bool? canGoDown = null;
        public bool? canGoLeft = null;
        public bool? canGoRight = null;
    }

    public HPAStar()
    {
        Name = "HPA*";
        Description = "Hierarchical Pathfinding A* - before applying traditional A*, map is divided into chunks, e.g. 4x4 nodes. Then, the path is searched in chunks instead of nodes (in 4x4 case, map is 16 times smaller). Then normal A* is applied only on chunks that are part of the path.";
        AvailableHeuristics = new Heuristic[]
        {
            Dijkstra.Instance,
            InversedDijkstra.Instance,
            Manhattan.Instance
        };
    }

    public override IEnumerator Solve(Heuristic heuristic, Vector2Int start, Vector2Int end)
    {
        Chunk[,] chunkMap = CreateChunkMap();
        PrintChunkMap(chunkMap);
        yield return null;
    }

    Chunk[,] CreateChunkMap()
    {
        int width = Mathf.CeilToInt((float)Map.Width / 4);
        int height = Mathf.CeilToInt((float)Map.Height / 4);

        Chunk[,] chunkMap = new Chunk[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                chunkMap[x, y] = new Chunk
                {
                    x = x * 4,
                    y = y * 4
                };
            }
        }

        CalculateChunkPassages(chunkMap);
        return chunkMap;
    }
    void CalculateChunkPassages(Chunk[,] chunks)
    {
        for (int x = 0; x < chunks.GetLength(0); x++)
        {
            for (int y = 0; y < chunks.GetLength(1); y++)
            {
                int mapX = x * 4;
                int mapY = y * 4;

                // If chunk direction wasn't set yet
                if (!chunks[x, y].canGoDown.HasValue)
                {
                    // Init value
                    chunks[x, y].canGoDown = false;

                    // Check if there is chunk below
                    if (y != 0)
                    {
                        chunks[x, y - 1].canGoUp = false;
                        for (int xx = 0; xx < chunkSize; xx++)
                        {
                            if (mapX + xx >= Map.Width)
                                break;

                            if (Map.RecentMap[mapX + xx, mapY] == Map.Node.Free &&
                                Map.RecentMap[mapX + xx, mapY - 1] == Map.Node.Free)
                            {
                                chunks[x, y].canGoDown = true;
                                chunks[x, y - 1].canGoUp = true;
                            }
                        }
                    }
                }

                if (!chunks[x, y].canGoUp.HasValue)
                {
                    chunks[x, y].canGoUp = false;
                    if (y < chunks.GetLength(1) - 2)
                    {
                        chunks[x, y + 1].canGoDown = false;
                        for (int xx = 0; xx < chunkSize; xx++)
                        {
                            if (mapX + xx >= Map.Width || mapY + chunkSize + 1 >= Map.Height)
                                break;

                            if (Map.RecentMap[mapX + xx, mapY + chunkSize] == Map.Node.Free &&
                                Map.RecentMap[mapX + xx, mapY + chunkSize + 1] == Map.Node.Free)
                            {
                                chunks[x, y].canGoUp = true;
                                chunks[x, y + 1].canGoDown = true;
                                break;
                            }
                        }
                    }
                }

                if (!chunks[x, y].canGoRight.HasValue)
                {
                    chunks[x, y].canGoRight = false;
                    if (x < chunks.GetLength(0) - 2)
                    {
                        chunks[x + 1, y].canGoLeft = false;
                        for (int yy = 0; yy < chunkSize; yy++)
                        {
                            if (mapX + 1 >= Map.Width || mapY + yy >= Map.Height)
                                break;

                            if (Map.RecentMap[mapX, mapY + yy] == Map.Node.Free &&
                                Map.RecentMap[mapX + 1, mapY + yy] == Map.Node.Free)
                            {
                                chunks[x, y].canGoRight = true;
                                chunks[x + 1, y].canGoLeft = true;
                                break;
                            }
                        }
                    }
                }

                if (!chunks[x, y].canGoLeft.HasValue)
                {
                    chunks[x, y].canGoLeft = false;
                    if (x != 0)
                    {
                        chunks[x - 1, y].canGoRight = false;
                        for (int yy = 0; yy < chunkSize; yy++)
                        {
                            if (mapX + chunkSize >= Map.Width || mapY + yy >= Map.Height)
                                break;

                            if (Map.RecentMap[mapX + chunkSize, mapY + yy] == Map.Node.Free &&
                                Map.RecentMap[mapX + chunkSize - 1, mapY + yy] ==  Map.Node.Free)
                            {
                                chunks[x, y].canGoLeft = true;
                                chunks[x - 1, y].canGoRight = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
    void PrintChunkMap(Chunk[,] map)
    {
        string[,] stringArray = new string[map.GetLength(0) * 6, map.GetLength(1) * 6];
    
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                int stringX = x * 6;
                int stringY = y * 6;

                for (int xx = 0; xx < 4; xx++)
                {
                    for (int yy = 0; yy < 4; yy++)
                    {
                        if (x * chunkSize + xx >= Map.Width ||
                            y * chunkSize + yy >= Map.Height)
                            break;

                        stringArray[stringX + xx + 1, stringY + yy + 1] = Map.RecentMap[x * chunkSize + xx, y * chunkSize + yy] == Map.Node.Obstacle ? "#" : "O";
                    }
                }

                
                if (map[x, y].canGoUp.Value)
                    for (int xx = 1; xx < 5; xx++)
                        stringArray[stringX + xx, stringY + 5] = "^";
                
                if (map[x, y].canGoDown.Value)
                    for (int xx = 1; xx < 5; xx++)
                        stringArray[stringX + xx, stringY] = "v";

                if (map[x, y].canGoLeft.Value)
                    for (int yy = 1; yy < 5; yy++)
                        stringArray[stringX, stringY + yy] = "<";

                if (map[x, y].canGoRight.Value)
                    for (int yy = 1; yy < 5; yy++)
                        stringArray[stringX + 5, stringY + yy] = ">";
            }
        }

        for (int i = 0; i < stringArray.GetLength(0); i++)
            for (int j = 0; j < stringArray.GetLength(1); j++)
                if (stringArray[i, j] == null)
                    stringArray[i, j] = " ";

        StringBuilder content = new StringBuilder();
        for (int y = stringArray.GetLength(1) - 1; y >= 0; y--)
        {
            for (int x = 0; x < stringArray.GetLength(0); x++)
            {
                if (x % 6 == 0)
                    content.Append("  ");
                content.Append(stringArray[x, y]);
            }
            if (y % 6 == 0)
                content.Append("\n\n");
            content.AppendLine();
        }

        System.IO.File.WriteAllText("chunkMap.txt", content.ToString());
        System.Diagnostics.Process.Start("chunkMap.txt");
    }
}
