using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solver : MonoBehaviour
{
	[Tooltip("Flag that switches between animation and instant mode")]
	bool animateSolvingProcess;
	
	
	
	
	
	
	
	/// <summary>
	/// Toggles between animation and instant mode
	/// </summary>
	public void ToggleAnimationAndInstantMode(bool state)
	{
		Debug.Log(state);
	}
}
