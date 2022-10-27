using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoalBoundingManager : MonoBehaviour
{
	[Tooltip("Singleton")]
	static GoalBoundingManager Instance;
	
	[Tooltip("Slider to get value from")]
	public Slider slider;
	
	[Tooltip("Value accessor")]
	public static float strength;
	
	[Tooltip("Flag indicating whether or not goal bounding should be applied")]
	public static bool shouldApply;
	
	
	
	
	
	
	// Awake is called when the script instance is being loaded.
	protected void Awake()
	{
		// Create singleton
		Instance = this;
		
		// Subscribe event
		slider.onValueChanged.AddListener(UpdateStrengthValue);
	}
	
	/// <summary>
	/// Updates strength's value to match slider's value
	/// </summary>
	public void UpdateStrengthValue(float value)
	{
		// Save value
		strength = value;
		
		// Check if value qualifies for goal bounding
		shouldApply = strength != 0;
		
		// Update heuristics' lookup table
		for (int i = 0; i < 181; i++)
			Heuristic.goalBoundingSines[i] = Heuristic.sines[i] * strength;
	}
}
