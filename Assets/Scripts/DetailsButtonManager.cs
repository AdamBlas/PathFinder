using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetailsButtonManager : MonoBehaviour
{
	[Tooltip("Singleton")]
	static DetailsButtonManager Instance;
	
	[Tooltip("Button component")]
	Button button;
	
	[Tooltip("Button component accessible from outside")]
	public static Button ButtonComponent => Instance.button;
	
	
	
	
	// Awake is called when the script instance is being loaded.
	protected void Awake()
	{
		// Create singleton
		Instance = this;
		
		// Get component
		button = GetComponent<Button>();
	}
	
	/// <summary>
	/// Enables button
	/// </summary>
	public static void EnableButton()
	{
		Instance.gameObject.SetActive(true);
	}
	
	/// <summary>
	/// Disables button
	/// </summary>
	public static void DisableButton()
	{
		Instance.gameObject.SetActive(false);
	}
}
