using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class represents map disaplyer
/// </summary>
public class Displayer : MonoBehaviour
{
	[Tooltip("Singleton")]
	public static Displayer Instance;
	
	[Tooltip("Map's mask")]
	public RectTransform mask;
	
	[Tooltip("Map layer")]
	public Image mapDisplayer;
	
	[Tooltip("Path layer")]
	public Image pathDisplayer;
	
	[Tooltip("Start and Goal Layer")]
	public Image startGoalDisplayer;
	
	[Tooltip("Hover layer")]
	public Image hoverDisplayer;
	
	// Colors
	public Color freeColor = new Color(1f, 1f, 1f);
	public Color obstacleColor = new Color(.25f, .25f, .25f);
	public Color startColor = new Color(1f, .6f, 0f);
	public Color goalColor = new Color(1f, .2f, 0f);
	public Color pathColor = new Color(.5f, 0f, 1);
	public Color subpathColor1 = new Color(.75f, .5f, 1);
	public Color subpathColor2 = new Color(.75f, .65f, 1);
	public Color toAnalyzeColor = new Color(.8f, 1f, .6f);
	public Color analyzedColor = new Color(0f, 0.75f, 0f);

	[Tooltip("Texture of the map layer")]
	Texture2D mapTexture;
	
	[Tooltip("Texture of the path layer")]
	Texture2D pathTexture;
	
	[Tooltip("Texture of the start/goal layer")]
	Texture2D startGoalTexture;
	
	[Tooltip("Texture of the map hover layer")]
	Texture2D hoverTexture;
	
	[Tooltip("Flag indicating whether or not mouse cursor is inside mask")]
	public static bool isMouseInside;
	
	[Tooltip("Flag indicating whether or not path layer should be updated")]
	public static bool shouldUpdatePathLayer;







	// Awake is called when the script instance is being loaded.
	protected void Awake()
	{
		// Create singleton
		if (Instance == null) Instance = this;
		else throw new System.Exception("Attempt to create second instance of singleton");
	}

	// Update is called every frame, if the MonoBehaviour is enabled.
	protected void Update()
	{
		// Check if mouse is inside displayer mask
		isMouseInside = IsMouseInsideMask();
		
		// Check if overlay layer should be updated
		if (shouldUpdatePathLayer)
		{
			// Reset flag
			shouldUpdatePathLayer = false;
			
			// Update layer
			UpdatePathLayer();
		}
	}

	/// <summary>
	/// Create texture based on load map
	/// </summary>
	public void RefreshMap()
	{
		// Create textures
		mapTexture = new Texture2D(Map.width, Map.height);
		pathTexture = new Texture2D(Map.width, Map.height);
		startGoalTexture = new Texture2D(Map.width, Map.height);
		
		// Set filter mode so that textures are pixel-perfect and without blur
		mapTexture.filterMode = FilterMode.Point;	
		pathTexture.filterMode = FilterMode.Point;
		startGoalTexture.filterMode = FilterMode.Point;
		
		// Fill textures with transparent pixels
		// No need to do that for map texture as it will be fully filled in the next step
		ClearTexture(pathTexture);
		ClearTexture(startGoalTexture);
		
		// Pain every pixel on the map
		for (int row = 0; row < Map.height; row++)
			for (int col = 0; col < Map.width; col++)
				mapTexture.SetPixel(col, Map.height - 1 - row, Map.map[row, col] == NodeType.FREE ? freeColor : obstacleColor);

		// Apply changes
		mapTexture.Apply();
		pathTexture.Apply();
		startGoalTexture.Apply();
		
		// Save textures to images
		UpdateDisplayLayer();
		UpdatePathLayer();
		UpdateStartGoalLayer();
		
		// Remove start and goal nodes (in case there are already set)
		StartGoalManager.Instance.ClearStartGoalCoords();
	}
	
	/// <summary>
	/// Removes start pixel from the overlay
	/// </summary>
	public void RemoveStartCoords()
	{
		if (StartGoalManager.startExists)
			startGoalTexture.SetPixel(StartGoalManager.startCol, StartGoalManager.startRow, new Color(0, 0, 0, 0));
		
		UpdateStartGoalLayer();
	}
	
	/// <summary>
	/// Removes goal pixel from the overlay
	/// </summary>
	public void RemoveGoalCoords()
	{
		if (StartGoalManager.goalExists)
			startGoalTexture.SetPixel(StartGoalManager.goalCol, StartGoalManager.goalRow, new Color(0, 0, 0, 0));
	
		UpdateStartGoalLayer();
	}
	
	/// <summary>
	/// Prints start coord on the overlay
	/// </summary>
	public void PrintStartCoords()
	{
		if (StartGoalManager.startExists)
			startGoalTexture.SetPixel(StartGoalManager.startCol, StartGoalManager.startRow, startColor);
		
		UpdateStartGoalLayer();
	}
	
	/// <summary>
	/// Prints goal coord on the overlay
	/// </summary>
	public void PrintGoalCoords()
	{
		if (StartGoalManager.goalExists)
			startGoalTexture.SetPixel(StartGoalManager.goalCol, StartGoalManager.goalRow, goalColor);
	
		UpdateStartGoalLayer();
	}
	
	/// <summary>
	/// Paints new pixel on path layer
	/// </summary>
	/// <param name="row"> X coordinate to paint </param>
	/// <param name="column"> Y coordinate to paint </param>
	/// <param name="color"> Color to paint </param>
	public void PaintPath(int row, int column, Color color)
	{
		// Set pixel's color
		pathTexture.SetPixel(row, column, color);
		
		// Set flag so that texture will be updated next frame
		shouldUpdatePathLayer = true;
	}
	
	/// <summary>
	/// Clears hover layer and sets new pixel
	/// </summary>
	/// <param name="row"> X coordinate to paint </param>
	/// <param name="column"> Y coordinate to paint </param>
	/// <param name="color"> Color to paint </param>
	public void SetHoverPixel(int row, int column, Color color)
	{
		// Create new texture
		hoverTexture = new Texture2D(Map.width, Map.height);
		hoverTexture.filterMode = FilterMode.Point;
		ClearTexture(hoverTexture);
		
		// Sets pixel color
		hoverTexture.SetPixel(row, -column - 1, color);
		
		// Apply changes and set texure to displayer
		UpdateHoverLayer();
	}
	
	/// <summary>
	/// Updates map image to display map texture
	/// </summary>
	public void UpdateDisplayLayer()
	{
		UpdateLayer(mapTexture, mapDisplayer);
	}
	
	/// <summary>
	/// Updates path image to display path texture
	/// </summary>
	public void UpdatePathLayer()
	{
		UpdateLayer(pathTexture, pathDisplayer);
	}
	
	/// <summary>
	/// Updates start/goal image to display start/goal texture
	/// </summary>
	public void UpdateStartGoalLayer()
	{
		UpdateLayer(startGoalTexture, startGoalDisplayer);
	}
	
	/// <summary>
	/// Updates hover image to display hover texture
	/// </summary>
	public void UpdateHoverLayer()
	{
		UpdateLayer(hoverTexture, hoverDisplayer);
	}
	
	/// <summary>
	/// Updates image to display texture
	/// </summary>
	/// <param name="tex"> Texture to display </param>
	/// <param name="renderer"> Image to display texture </param>
	public void UpdateLayer(Texture2D tex, Image renderer)
	{
		// Apply changes to texture
		tex.Apply();
		
		// Create sprite
		Rect rect = new Rect(0, 0, Map.width, Map.height);
		Sprite sprite = Sprite.Create(tex, rect, Vector2.zero);
		
		// Set sprite as image's source
		renderer.sprite = sprite;
	}
	
	/// <summary>
	/// Checks if mouse cursor is inside visible part of map
	/// </summary>
	/// <returns> True if mouse is inside, False otherwise </returns>
	static bool IsMouseInsideMask()
	{
		// Get corners of mask
		Vector3[] corners = new Vector3[4];
		Instance.mask.GetWorldCorners(corners);
		
		// Get mouse position in world coords
		Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		
		// Check if mouse is inside rectangle
		bool isMouseInside = 
			mousePos.x >= corners[0].x &&
			mousePos.x <= corners[2].x &&
			mousePos.y >= corners[0].y &&
			mousePos.y <= corners[2].y;
			
		// Return result
		return isMouseInside;
	}
	
	/// <summary>
	/// Clears all pixels in path layer
	/// </summary>
	public void ClearPathLayer()
	{
		// Clear texture
		ClearTexture(pathTexture);
		
		// Apply changes
		UpdatePathLayer();
	}
	
	public void ClearStartGoalLayer()
	{
		// Clear texture
		ClearTexture(startGoalTexture);
		
		// Apply changes
		UpdateStartGoalLayer();
	}
	
	/// <summary>
	/// Fills texture with zero alpha pixels
	/// </summary>
	/// <param name="tex"> Texture to clear </param>
	void ClearTexture(Texture2D tex)
	{
		for (int i = 0; i < tex.width; i++)
			for (int j = 0; j < tex.height; j++)
				tex.SetPixel(i, j, Color.clear);
	}
}
