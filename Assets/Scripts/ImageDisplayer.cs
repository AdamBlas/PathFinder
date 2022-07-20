using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageDisplayer : MonoBehaviour
{
    public Image image;
    public Color startColor;
    public Color endColor;
    public Color freeCellColor;
    public Color obstacleColor;
    public Color toSearchColor;
    public Color searchedColor;
    public Color pathColor;
    public Color subPathColor1;
    public Color subPathColor2;
    public Text mapDetails;
    public Toggle optimizeMapToggle;

    public static ImageDisplayer Instance { get; private set; }
    public static Image Image { get => Instance != null ? Instance.image : null; }
    public static Map Map { get => Instance.map; }
    Map map;
    Texture2D texture;

    void Awake()
    {
        Instance = this;
    }

    public static void LoadImage(Map map, bool reload = false)
    {
        if (Instance.optimizeMapToggle.isOn)
            map.Optimize();

        Instance.map = map;
        Instance.LoadImage_();

        Instance.mapDetails.text = "" +
            "Width:  " + Map.Width + "\n" +
            "Height: " + Map.Height;

        Paint.ResetPaint();
        if (!reload)
        {
            Paint.ResetCoords();
            //Paint.StartCoordinates = null;
            //Paint.EndCoordinates = null;
            PathFinder.UpdateButtonState();
        }
    }
    public static void RefreshPixel(int x, int y)
    {
        Instance.RefreshPixel_(x, y);
    }
    public static void RefreshPixel(Vector2Int? vector)
    {
        if (!vector.HasValue)
            return;

        Instance.RefreshPixel_(vector.Value.x, vector.Value.y);
    }
    public static void SetPixel(int x, int y, Map.Node node)
    {
        Instance.SetPixel_(x, y, node);
    }

    void LoadImage_()
    {
        texture = new Texture2D(Map.Width, Map.Height);
        texture.filterMode = FilterMode.Point;

        for (int x = 0; x < Map.Width; x++)
        {
            for (int y = 0; y < Map.Height; y++)
            {
                switch (map[x, y])
                {
                    case Map.Node.Start:
                        texture.SetPixel(x, y, startColor);
                        break;
                    case Map.Node.End:
                        texture.SetPixel(x, y, endColor);
                        break;
                    case Map.Node.Free:
                        texture.SetPixel(x, y, freeCellColor);
                        break;
                    case Map.Node.Obstacle:
                        texture.SetPixel(x, y, obstacleColor);
                        break;
                }
            }
        }

        UpdateImage();
    }
    void RefreshPixel_(int x, int y)
    {
        SetPixel_(x, y, map[x, y]);
    }
    void SetPixel_(int x, int y, Map.Node node)
    {
        switch (node)
        {
            case Map.Node.ToSearch:
                texture.SetPixel(x, y, toSearchColor);
                break;
            case Map.Node.Searched:
                texture.SetPixel(x, y, searchedColor);
                break;
            case Map.Node.Path:
                texture.SetPixel(x, y, pathColor);
                break;
            case Map.Node.Start:
                texture.SetPixel(x, y, startColor);
                break;
            case Map.Node.End:
                texture.SetPixel(x, y, endColor);
                break;
            case Map.Node.Free:
                texture.SetPixel(x, y, freeCellColor);
                break;
            case Map.Node.Obstacle:
                texture.SetPixel(x, y, obstacleColor);
                break;
        }

        UpdateImage();
    }
    public static void SetPixel(int x, int y, Color color)
    {
        Instance.texture.SetPixel(x, y, color);
    }
    public void UpdateImage()
    {
        texture.Apply();
        Rect rect = new Rect(0, 0, Map.Width, Map.Height);
        Sprite sprite = Sprite.Create(texture, rect, Vector2.zero);
        image.sprite = sprite;
    }
}
