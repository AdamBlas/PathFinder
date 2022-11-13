using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes this object to follow mouse cursor
/// </summary>
public class MouseFollower : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
	    // Get mouse coordinates relative to world
	    Vector3 mouseWorldCoords = Camera.main.ScreenToWorldPoint(Input.mousePosition);
	    
	    // Move coords away from camera
	    mouseWorldCoords.z = -9;
	    
	    // Update this object's position
	    this.transform.position = mouseWorldCoords;
    }
}
