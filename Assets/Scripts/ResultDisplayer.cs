using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultDisplayer : MonoBehaviour
{
	[Tooltip("Singleton")]
	static ResultDisplayer Instance;
	
	[Tooltip("1st column for results display")]
	public TMPro.TMP_Text column1;
	
	[Tooltip("2nd column for results display")]
	public TMPro.TMP_Text column2;
	
	[Tooltip("3rd column for results display")]
	public TMPro.TMP_Text column3;
	
	
	
	
	
	
	// Awake is called when the script instance is being loaded.
	protected void Awake()
	{
		// Create singleton
		if (Instance == null) Instance = this;
		else throw new System.Exception("Attempt to create second instance of singleton");
		
		// Clear Lorem Ipsums
		column1.text = string.Empty;
		column2.text = string.Empty;
		column3.text = string.Empty;
	}
	
	/// <summary>
	/// Sets given text in given column
	/// </summary>
	/// <param name="column"> Column from 1-3 range </param>
	/// <param name="text"> Text to display </param>
	public static void SetText(int column, string text)
	{
		switch (column)
		{
		case 1:
			Instance.column1.text = text;
			break;
		case 2:
			Instance.column2.text = text;
			break;
		case 3:
			Instance.column3.text = text;
			break;
		default:
			throw new System.NotImplementedException("Unknown column: " + column);
		}
	}
}
