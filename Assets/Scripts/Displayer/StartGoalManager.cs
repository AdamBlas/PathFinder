using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartGoalManager : MonoBehaviour
{
	[Tooltip("Singleton")]
	public static StartGoalManager Instance;
	
	// Start coords
	public static bool startExists = false;
	public static int startRow, startCol;
	
	// Goal coords
	public static bool goalExists = false;
	public static int goalRow, goalCol;
	
	
	
	
	[Tooltip("Label of the start button")]
	public TMPro.TMP_Text startButtonLabel;
	
	[Tooltip("Label of the goal button")]
	public TMPro.TMP_Text goalButtonLabel;
	
	[Tooltip("Shadow of the start button")]
	public LeTai.TrueShadow.TrueShadow startShadow;
	
	[Tooltip("Shadow of the goal button")]
	public LeTai.TrueShadow.TrueShadow goalShadow;
	
	[Tooltip("Map's hover layer")]
	public RectTransform hover;
	
	[Tooltip("Map mask")]
	public RectTransform mask;
	
	
	
	
	
	
	
	
	
	
	// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
	protected void Start()
	{
		// Create singleton
		Instance = this;
		
		// Set buttons' colors
		startShadow.Color = Displayer.Instance.startColor;
		goalShadow.Color = Displayer.Instance.goalColor;
	}
	
	/// <summary>
	/// Enables start brush
	/// </summary>
	public void SetStartBrush()
	{
		// If no map is loaded, do nothing
		if (Map.map == null)
			return;
		
		// Select start paint and start painting pixels that are hovered
		StartCoroutine(PaintSelected(Displayer.Instance.startColor, SetStartPoint));
	}
	
	/// <summary>
	/// Enables goal brush
	/// </summary>
	public void SetGoalBrush()
	{
		// If no map is loaded, do nothing
		if (Map.map == null)
			return;
		
		// Select goal paint and start painting pixels that are hovered
		StartCoroutine(PaintSelected(Displayer.Instance.goalColor, SetGoalPoint));
	}
	
	/// <summary>
	/// Paints hovered parts of the image
	/// </summary>
	/// <param name="color"> Color to paint </param>
	/// <param name="onClick"> Action to invoke when clicked </param>
	IEnumerator PaintSelected(Color color, System.Action<int, int> onClick)
	{
		Vector2Int? pixelCoords = null;
		
		// Loop as long as mouse button is not clicked
		while (Input.GetMouseButtonDown(0) == false)
		{
			// Get pixel coords
			pixelCoords = GetPixelCoordsBasedOnCursor();
			
			if (pixelCoords.HasValue)
			{
				// Cursor is inside boudaries, color pixel
				Displayer.Instance.SetHoverPixel(pixelCoords.Value.y, pixelCoords.Value.x, color);
			}
			else
			{
				// Cursor is outsie boundaries, clear hover layer
				Displayer.Instance.SetHoverPixel(0, 0, Color.clear);
			}

			// Go to the next frame
			yield return null;
		}
		// Exiting the loop means button was pressed
		
		// Clear hover layer
		Displayer.Instance.SetHoverPixel(0, 0, Color.clear);
		
		// Place color pixel on the overlay layer
		// No value means that clicked outside of displayer, in that case, do everything but set point
		if (pixelCoords.HasValue)
			onClick.Invoke(pixelCoords.Value.x, pixelCoords.Value.y);
	}
	
	/// <summary>
	/// Saves start node values
	/// </summary>
	/// <param name="row"> Start row </param>
	/// <param name="column"> Start column </param>
	public void SetStartPoint(int row, int column)
	{
		// Clear previous values
		Displayer.Instance.RemoveStartCoords();
		
		// Save values
		startExists = true;
		startRow = row;
		startCol = column;
		
		// Print that values to GUI
		startButtonLabel.text = "Start: Column=" + column + ", Row=" + row;
		
		Displayer.Instance.PrintStartCoords();
	}
	
	/// <summary>
	/// Saves goal node values
	/// </summary>
	/// <param name="row"> Start row </param>
	/// <param name="column"> Start column </param>
	void SetGoalPoint(int row, int column)
	{
		// Clear previous values
		Displayer.Instance.RemoveGoalCoords();
		
		// Save values
		goalExists = true;
		goalRow = row;
		goalCol = column;
		
		// Print that values to GUI
		goalButtonLabel.text = "Goal: Column=" + column + ", Row=" + row;
		
		Displayer.Instance.PrintGoalCoords();
	}
	
	/// <summary>
	/// Calculates coordinates of the pixel that cursor is hovering
	/// </summary>
	/// <returns> Coords of the pixel cursor is hovering </returns>
	public Vector2Int? GetPixelCoordsBasedOnCursor()
	{
		// Get map's corners
		Vector3[] corners = new Vector3[4];
		hover.GetWorldCorners(corners);
		
		// Get cursor world position
		Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		// Get displayer's dimensions
		float width = corners[2].x - corners[0].x;
		float height = corners[2].y - corners[0].y;

		// Get cursor coords in pixels values
		float x = (cursorWorldPosition.x - corners[0].x) * Map.width / width;
		float y = (corners[2].y - cursorWorldPosition.y) * Map.height / height;
		
		// If cusror is outside map area, return null
		if (x < 0 || x >= Map.width || y < 0 || y >= Map.height)
			return null;
		
		// Round values to ints and return result
		Vector2Int result = new Vector2Int(Mathf.FloorToInt(y), Mathf.FloorToInt(x));
		return result;
	}
	
	/// <summary>
	/// Clear start and goal nodes' data
	/// </summary>
	public void ClearStartGoalCoords()
	{
		// Set data
		startRow = 0;
		startCol = 0;
		goalRow = 0;
		goalCol = 0;
		startExists = false;
		goalExists = false;
		
		// Update GUI
		startButtonLabel.text = "Start: None";
		goalButtonLabel.text = "Goal: None";
		
		// Clear displayer
		Displayer.Instance.RemoveStartCoords();
		Displayer.Instance.RemoveGoalCoords();
	}
	
	public static string StartToString()
	{
		return "(" + startCol + "x" + startRow + ")";
	}
	
	public static string GoalToString()
	{
		return "(" + goalCol + "x" + goalRow + ")";
	}
}
