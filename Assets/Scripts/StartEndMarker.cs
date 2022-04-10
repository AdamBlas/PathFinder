using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Allows to put start and end markers on map
/// </summary>
public class StartEndMarker : MonoBehaviour
{
    public Image startMarker;
    public Image endMarker;
    public float scaleWithMapSize;
    public float minMapSize;

    static StartEndMarker instance;

    void Start()
    {
        instance = this;

        startMarker.color = ImageDisplayer.Instance.startColor;
        endMarker.color = ImageDisplayer.Instance.endColor;

        DisableStartMarker();
        DisableEndMarker();
    }

    public static void SetStartMarker(int x, int y)
    {
        instance.startMarker.transform.position = GetPosition();
        SetStartMarkerProperties(x, y);
        PathFinder.UpdateButtonState();
    }
    public static void SetStartMarkerProperties(int x, int y)
    {
        instance.startMarker.enabled = true;
        instance.startMarker.transform.localScale = GetScale();
    }
    public static void DisableStartMarker()
    {
        instance.startMarker.enabled = false;
    }

    public static void SetEndMarker(int x, int y)
    {
        instance.endMarker.transform.position = GetPosition();
        SetEndMarkerProperties(x, y);
        PathFinder.UpdateButtonState();
    }
    public static void SetEndMarkerProperties(int x, int y)
    {
        instance.endMarker.enabled = true;
        instance.endMarker.transform.localScale = GetScale();
    }
    public static void DisableEndMarker()
    {
        instance.endMarker.enabled = false;
    }

    public static Vector3 GetPosition()
    {
        Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        position.z = 0;
        return position;
    }
    static Vector3 GetScale()
    {
        float mapSize = Mathf.Max(Map.Width, Map.Height);

        if (mapSize < instance.minMapSize)
            return Vector3.zero;

        float scale = instance.scaleWithMapSize * 100 / mapSize;
        return new Vector3(scale, scale, scale);
    }
}
