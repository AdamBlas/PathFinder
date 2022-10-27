using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChunkSizeManager : MonoBehaviour
{
	[Tooltip("Singleton")]
	static ChunkSizeManager Instance;
	
	[Tooltip("Slider to get value from")]
	public Slider slider;
	
	[Tooltip("Size of the chunk")]
	public static int ChunkSize;
	
	
	
	
	
	
	// Awake is called when the script instance is being loaded.
	protected void Awake()
	{
		// Create singleton
		Instance = this;
	
		// Subscribe event
		slider.onValueChanged.AddListener(UpdateSizeValue);
		
		// Initialize value
		ChunkSize = (int)slider.value;
	}
	
	/// <summary>
	/// Invoked when slider value changes. Saves value to size field
	/// </summary>
	void UpdateSizeValue(float value)
	{
		// Save value
		ChunkSize = (int)value;
	}
}
