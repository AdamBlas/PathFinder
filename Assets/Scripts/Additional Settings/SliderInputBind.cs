using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Binds slider and input field to show the same value
/// </summary>
public class SliderInputBind : MonoBehaviour
{
	[Tooltip("Slider to bind")]
	public Slider slider;
	
	[Tooltip("InputField to bind")]
	public TMPro.TMP_InputField input;
	
	[Tooltip("Default value, set when input is enpty")]
	public float defaultValue;
	
	
	
	
	
	
	
	// Awake is called when the script instance is being loaded.
	protected void Awake()
	{
		// Subscribe events
		slider.onValueChanged.AddListener(SliderToInput);
		input.onValueChanged.AddListener(InputToSlider);
		
		// Assign default value
		slider.value = defaultValue;
	}
	
	/// <summary>
	/// Assigns input value to slider
	/// </summary>
	void InputToSlider(string inputValue)
	{
		// If input change was caused by slider event, do nothing
		// If it was, inputField is not focused
		if (input.isFocused == false)
			return;
		
		// If input is empty, assign default value to slider
		if (string.IsNullOrEmpty(inputValue))
		{
			slider.value = defaultValue;
			return;
		}
			
		// If input can be parsed, assign that value to slider
		if (float.TryParse(inputValue, out float value))
		{
			slider.value = value;
			return;
		}
		
		// Input can't be parsed, do nothing
	}
	
	/// <summary>
	/// Assigns slider value to input
	/// </summary>
	void SliderToInput(float sliderValue)
	{
		// If slider change was caused by inputField event, do nothing
		// If it was, inputField is focused
		if (input.isFocused)
			return;
		
		// Assign slider's value to input
		// If values are floats, round to 2 significant digits
		if (slider.wholeNumbers)
			input.text = sliderValue.ToString();
		else
			input.text = sliderValue.ToString("f2");
	}
}
