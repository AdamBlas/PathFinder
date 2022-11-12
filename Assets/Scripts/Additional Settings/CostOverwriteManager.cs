using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CostOverwriteManager : MonoBehaviour
{
	[Tooltip("Toggle to get value from")]
	public Toggle toggle;
	
	[Tooltip("Flag indicating whether or not cost should be overwritten")]
	public static bool shouldOverwrite;
	
	
	
	
	
	
	// Awake is called when the script instance is being loaded.
	protected void Awake()
	{
		// Subscribe events
		toggle.onValueChanged.AddListener(UpdateOverwriterStatus);
		GoalBoundingManager.Instance.slider.onValueChanged.AddListener((value) => SetInteraction(value != 0));
		
		// Initialize state
		SetInteraction(GoalBoundingManager.strength != 0);
		
	}
	
	/// <summary>
	/// Updates flag's value to match toggle's status
	/// </summary>
	public void UpdateOverwriterStatus(bool value)
	{
		// Save value	
		shouldOverwrite = value;
	}
	
	/// <summary>
	/// Enables or disables interaction
	/// </summary>
	public void SetInteraction(bool shouldBeInteractable)
	{
		// Set interaction
		toggle.interactable = shouldBeInteractable;
		
		// If component is not interactable, cost overwrite should we disabled regardless of toggle's value
		if (shouldBeInteractable == false)
			shouldOverwrite = false;
		else
			shouldOverwrite = toggle.isOn;
	}
}
