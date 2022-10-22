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
	
	[Tooltip("Map displayer layer")]
	public Image mapDisplayer;
	
	[Tooltip("Map overlay layer")]
	public Image mapOverlay;
	
	[Tooltip("Map hover layer")]
	public Image mapHover;
	
	// Colors
	public Color freeColor = new Color(1f, 1f, 1f);
	public Color obstacleColor = new Color(.25f, .25f, .25f);
	public Color startColor = new Color(1f, .6f, 0);
	public Color goalColor = new Color(1f, .2f, 0);
	public Color pathColor = new Color(.5f, 0, 1);
	public Color subpathColor1 = new Color(.75f, .5f, 1);
	public Color subpathColor2 = new Color(.75f, .65f, 1);
	public Color toAnalyzeColor = new Color(.6f, 1f, .6f);
	public Color analyzedColor = new Color(.15f, 1f, .15f);

	[Tooltip("Texture of the map displayer layer")]
	Texture2D mapTexture;
	
	[Tooltip("Texture of the map overlay layer")]
	Texture2D overlayTexture;
	
	[Tooltip("Texture of the map hover layer")]
	Texture2D hoverTexture;
	
	[Tooltip("Flag indicating whether or not mouse cursor is inside mask")]
	public static bool isMouseInside;





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
		isMouseInside = IsMouseInsideMask();
	}

	/// <summary>
	/// Create texture based on load map
	/// </summary>
	public void RefreshMap()
	{
		// Create textures
		mapTexture = new Texture2D(Map.width, Map.height);
		overlayTexture = new Texture2D(Map.width, Map.height);
		mapTexture.filterMode = FilterMode.Point;	
		overlayTexture.filterMode = FilterMode.Point;
		ClearTexture(mapTexture);
		ClearTexture(overlayTexture);
		
		// Pain every pixel on the map
		for (int row = 0; row < Map.height; row++)
			for (int col = 0; col < Map.width; col++)
				mapTexture.SetPixel(col, row, Map.map[row, col] == NodeType.FREE ? freeColor : obstacleColor);
	
		// If start or end nodes are set, print them
		PrintStartCoords();
		PrintGoalCoords();
		
		// Apply changes
		mapTexture.Apply();
		overlayTexture.Apply();
		
		// Save textures to images
		UpdateDisplayLayer();
		UpdateOverlayLayer();
	}
	
	/// <summary>
	/// Removes start pixel from the overlay
	/// </summary>
	public void RemoveStartCoords()
	{
		if (StartGoalManager.startExists)
			overlayTexture.SetPixel(StartGoalManager.startCol, StartGoalManager.startRow, new Color(0, 0, 0, 0));
		
		UpdateOverlayLayer();
	}
	
	/// <summary>
	/// Removes goal pixel from the overlay
	/// </summary>
	public void RemoveGoalCoords()
	{
		if (StartGoalManager.goalExists)
			overlayTexture.SetPixel(StartGoalManager.goalCol, StartGoalManager.goalRow, new Color(0, 0, 0, 0));
	
		UpdateOverlayLayer();
	}
	
	/// <summary>
	/// Prints start coord on the overlay
	/// </summary>
	public void PrintStartCoords()
	{
		if (StartGoalManager.startExists)
			overlayTexture.SetPixel(StartGoalManager.startCol, StartGoalManager.startRow, startColor);
		
		UpdateOverlayLayer();
	}
	
	/// <summary>
	/// Prints goal coord on the overlay
	/// </summary>
	public void PrintGoalCoords()
	{
		if (StartGoalManager.goalExists)
			overlayTexture.SetPixel(StartGoalManager.goalCol, StartGoalManager.goalRow, goalColor);
	
		UpdateOverlayLayer();
	}
	
	/// <summary>
	/// Clears hover layer and sets new pixel
	/// </summary>
	/// <param name="row"></param>
	/// <param name="column"></param>
	/// <param name="color"></param>
	public void SetHoverPixel(int row, int column, Color color)
	{
		// Create new texture
		hoverTexture = new Texture2D(Map.width, Map.height);
		hoverTexture.filterMode = FilterMode.Point;
		ClearTexture(hoverTexture);
		
		// Sets pixel color
		hoverTexture.SetPixel(row, column, color);
		
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
	/// Updates overlay image to display overlay texture
	/// </summary>
	public void UpdateOverlayLayer()
	{
		UpdateLayer(overlayTexture, mapOverlay);
	}
	
	/// <summary>
	/// Updates hover image to display hover texture
	/// </summary>
	public void UpdateHoverLayer()
	{
		UpdateLayer(hoverTexture, mapHover);
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
