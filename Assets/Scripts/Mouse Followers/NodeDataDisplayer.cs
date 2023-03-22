using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeDataDisplayer : MonoBehaviour
{
	[Tooltip("Label responsible for visuals")]
	public GameObject label;
	
	[Tooltip("Data texts")]
	public TMPro.TMP_Text[] dataTexts;
	
	
	
	
	
	
    // Update is called once per frame
    void Update()
	{
		// Get pixel coords
		Vector2Int? pixels = StartGoalManager.Instance.GetPixelCoordsBasedOnCursor();
		
		// Show coords displayer only if cursor is inside map displayer
		label.SetActive(pixels.HasValue);

		// Don't proceed if pixel value was not set
		if (pixels.HasValue == false)
			return;
		
		// Set coords
		dataTexts[0].text = pixels.Value.y + " × " + pixels.Value.x;
			
		// Add algorith's data to displayer
		AddAlgorithmData(pixels.Value);
	}
    
	/// <summary>
	/// Gets node's data depending on selected algorithm
	/// </summary>
	public void AddAlgorithmData(Vector2Int pixels)
	{
		// Get current algorithm
		Algorithm algorithm = AlgorithmSelector.GetAlgorithm();

		try
		{
			// Invoke proper method depending on algorithm's type
			if (algorithm.GetType().Equals(typeof(AStar)))
				AddAStarData(pixels);
			else if (algorithm.GetType().Equals(typeof(HPAStar)))
				AddHPAStarData(pixels);
			else if (algorithm.GetType().Equals(typeof(JPS)))
				AddJPSData(pixels);
			else
				Debug.LogWarning("Unknown algorithm type: " + algorithm.GetType().ToString());
		}
		catch (System.Exception)
		{
			// If any error occurs, do nothing	
		}
	}
	
	/// <summary>
	/// Gets node's data for A* algorithm
	/// </summary>
	public void AddAStarData(Vector2Int pixels)
	{
		// Disable thirs label as it will not be used
		dataTexts[2].gameObject.SetActive(false);
		
		// Get array of visited nodes
		Array2D<Node> array = AStar.Instance.nodesVisited;
		
		// Check if array was initialized and if node was created
		if (array == null || array[pixels.y, pixels.x] == null)
		{
			dataTexts[1].gameObject.SetActive(false);
			return;
		}
		
		// Node was initialized
		dataTexts[1].gameObject.SetActive(true);
		dataTexts[1].text = array[pixels.y, pixels.x].goalBoundCost.ToString("f2");
		
		// If mouse button is pressed, print node's detailed data
		if (Input.GetMouseButtonDown(0))
			Debug.Log("Detailed data about node: " + array[pixels.y, pixels.x]);
	}
	
	/// <summary>
	/// Gets node's data for HPA* algorithm
	/// </summary>
	public void AddHPAStarData(Vector2Int pixels)
	{
		// Convert pixel coords to chunk coords
		pixels /= ChunkSizeManager.ChunkSize;
		
		// Get chunks array
		Array2D<HPAStar.Chunk> chunkArray = HPAStar.Instance.visitedChunks;
		
		// Check if chunk array was initialized and if chunk was created
		if (chunkArray == null || chunkArray[pixels.y, pixels.x] == null)
		{
			dataTexts[1].gameObject.SetActive(false);
			dataTexts[2].gameObject.SetActive(false);
			return;
		}
		
		// Add chunk's cost to the text
		dataTexts[1].gameObject.SetActive(true);
		dataTexts[1].text = chunkArray[pixels.y, pixels.x].goalBoundCost.ToString("f2");
		
		// If mouse button is pressed, print detailed chunk info
		if (Input.GetMouseButtonDown(0))
			Debug.Log("Detailed data about chunk: " + chunkArray[pixels.y, pixels.x]);
	}
	
	/// <summary>
	/// Gets node's data for JPS algorithm
	/// </summary>
	public void AddJPSData(Vector2Int pixels)
	{
		
	}
}
