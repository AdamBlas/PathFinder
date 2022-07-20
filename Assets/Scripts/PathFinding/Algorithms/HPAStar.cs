using System.Collections;
using System.Text;
using UnityEngine;
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
        public Chunk previousChunk;

        public bool? canGoUp = null;
        public bool? canGoDown = null;
        public bool? canGoLeft = null;
        public bool? canGoRight = null;
        public bool? canGoUpLeft = null;
        public bool? canGoUpRight = null;
        public bool? canGoDownLeft = null;
        public bool? canGoDownRight = null;
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
        Debug.Log("Created chunk map");

        // Print chunk map to the .txt file
        PrintChunkMap(chunkMap);

        // Get last chunk in chunk name conviction (coords are not multiplies of chunkSize)
        Debug.Log("Applying A* on chunks...");
        Node lastChunk = ApplyAStarOnChunkMap(chunkMap, start, end, heuristic);
        Debug.Log("A* applied on chunks");

        // yield break;

        if (lastChunk== null)
            Debug.LogWarning("PATH NOT FOUND");

        Node chunk = lastChunk;
        // Apply color on display
        while (chunk != null)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int mapX = chunk.x * chunkSize;

                for (int y = 0; y < chunkSize; y++)
                {
                    int mapY = chunk.y * chunkSize;
                    
                    if (Map.RecentMap[mapX + x, mapY + y] == Map.Node.Free)
                    {
                        Debug.Log("X=" + mapX + ", Y=" + mapY + ", CONDITION=" + (mapX % 2 == mapY % 2));

                        if (mapX % (chunkSize * 2) == mapY % (chunkSize * 2))
                            ImageDisplayer.SetPixel(mapX + x, mapY + y, ImageDisplayer.Instance.subPathColor1);
                        else
                            ImageDisplayer.SetPixel(mapX + x, mapY + y, ImageDisplayer.Instance.subPathColor2);
                    }
                }
            }

            Debug.Log("CHUNK: " + chunk.x + " | " + chunk.y);
            chunk = chunk.previousNode;
        }

        
        // Go through every chunk
        chunk = lastChunk;
        Vector2Int startCoords = end;
        while (chunk != null)
        {
            // For every chunk, find path
            Node node = ApplyAStartOnChunk(startCoords, chunk, heuristic, start);

            if (node == null)
            {
                Debug.LogWarning("Can't find path inside chunk!");
                node = ApplyAStartOnChunk(startCoords, chunk, heuristic, start);
                break;
            }

            chunk = chunk.previousNode;

            // Next chunk's start coords are the the same as end in current one
            startCoords = new Vector2Int(node.x, node.y);
            while (node != null)
            {
                // If node is not start or end, color it
                if (!((node.x == start.x && node.y == start.y) || (node.x == end.x && node.y == end.y)))
                    ImageDisplayer.SetPixel(node.x, node.y, Map.Node.Path);
                node = node.previousNode;

                yield return null;
            }
        }
        

        ImageDisplayer.Instance.UpdateImage();
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
                // Bottom left node coords
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
                        // Init value in neighbour chunk
                        chunks[x, y - 1].canGoUp = false;

                        for (int xx = 0; xx < chunkSize; xx++)
                        {
                            // Check if we didn't go out of the map
                            if (mapX + xx >= Map.Width)
                                break;

                            // Check if every node in a lowest row (mapY is the lowest in chunk) can connect...
                            // ...with the highest node in chunk below
                            if (Map.RecentMap[mapX + xx, mapY] == Map.Node.Free &&
                                Map.RecentMap[mapX + xx, mapY - 1] == Map.Node.Free)
                            {
                                chunks[x, y].canGoDown = true;
                                chunks[x, y - 1].canGoUp = true;
                                break;
                            }
                        }
                    }
                }

                // If chunk direction wasn't set yet
                if (!chunks[x, y].canGoUp.HasValue)
                {
                    // Init value
                    chunks[x, y].canGoUp = false;

                    // If there is chunk above
                    if (y != chunks.GetLength(1) - 1)
                    {
                        // Init neighbour's value
                        chunks[x, y + 1].canGoDown = false;

                        for (int xx = 0; xx < chunkSize; xx++)
                        {
                            if (mapX + xx >= Map.Width)
                                break;

                            if (Map.RecentMap[mapX + xx, mapY + chunkSize - 1] == Map.Node.Free &&
                                Map.RecentMap[mapX + xx, mapY + chunkSize] == Map.Node.Free)
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
                    if (x != chunks.GetLength(0) - 1)
                    {
                        chunks[x + 1, y].canGoLeft = false;
                        for (int yy = 0; yy < chunkSize; yy++)
                        {
                            if (mapY + yy >= Map.Height)
                                break;

                            if (Map.RecentMap[mapX + chunkSize - 1, mapY + yy] == Map.Node.Free &&
                                Map.RecentMap[mapX + chunkSize, mapY + yy] == Map.Node.Free)
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
                            if (mapY + yy >= Map.Height)
                                break;

                            if (Map.RecentMap[mapX, mapY + yy] == Map.Node.Free &&
                                Map.RecentMap[mapX - 1, mapY + yy] ==  Map.Node.Free)
                            {
                                chunks[x, y].canGoLeft = true;
                                chunks[x - 1, y].canGoRight = true;
                                break;
                            }
                        }
                    }
                }


                // DIAGONAL

                if (!chunks[x, y].canGoUpRight.HasValue)
                {
                    chunks[x, y].canGoUpRight = false;
                    if (x != chunks.GetLength(0) - 1 &&
                        y != chunks.GetLength(1) - 1)
                    {
                        chunks[x + 1, y + 1].canGoDownLeft = false;

                        if (Map.RecentMap[mapX + chunkSize - 1,     mapY + chunkSize - 1]   == Map.Node.Free &&
                            Map.RecentMap[mapX + chunkSize,         mapY + chunkSize - 1]   == Map.Node.Free && 
                            Map.RecentMap[mapX + chunkSize - 1,     mapY + chunkSize]       == Map.Node.Free &&
                            Map.RecentMap[mapX + chunkSize,         mapY + chunkSize]       == Map.Node.Free)
                        {
                            chunks[x, y].canGoUpRight = true;
                            chunks[x + 1, y + 1].canGoDownLeft = true;
                        }
                    }
                }

                if (!chunks[x, y].canGoUpLeft.HasValue)
                {
                    chunks[x, y].canGoUpLeft = false;
                    if (x != 0 &&
                        y != chunks.GetLength(1) - 1)
                    {
                        chunks[x - 1, y + 1].canGoDownLeft = false;

                        if (Map.RecentMap[mapX,         mapY + chunkSize - 1]   == Map.Node.Free &&
                            Map.RecentMap[mapX - 1,     mapY + chunkSize - 1]   == Map.Node.Free &&
                            Map.RecentMap[mapX,         mapY + chunkSize]       == Map.Node.Free &&
                            Map.RecentMap[mapX - 1,     mapY + chunkSize]       == Map.Node.Free)
                        {
                            chunks[x, y].canGoUpLeft = true;
                            chunks[x - 1, y + 1].canGoDownRight = true;
                        }
                    }
                }

                if (!chunks[x, y].canGoDownRight.HasValue)
                {
                    chunks[x, y].canGoDownRight = false;
                    if (x != chunks.GetLength(0) - 1 &&
                        y != 0)
                    {
                        chunks[x + 1, y - 1].canGoUpLeft = false;

                        if (Map.RecentMap[mapX + chunkSize - 1,     mapY]       == Map.Node.Free &&
                            Map.RecentMap[mapX + chunkSize,         mapY]       == Map.Node.Free &&
                            Map.RecentMap[mapX + chunkSize - 1,     mapY - 1]   == Map.Node.Free &&
                            Map.RecentMap[mapX + chunkSize,         mapY - 1]   == Map.Node.Free)
                        {
                            chunks[x, y].canGoDownRight = true;
                            chunks[x + 1, y - 1].canGoUpLeft = true;
                        }
                    }
                }

                if (!chunks[x, y].canGoDownLeft.HasValue)
                {
                    chunks[x, y].canGoDownLeft = false;
                    if (x != 0 &&
                        y != 0)
                    {
                        chunks[x - 1, y - 1].canGoUpRight = false;

                        if (Map.RecentMap[mapX,         mapY]       == Map.Node.Free &&
                            Map.RecentMap[mapX - 1,     mapY]       == Map.Node.Free &&
                            Map.RecentMap[mapX,         mapY - 1]   == Map.Node.Free &&
                            Map.RecentMap[mapX - 1,     mapY - 1]   == Map.Node.Free)
                        {
                            chunks[x, y].canGoDownLeft = true;
                            chunks[x - 1, y - 1].canGoUpRight = true;
                        }
                    }
                }
            }
        }
    }
    Node ApplyAStarOnChunkMap(Chunk[,] chunks, Vector2Int start, Vector2Int end, Heuristic heuristic)
    {
        // Returns last node in a path

        // Convert full map coords to chunk map coords
        start /= 4;
        end /= 4;

        // Create list and init it with starting chunk
        // float startNodeValue = heuristic.GetNodeValue(null, start.x, start.y, 0);
        Node startNode = new Node(start.x, start.y, 0);
        SortedList list = new SortedList(startNode);
        List<(int, int)> visitedNodes = new List<(int, int)>();

        while (list.Count != 0)
        {
            list.Sort();
            Node node = list.GetAtZero();

            Debug.Log("RUNTIME LOG: ANALYZING x=" + node.x + ", y=" + node.y);

            if (node.x == end.x && node.y == end.y)
                return node;

            list.RemoveAtZero();
            

            if (chunks[node.x, node.y].canGoUp.Value)
            {
                if (!visitedNodes.Contains((node.x, node.y + 1)))
                {
                    float value = heuristic.GetNodeValue(node, node.x, node.y + 1, 1);
                    Node newNode = new Node(node.x, node.y + 1, value);
                    newNode.previousNode = node;
                    list.Add(newNode);
                    visitedNodes.Add((node.x, node.y + 1));
                }
            }

            if (chunks[node.x, node.y].canGoDown.Value)
            {
                if (!visitedNodes.Contains((node.x, node.y - 1)))
                {
                    float value = heuristic.GetNodeValue(node, node.x, node.y - 1, 1);
                    Node newNode = new Node(node.x, node.y - 1, value);
                    newNode.previousNode = node;
                    list.Add(newNode);
                    visitedNodes.Add((node.x, node.y - 1));
                }
            }

            if (chunks[node.x, node.y].canGoRight.Value)
            {
                if (!visitedNodes.Contains((node.x + 1, node.y)))
                {
                    float value = heuristic.GetNodeValue(node, node.x + 1, node.y, 1);
                    Node newNode = new Node(node.x + 1, node.y, value);
                    newNode.previousNode = node;
                    list.Add(newNode);
                    visitedNodes.Add((node.x + 1, node.y));
                }
            }

            if (chunks[node.x, node.y].canGoLeft.Value)
            {
                if (!visitedNodes.Contains((node.x - 1, node.y)))
                {
                    float value = heuristic.GetNodeValue(node, node.x - 1, node.y, 1);
                    Node newNode = new Node(node.x - 1, node.y, value);
                    newNode.previousNode = node;
                    list.Add(newNode);
                    visitedNodes.Add((node.x - 1, node.y));
                }
            }

            if (chunks[node.x, node.y].canGoUpRight.Value)
            {
                if (!visitedNodes.Contains((node.x + 1, node.y + 1)))
                {
                    float value = heuristic.GetNodeValue(node, node.x + 1, node.y + 1, 1.41421f);
                    Node newNode = new Node(node.x + 1, node.y + 1, value, node);
                    newNode.previousNode = node;
                    list.Add(newNode);
                    visitedNodes.Add((node.x + 1, node.y + 1));
                }
            }

            if (chunks[node.x, node.y].canGoUpLeft.Value)
            {
                if (!visitedNodes.Contains((node.x - 1, node.y + 1)))
                {
                    float value = heuristic.GetNodeValue(node, node.x - 1, node.y + 1, 1.41421f);
                    Node newNode = new Node(node.x - 1, node.y + 1, value, node);
                    newNode.previousNode = node;
                    list.Add(newNode);
                    visitedNodes.Add((node.x - 1, node.y + 1));
                }
            }

            if (chunks[node.x, node.y].canGoDownRight.Value)
            {
                if (!visitedNodes.Contains((node.x + 1, node.y - 1)))
                {
                    float value = heuristic.GetNodeValue(node, node.x + 1, node.y - 1, 1.41421f);
                    Node newNode = new Node(node.x + 1, node.y - 1, value, node);
                    newNode.previousNode = node;
                    list.Add(newNode);
                    visitedNodes.Add((node.x + 1, node.y - 1));
                }
            }

            if (chunks[node.x, node.y].canGoDownLeft.Value)
            {
                if (!visitedNodes.Contains((node.x - 1, node.y - 1)))
                {
                    float value = heuristic.GetNodeValue(node, node.x - 1, node.y - 1, 1.41421f);
                    Node newNode = new Node(node.x - 1, node.y - 1, value, node);
                    newNode.previousNode = node;
                    list.Add(newNode);
                    visitedNodes.Add((node.x - 1, node.y - 1));
                }
            }
        }

        return null;
    }
    Node ApplyAStartOnChunk(Vector2Int startNode, Node chunk, Heuristic heuristic, Vector2Int endNode)
    {
        int chunkXOffset;
        int chunkYOffset;
        if (chunk.previousNode != null)
        {
            // Info about offset will tell us end condition
            chunkXOffset = chunk.x - chunk.previousNode.x;
            chunkYOffset = chunk.y - chunk.previousNode.y;
        }
        else
        {
            chunkXOffset = 0;
            chunkYOffset = 0;
        }

        // Prepare list
        SortedList list = new SortedList(new Node(startNode.x, startNode.y, 0));
        List<(int, int)> nodesVisited = new List<(int, int)>();

        int index = 0;
        while (list.Count != 0)
        {
            // Secure loop
            if (index++ > chunkSize * chunkSize * 30)
            {
                Debug.LogWarning("Secure loop invoked");
                return null;
            }

            // Get node
            Node node = list.GetAtZero();
            list.RemoveAtZero();

            // Check end condition
            if (EndConditionMet(chunkXOffset, chunkYOffset, chunk, node.x, node.y))
                return node;

            // Calculate loop values
            int startI;
            int endI;
            int startJ;
            int endJ;

            startI = node.x == chunk.x * chunkSize ? 0 : -1;
            endI = node.x == chunk.x * chunkSize + chunkSize ? 0 : 1;

            startJ = node.y == chunk.y * chunkSize ? 0 : -1;
            endJ = node.y == chunk.y * chunkSize + chunkSize ? 0 : 1;

            // End condition not met, search nearby area
            for (int i = startI; i <= endI; i++)
            {
                for (int j = startJ; j <= endJ; j++)
                {
                    // Skip this node
                    if (i == 0 && j == 0)
                        continue;

                    // If node is outside of chunk, skip
                    if (!IsNodeInsideChunk(node.x + i, node.y + j, chunk.x, chunk.y))
                        continue;

                    // Check if node is inside map
                    if (node.x + i >= Map.Width || node.y + j >= Map.Height)
                        continue;

                    // Check if node wasn't visited before
                    if (nodesVisited.Contains((node.x + i, node.y + j)))
                        continue;
                    nodesVisited.Add((node.x + i, node.y + j));

                    if (Map.RecentMap[node.x + i, node.y + j] == Map.Node.Start)
                    {
                        Debug.Log("Start Node Found");
                        Node newNode = new Node(node.x + i, node.y + j, 0);
                        newNode.previousNode = node;
                        return newNode;
                    }

                    // Check if node is not obstacle
                    if (Map.RecentMap[node.x + i, node.y + j] == Map.Node.Free)
                    {
                        float offset = i == 0 || j == 0 ? 1 : 1.42f;
                        float value = heuristic.GetNodeValue(node, node.x + i, node.y + j, offset);
                        Node newNode = new Node(node.x + i, node.y + j, value);
                        newNode.previousNode = node;
                        list.Add(newNode);
                    }
                }
            }
            list.Sort();
        }

        // Path not found
        return null;
    }


    bool EndConditionMet(int chunkXOffset, int chunkYOffset, Node chunk, int nodeX, int nodeY)
    {
        int chunkX = chunk.x * chunkSize;
        int chunkY = chunk.y * chunkSize;

        if (chunkXOffset < 0)
        {
            // Goal is on the left
            
            if (chunkYOffset < 0)
            {
                // Top left corner
                return nodeX == chunkX + chunkSize - 1 &&
                       nodeY == chunkY + chunkSize - 1;

            }
            else if (chunkYOffset == 0)
            {
                // Left edge
                return nodeX == chunkX + chunkSize - 1;
            }
            else
            {
                // Bottom left corner
                return nodeX == chunkX + chunkSize - 1 &&
                       nodeY == chunkY;
            }
        }
        else if (chunkXOffset == 0)
        {
            // Goal is in the same column

            if (chunkYOffset < 0)
            {
                // Top edge
                return nodeY == chunkY + chunkSize - 1;
            }
            else if (chunkYOffset == 0)
            {
                return false;
            }
            else
            {
                // Bottom edge
                return nodeY == chunkY;
            }
        }
        else
        {
            // Goal is on the right
            if (chunkYOffset < 0)
            {
                // Top right corner
                return nodeX == chunkX &&
                       nodeY == chunkY + chunkSize - 1;
            }
            else if (chunkYOffset == 0)
            {
                // Right edge
                return nodeX == chunkX;
            }
            else
            {
                // Bottom right corner
                return nodeX == chunkX &&
                       nodeY == chunkY;
            }
        }
    }
    bool IsNodeInsideChunk(int nodeX, int nodeY, int chunkX, int chunkY)
    {
        return nodeX >= chunkX * chunkSize &&
               nodeX < (chunkX + 1) * chunkSize &&
               nodeY >= chunkY * chunkSize &&
               nodeY < (chunkY + 1) * chunkSize;
    }
    int RoundUp(float i)
    {
        return Mathf.CeilToInt(i / chunkSize) * chunkSize;
    }
    int RoundDown(float i)
    {
        return Mathf.FloorToInt(i / chunkSize) * chunkSize;
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

                if (map[x, y].canGoUpRight.Value)
                    stringArray[stringX + 5, stringY + 5] = "/";

                if (map[x, y].canGoUpLeft.Value)
                    stringArray[stringX, stringY + 5] = "\\";

                if (map[x, y].canGoDownRight.Value)
                    stringArray[stringX + 5, stringY] = "\\";

                if (map[x, y].canGoDownLeft.Value)
                    stringArray[stringX, stringY] = "/";
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
        // System.Diagnostics.Process.Start("chunkMap.txt");
    }
}
