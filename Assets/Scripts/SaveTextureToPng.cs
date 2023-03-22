using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveTextureToPng : MonoBehaviour
{
	[Tooltip("Singleton")]
	public static SaveTextureToPng Instance;
	
	[Tooltip("Textures that have to be flattened")]
	public Image[] images;
	
	[Tooltip("List of flags if images should be inversed")]
	public bool[] inversed;
	
	
	
	
	// Start is called on the frame when a script is enabled just before any of the Update methods is called the first time.
	protected void Start()
	{
		Instance = this;
	}
	
	public static void StaticSaveToPng(string path) => Instance.SaveToPng(path);
	
	public void SaveToPng() => SaveToPng("Images/Map_" + System.DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + ".png");
	
	public void SaveToPng(string path)
	{
		// First, prepare texture
		Texture2D tex = new Texture2D(Map.width, Map.height);
		
		// Flatten textures
		for (int i = 0; i < images.Length; i++)
		{
			// Iterate through image
			for (int x = 0; x < Map.width; x++)
			{
				for (int y = 0; y < Map.height; y++)
				{
					// Get source pixel
					Color pixel = inversed[i] ? images[i].sprite.texture.GetPixel(x, Map.height - 1 - y) : images[i].sprite.texture.GetPixel(x, y);
					
					// If that pixel is not transparent, save data
					if (pixel.a != 0)
						tex.SetPixel(x, y, pixel);
				}
			}
		}
		
		// Save texture to file
		byte[] bytes = tex.EncodeToPNG();
		System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
		System.IO.File.WriteAllBytes(path, bytes);
		
		Debug.Log("Saved map image into " + path);
	}
}
