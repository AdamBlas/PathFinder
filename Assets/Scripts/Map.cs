using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map
{
    public enum Node
    {
        Start,
        End,
        Free,
        Obstacle,
        ToSearch,
        Searched,
        Path,
    }

    static Node[,] map;
    public static int Width { get; private set; }
    public static int Height { get; private set; }
    public static Map RecentMap { get; private set; }

    public Map(int width, int height)
    {
        Width = width;
        Height = height;
        map = new Node[width, height];

        RecentMap = this;
    }

    public Node this[int x, int y]
    {
        get => map[x, Height - 1 - y];
        set => map[x, Height - 1 - y] = value;
    }

    public void Optimize()
    {
        int xMin = 0;
        int xMax = Width - 1;
        int yMin = 0;
        int yMax = Height - 1;

        for (int row = 0; row < Height; row++)
        {
            for (int col = 0; col < Width; col++)
            {
                if (map[col, row] == Node.Free)
                {
                    xMin = row;
                    goto xMinFound;
                }
            }
        }
        xMinFound:

        for (int row = Height - 1; row >= 0; row--)
        {
            for (int col = 0; col < Width; col++)
            {
                if (map[col, row] == Node.Free)
                {
                    xMax = row + 1;
                    goto xMaxFound;
                }
            }
        }
        xMaxFound:

        for (int col = 0; col < Width; col++)
        {
            for (int row = 0; row < Height; row++)
            {
                if (map[col, row] == Node.Free)
                {
                    yMin = col;
                    goto yMinFound;
                }
            }
        }
        yMinFound:

        for (int col = Width - 1; col >= 0; col--)
        {
            for (int row = 0; row < Height; row++)
            {
                if (map[col, row] == Node.Free)
                {
                    yMax = col + 1;
                    goto yMaxFound;
                }
            }
        }
        yMaxFound:

        Width = yMax - yMin;
        Height = xMax - xMin;
        Node[,] newMap = new Node[Width, Height];
        for (int row = 0; row < Height; row++)
        {
            for (int col = 0; col < Width; col++)
            {
                newMap[col, row] = map[yMin + col, xMin+ row];
            }
        }
        map = newMap;
    }
}
