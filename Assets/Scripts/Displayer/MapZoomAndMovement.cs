using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class manages map position and scale
/// </summary>
public class MapZoomAndMovement : MonoBehaviour
{
	[Tooltip("Maps containet that will be transformed")]
	public RectTransform mapsScaler;
	
	[Tooltip("Map Mask that will be used to determine movement")]
	public RectTransform mask;
	
	// Border values of the map. While moving, new values can't exceed these ones.
	float minX, maxX, minY, maxY;
	
	
	
	
	
	
    // Start is called before the first frame update
    void Start()
	{
		// Get four corners
    	Vector3[] corners = new Vector3[4];
    	mapsScaler.GetWorldCorners(corners);
    	
		// Get border values
		// 0 - bottom left
		// 2 - top right
		minX = corners[0].x;
		minY = corners[0].y;
		maxX = corners[2].x;
		maxY = corners[2].y;
    }

    // Update is called once per frame
    void Update()
	{
		if (Displayer.isMouseInside)
		{
	    	ManageZoom();
			ManagePosition();
		}
    }
    
	/// <summary>
	/// Chamges map's zoom
	/// </summary>
	void ManageZoom()
	{
		// Scroll up = zoom in
		if (Input.mouseScrollDelta.y > 0)
		{
			// Increase zoom
			mapsScaler.transform.localScale *= 2;
			
			// Secure value
			if (mapsScaler.transform.localScale.x > 16)
				mapsScaler.transform.localScale = new Vector3(16, 16, 16);
		}
		else if (Input.mouseScrollDelta.y < 0)
		{
			// Decrease zoom
			mapsScaler.transform.localScale *= 0.5f;
			
			// Secure value
			if (mapsScaler.transform.localScale.x < 1)
				mapsScaler.transform.localScale = Vector3.one;
				
			SecureImage();
		}
	}
	
	/// <summary>
	/// Changes map's position
	/// </summary>
	void ManagePosition()
	{
		// Start movement loop if mouse button is pressed and cursor is inside map
		if (Input.GetMouseButtonDown(0))
			StartCoroutine(MoveImage());
	}
	
	/// <summary>
	/// Moves image as long as mouse button is pressed
	/// </summary>
	IEnumerator MoveImage()
	{
		// Get mouse position in previous frame
		Vector3 previousMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		
		// Loop as long as mouse button is pressed
		while (Input.GetMouseButton(0))
		{
			// Get cursor's world position
			Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			
			// Get mouse delta
			Vector3 mouseDelta = mouseWorldPosition - previousMousePosition;
			
			// Save mouse position as previous position
			previousMousePosition = mouseWorldPosition;
			
			// Move image depending on input
			mapsScaler.transform.position += mouseDelta;
			
			// Secure map so it can't go out of borders
			SecureImage();
			
			// Go to the next frame
			yield return null;
		}
	}
	
	/// <summary>
	/// Makes sure that image does not exceed borders
	/// </summary>
	void SecureImage()
	{
		// Get corners
		Vector3[] corners = new Vector3[4];
		mapsScaler.GetWorldCorners(corners);
			
		// Prepare offset variable
		Vector3 offset = Vector3.zero;
			
		// Modify offset based on map's position
		if (corners[0].x > minX) offset.x -= corners[0].x - minX;
		if (corners[0].y > minY) offset.y -= corners[0].y - minY;
		if (corners[2].x < maxX) offset.x -= corners[2].x - maxX;
		if (corners[2].y < maxY) offset.y -= corners[2].y - maxY;
			
		// Apply offset
		mapsScaler.transform.position += offset;
	}
}
