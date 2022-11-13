using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordsDisplayer : MonoBehaviour
{
	[Tooltip("Objects responsible for GFX")]
	public GameObject[] gfx;
	
	[Tooltip("Coords' text")]
	public TMPro.TMP_Text text;
	
	
	
	
	
	
    // Update is called once per frame
    void Update()
	{
		// Get pixel coords
		Vector2Int? pixels = StartGoalManager.Instance.GetPixelCoordsBasedOnCursor();
		
		// Show coords displayer only if cursor is inside map displayer
		foreach (GameObject obj in gfx)
			obj.SetActive(pixels.HasValue);

		// Set text if cursor is inside map displayer
		if (pixels.HasValue)
			text.text = pixels.Value.y + " × " + pixels.Value.x;
    }
}
