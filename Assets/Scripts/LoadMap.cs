using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NodeType;
using SFB;

/// <summary>
/// Class loads map txt file and creates map
/// </summary>
public class LoadMap : MonoBehaviour
{
	[Tooltip("Input field that contains characters that are considered as free nodes")]
	public TMPro.TMP_InputField freeChars;
	
	[Tooltip("Text field that copntains width and height values")]
	public TMPro.TMP_Text mapDimensionsValues;
	
	[Tooltip("Name of the file")]
	public static string mapName;
	
	
	
	
	
	
	public void OpenMapFile()
	{
		// Prepare extensions
		var extensions = new[] { new ExtensionFilter("Map files", "map") };
		
		// Get selected file (multiselect = false will ensire that length will be wither one or zero)
		string[] paths = StandaloneFileBrowser.OpenFilePanel("Open map", "", extensions, false);
	
		// If no file was selected, end early
		if (paths.Length == 0)
			return;
			
		string path = paths[0];
		
		// Save path for future
		mapName = System.IO.Path.GetFileNameWithoutExtension(path);
		
		// Get file content
		string[] mapText = File.ReadAllLines(path);
		
		// Get map dimensions from header
		ushort height = ushort.Parse(mapText[1].Split(' ')[1]);
		ushort width = ushort.Parse(mapText[2].Split(' ')[1]);
		
		// Create map
		Map.CreateMap(width, height);
		
		// Get chars with free strings
		string freeChars = this.freeChars.text;
		
		// Iterate through all content and set nodes' values
		// If free characters list contains character, set node as free, otherwise, obstacle
		for (int row = 0; row < height; row++)
			for (int col = 0; col < width; col++)
				Map.map[row, col] = freeChars.IndexOf(mapText[4 + row][col]) >= 0 ? FREE : OBSTACLE;

		// Trim map
		Map.Trim();
		
		// Print dimensions on GUI
		mapDimensionsValues.text = Map.width + "\n" + Map.height;
		
		// Display that map
		Displayer.Instance.RefreshMap();
	}
}
