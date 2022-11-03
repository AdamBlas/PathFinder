using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Solver : MonoBehaviour
{
	[Tooltip("Flag that switches between animation and instant mode")]
	public static bool animateSolvingProcess;
	
	[Tooltip("Toggle responsible for animation/instant mode")]
	public Toggle animateSolvingProcessToggle;
	
	[Tooltip("Delay value if solving process is supposed to be animated")]
	public static float delay;
	
	[Tooltip("Slider responsible for delay length")]
	public Slider delaySlider;
	
	[Tooltip("Button responsible for pausing")]
	public Button pauseButton;
	
	[Tooltip("Button responsible for resuming")]
	public Button resumeButton;
	
	[Tooltip("Button responsible for next step")]
	public Button nextStepButton;
	
	[Tooltip("Solver's coroutine")]
	Coroutine solverCoroutine;
	
	
	
	
	
	
	// Awake is called when the script instance is being loaded.
	protected void Awake()
	{
		// Initialize buttons' states
		ResumeSolver();
		
		// Initialize values
		animateSolvingProcess = animateSolvingProcessToggle.isOn;
		delay = delaySlider.value;
	}
	
	/// <summary>
	/// Toggles between animation and instant mode
	/// </summary>
	public void ToggleAnimationAndInstantMode()
	{
		// Set flag
		animateSolvingProcess = animateSolvingProcessToggle.isOn;
		
		// Set slider's interaction state
		delaySlider.interactable = animateSolvingProcess;	
	}
	
	/// <summary>
	/// Changes delay's value
	/// </summary>
	public void ChangeDelayValue()
	{
		// Set value
		delay = delaySlider.value;
	}
	
	/// <summary>
	/// Solves given pathfinding probelm
	/// </summary>
	public void Solve()
	{
		// Clear path displayer
		Displayer.Instance.ClearPathLayer();
		
		// Start solving coroutine
		solverCoroutine = StartCoroutine(AlgorithmSelector.GetAlgorithm().Solve(AlgorithmSelector.GetHeuristic()));
	}
	
	/// <summary>
	/// Clears map and prepares for another solve
	/// </summary>
	public void Clear()
	{
		// Stop solving coroutine if such exists
		if (solverCoroutine != null)
			StopCoroutine(solverCoroutine);
		
		// Send invoke to displayer
		Displayer.Instance.ClearPathLayer();
	}
	
	/// <summary>
	/// Pauses solving animation by setting time scale to zero
	/// </summary>
	public void PauseSolver()
	{
		// Set time scale to zero, so WaitForSeconds in solving coroutine will not end
		Time.timeScale = 0;
		
		// Manage buttons' states
		pauseButton.interactable = false;
		resumeButton.interactable = true;
		nextStepButton.interactable = true;
	}
	
	/// <summary>
	/// Resumes solving animation by setting time scale back to one
	/// </summary>
	public void ResumeSolver()
	{
		// Reset time scale
		Time.timeScale = 1;
		
		// Manage buttons' states
		pauseButton.interactable = true;
		resumeButton.interactable = false;
		nextStepButton.interactable = false;
	}
	
	/// <summary>
	/// Resumes animation for one step and then pauses it back
	/// </summary>
	public void MoveOneStep()
	{
		// Coroutine responsible for changing time scale
		IEnumerator OneStep()
		{
			// By setting time scale to infinity, we ensure that WaitForSecond inside solver coroutine will end
			// Update: 100 is the max value, so it has to suffice
			Time.timeScale = 100;
			
			// Wait one frame
			yield return null;
			
			// Pause time once again
			Time.timeScale = 0;
		}
		
		// Run coroutine
		StartCoroutine(OneStep());
	}
}
