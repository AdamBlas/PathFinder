using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Paint : MonoBehaviour
{
    static Paint instance;

    public Text startLabel;
    public Text endLabel;
    public Color okCoordsColor;
    public Color noCoordsColor;

    Vector2 topLeft;
    Vector2 widthHeight;
    Vector2Int? lastCoordinates = null;
    Map.Node? brush;

    Vector2Int? _startCoordinates = null;
    public static Vector2Int? StartCoordinates
    {
        get => instance._startCoordinates;
        set
        {
            if (value == null)
            {
                StartEndMarker.DisableStartMarker();

                instance.startLabel.text = "Start point\nnot set";
                instance.startLabel.alignment = TextAnchor.UpperCenter;
                instance.startLabel.color = instance.noCoordsColor;

                if (instance._startCoordinates.HasValue)
                {
                    Map.RecentMap[instance._startCoordinates.Value.x, instance._startCoordinates.Value.y] = Map.Node.Free;
                    ImageDisplayer.RefreshPixel(instance._startCoordinates.Value.x, instance._startCoordinates.Value.y);
                }
            }
            else
            {
                StartEndMarker.SetStartMarkerProperties(value.Value.x, value.Value.y);

                instance.startLabel.text = "X: " + value.Value.x + "\nY: " + value.Value.y;
                instance.startLabel.alignment = TextAnchor.UpperLeft;
                instance.startLabel.color = instance.okCoordsColor;

                Map.RecentMap[value.Value.x, value.Value.y] = Map.Node.Start;
                ImageDisplayer.RefreshPixel(value.Value.x, value.Value.y);
            }

            instance._startCoordinates = value;
        }
    }
    Vector2Int? _endCoordinates = null;
    public static Vector2Int? EndCoordinates
    {
        get => instance._endCoordinates;
        set
        {
            if (value == null)
            {
                StartEndMarker.DisableEndMarker();

                instance.endLabel.text = "End point\nnot set";
                instance.endLabel.alignment = TextAnchor.UpperCenter;
                instance.endLabel.color = instance.noCoordsColor;

                if (instance._endCoordinates.HasValue)
                {
                    Map.RecentMap[instance._endCoordinates.Value.x, instance._endCoordinates.Value.y] = Map.Node.Free;
                    ImageDisplayer.RefreshPixel(instance._endCoordinates.Value.x, instance._endCoordinates.Value.y);
                }
            }
            else
            {
                StartEndMarker.SetEndMarkerProperties(value.Value.x, value.Value.y);

                instance.endLabel.text = "X: " + value.Value.x + "\nY: " + value.Value.y;
                instance.endLabel.alignment = TextAnchor.UpperLeft;
                instance.endLabel.color = instance.okCoordsColor;

                Map.RecentMap[value.Value.x, value.Value.y] = Map.Node.End;
                ImageDisplayer.RefreshPixel(value.Value.x, value.Value.y);
            }

            instance._endCoordinates = value;
        }
    }

    void Awake()
    {
        instance = this;
    }
    public void Update()
    {
        if (brush != null)
        {
            Map.Node brush = (Map.Node)this.brush;

            bool insideImage = GetPixelCoordinates(out Vector2Int coordinates);
            bool clicked = Input.GetMouseButton(0);

            if (!insideImage)
            {
                if (lastCoordinates.HasValue)
                {
                    ImageDisplayer.RefreshPixel(lastCoordinates.Value.x, lastCoordinates.Value.y);
                }

                if (clicked)
                {
                    this.brush = null;
                    lastCoordinates = null;
                }

                return;
            }

            int x = coordinates.x;
            int y = coordinates.y;

            if (lastCoordinates != coordinates)
            {
                if (lastCoordinates.HasValue)
                {
                    ImageDisplayer.RefreshPixel(lastCoordinates.Value.x, lastCoordinates.Value.y);
                }
                ImageDisplayer.SetPixel(x, y, brush);
            }

            if (clicked)
            {
                if (ImageDisplayer.Map[x, y] != brush)
                {
                    ImageDisplayer.Map[x, y] = brush;
                    ImageDisplayer.RefreshPixel(x, y);

                    if (brush == Map.Node.Start || brush == Map.Node.End)
                    {
                        if (brush == Map.Node.Start)
                        {
                            if (StartCoordinates != null)
                            {
                                Vector2Int startCoordinates = (Vector2Int)StartCoordinates;
                                ImageDisplayer.Map[startCoordinates.x, startCoordinates.y] = Map.Node.Free;
                                ImageDisplayer.RefreshPixel(startCoordinates.x, startCoordinates.y);
                            }

                            StartCoordinates = coordinates;
                            StartEndMarker.SetStartMarker(coordinates.x, coordinates.y);
                        }
                        else
                        {
                            if (EndCoordinates != null)
                            {
                                Vector2Int endCoordinates = (Vector2Int)EndCoordinates;
                                ImageDisplayer.Map[endCoordinates.x, endCoordinates.y] = Map.Node.Free;
                                ImageDisplayer.RefreshPixel(endCoordinates.x, endCoordinates.y);
                            }

                            EndCoordinates = coordinates;
                            StartEndMarker.SetEndMarker(coordinates.x, coordinates.y);
                        }

                        lastCoordinates = null;
                    }
                }
            }

            lastCoordinates = coordinates;
        }
    }

    public static void ResetPaint()
    {
        float x = ImageDisplayer.Image.rectTransform.anchoredPosition.x;
        float y = -ImageDisplayer.Image.rectTransform.anchoredPosition.y;
        float width = ImageDisplayer.Image.rectTransform.rect.width;
        float height = ImageDisplayer.Image.rectTransform.rect.height;

        instance.topLeft = new Vector2(x, y);
        instance.widthHeight = new Vector2(width, height);

        instance.lastCoordinates = null;
        instance._startCoordinates = null;
        instance._endCoordinates = null;
    }
    public static void ResetCoords()
    {
        instance._startCoordinates = null;
        instance._endCoordinates = null;
    }

    public void AddStartPoint()
    {
        if (ImageDisplayer.Map == null)
            return;

        brush = Map.Node.Start;
    }
    public void AddEndPoint()
    {
        if (ImageDisplayer.Map == null)
            return;

        brush = Map.Node.End;
    }
    public void AddFreeSpace()
    {
        if (ImageDisplayer.Map == null)
            return;

        brush = Map.Node.Free;
    }
    public void AddObstacle()
    {
        if (ImageDisplayer.Map == null)
            return;

        brush = Map.Node.Obstacle;
    }
    public void ClearBrush()
    {
        brush = null;
    }

    bool GetPixelCoordinates(out Vector2Int coordinates)
    {
        if (ImageDisplayer.Map == null)
        {
            coordinates = -Vector2Int.one;
            return false;
        }

        Vector2 point = Input.mousePosition;
        point.y = Screen.height - point.y;
        point -= topLeft;
        point /= widthHeight;

        if (point.x < 0 ||
            point.y < 0 ||
            point.x > 1 ||
            point.y > 1)
        {
            coordinates = -Vector2Int.one;
            return false;
        }

        point.x *= Map.Width;
        point.y *= Map.Height;
        point.y = Map.Height - point.y;

        coordinates = new Vector2Int((int)point.x, (int)point.y);
        return true;
    }
}
