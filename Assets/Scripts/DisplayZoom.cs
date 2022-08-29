using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DisplayZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{   
    public Transform display;
    public float zoomStrength;
    public float movementStrength;

    bool isMouseInside;
    bool isMoving;
    Vector3 initialPosition;
    Vector3 initialScale;

    RectTransform rectTransform;
    float maxTop;
    float maxBottom;
    float maxRight;
    float maxLeft;



    void Awake()
    {
        initialPosition = display.position;
        initialScale = display.localScale;

        rectTransform = display.GetComponent<RectTransform>();

        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        maxTop = GetTop(corners);
        maxBottom = GetBottom(corners);
        maxRight = GetRight(corners);
        maxLeft = GetLeft(corners);
    }

    void Update()
    {
        ManageZoom();
        ManageOffset();
    }

    void ManageZoom()
    {
        if (isMouseInside)
        {
            float scrollValue = Input.mouseScrollDelta.y * zoomStrength;
            
            if (scrollValue == 0)
                return;
            
            display.localScale -= Vector3.one * scrollValue;
            if (display.localScale.x < initialScale.x)
                display.localScale = initialScale;

            CheckBoundaries();
        }
    }

    void ManageOffset()
    {
        if (isMouseInside && Input.GetMouseButtonDown(0) && !isMoving)
        {
            StartCoroutine(Move());
        }
    }
    IEnumerator Move()
    {
        isMoving = true;

        Vector3 initialMousePosition = Input.mousePosition;

        while (Input.GetMouseButton(0))
        {
            display.position = initialPosition
                + (Input.mousePosition - initialMousePosition)
                * movementStrength
                * Mathf.Sqrt(display.localScale.x);

            CheckBoundaries();
            yield return null;
        }
        initialPosition = display.position;

        isMoving = false;
    }

    void CheckBoundaries()
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        float top = GetTop(corners);
        float bottom = GetBottom(corners);
        float left = GetLeft(corners);
        float right = GetRight(corners);

        Vector3 offset = Vector3.zero;

        if (top < maxTop)
            offset.y -= top - maxTop;

        if (bottom > maxBottom)
            offset.y -= bottom - maxBottom;

        if (left > maxLeft)
            offset.x -= left - maxLeft;

        if (right < maxRight)
            offset.x -= right - maxRight;

        rectTransform.position += offset;
    }

    float GetTop(Vector3[] corners)
    {
        return Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
    }
    float GetBottom(Vector3[] corners)
    {
        return Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
    }
    float GetRight(Vector3[] corners)
    {
        return Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
    }
    float GetLeft(Vector3[] corners)
    {
        return Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
    }





    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseInside = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseInside = false;
    }
}
