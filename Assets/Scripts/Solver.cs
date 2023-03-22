using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Solver : MonoBehaviour
{
	[Tooltip("Singleton")]
	public static Solver Instance;
	
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
	
	[Tooltip("Flag indicating whether or not solver is running")]
	public static bool isRunning = false;

	
	
	
	
	// Awake is called when the script instance is being loaded.
	protected void Awake()
	{
		// Create singleton
		Instance = this;
		
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
		// Do nothing if solver is runnning
		if (isRunning)
			return;
		
		// Clear previous simulation's results
		Clear();
		
		// Run simulation
		solverCoroutine = StartCoroutine(AlgorithmSelector.GetAlgorithm().Solve(AlgorithmSelector.GetHeuristic(), 1, 0));
	}
	
	/// <summary>
	/// Solves multiple times using different parameters to give average statistics
	/// </summary>
	public void TotalSolve()
	{
		// Do nothing if solver is runnning
		if (isRunning)
			return;
		
		// Clear previous simulation's results
		Clear();
		
		// Run multiple simulations
		StartCoroutine(SolveWithParams());
	}
	
	/// <summary>
	/// Solves given pathfinding problem with changing parameters
	/// </summary>
	/// <returns></returns>
	public IEnumerator SolveWithParams()
	{
		// Extract data
		string mapName = LoadMap.mapName;
		string startCoords = "S" + StartGoalManager.startCol +"x" + StartGoalManager.startRow;
		string goalCoords = "G" + StartGoalManager.goalCol + "x" + StartGoalManager.goalRow;
		string dirName = "Statistics/" + mapName + "_" + startCoords + "_" + goalCoords + "/";
		
		// If such directory already exists, remove old data
		if (System.IO.Directory.Exists(dirName))
			System.IO.Directory.Delete(dirName, true);
		
		// Create folder for results
		System.IO.Directory.CreateDirectory(dirName);
		
		// Set file names
		AStar.Instance.statsFileName = dirName + "AStar.csv";
		HPAStar.Instance.statsFileName = dirName + "HPAStar.csv";
		JPS.Instance.statsFileName = dirName + "JPS.csv";
		
		// Prepare algorithm changing actions
		var algorithChangers = new System.Action[] {
			() => { AlgorithmSelector.Instance.algorithmDropdown.value = 0; },
			() => { AlgorithmSelector.Instance.algorithmDropdown.value = 1; },
			() => { AlgorithmSelector.Instance.algorithmDropdown.value = 2; },
		};
		
		// Take screenshot of the map
		SaveTextureToPng.StaticSaveToPng(dirName + mapName + "_" + startCoords + "_" + goalCoords + ".png");
		
		
		// Run solver for each algorithm
		foreach (var algorithm in algorithChangers)
		{
			// Change algorithm
			algorithm.Invoke();
		
			// Prepare heuristic changers
			var heuristicChangers = new System.Action[] {
				() => { AlgorithmSelector.Instance.heuristicDropdown.value = 0; },
				() => { AlgorithmSelector.Instance.heuristicDropdown.value = 1; },
			};
				
			// Run solver for each heuristic
			foreach (var heuristic in heuristicChangers)
			{
				// Change heuristic
				heuristic.Invoke();
				
				// HPA* needs additional operations that will change chunk size, so operations list will differ based on selected algorithm
				System.Action[] operations = null;
				if (AlgorithmSelector.GetAlgorithm().name != "HPA*")
				{
					// Prepare operations
					operations = new System.Action[] {
						() => { GoalBoundingManager.Instance.slider.value = 0; CostOverwriteManager.Instance.toggle.isOn = false; CostOverwriteManager.Instance.UpdateErrorMargin(0); },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.toggle.isOn = true; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.3f; },
					};
				}
				else
				{
					// Prepare operations
					operations = new System.Action[] {
						() => { ChunkSizeManager.Instance.slider.value = 2; GoalBoundingManager.Instance.slider.value = 0; CostOverwriteManager.Instance.toggle.isOn = false; CostOverwriteManager.Instance.UpdateErrorMargin(0); },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.toggle.isOn = true; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { ChunkSizeManager.Instance.slider.value = 3; GoalBoundingManager.Instance.slider.value = 0; CostOverwriteManager.Instance.toggle.isOn = false; CostOverwriteManager.Instance.UpdateErrorMargin(0); },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.toggle.isOn = true; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { ChunkSizeManager.Instance.slider.value = 4; GoalBoundingManager.Instance.slider.value = 0; CostOverwriteManager.Instance.toggle.isOn = false; CostOverwriteManager.Instance.UpdateErrorMargin(0); },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.toggle.isOn = true; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { ChunkSizeManager.Instance.slider.value = 6; GoalBoundingManager.Instance.slider.value = 0; CostOverwriteManager.Instance.toggle.isOn = false; CostOverwriteManager.Instance.UpdateErrorMargin(0); },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.toggle.isOn = true; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { ChunkSizeManager.Instance.slider.value = 8; GoalBoundingManager.Instance.slider.value = 0; CostOverwriteManager.Instance.toggle.isOn = false; CostOverwriteManager.Instance.UpdateErrorMargin(0); },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.toggle.isOn = true; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { ChunkSizeManager.Instance.slider.value = 12; GoalBoundingManager.Instance.slider.value = 0; CostOverwriteManager.Instance.toggle.isOn = false; CostOverwriteManager.Instance.UpdateErrorMargin(0); },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.toggle.isOn = true; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { ChunkSizeManager.Instance.slider.value = 16; GoalBoundingManager.Instance.slider.value = 0; CostOverwriteManager.Instance.toggle.isOn = false; CostOverwriteManager.Instance.UpdateErrorMargin(0); },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.toggle.isOn = true; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 2.5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 7.5f; CostOverwriteManager.Instance.slider.value = 0.3f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.1f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.2f; },
						() => { GoalBoundingManager.Instance.slider.value = 10f; CostOverwriteManager.Instance.slider.value = 0.3f; },
					};
				}
			
				// Clear path displayer
				Displayer.Instance.ClearPathLayer();
				
				// Run solver for every action
				foreach (var operation in operations)
				{
					// Prepare environment
					operation.Invoke();
					
					// Clear GUI
					Clear();
					
					// Start solving coroutine
					solverCoroutine = StartCoroutine(AlgorithmSelector.GetAlgorithm().Solve(AlgorithmSelector.GetHeuristic(), 12, 2));
				
					// Wait for solver to finish
					yield return new WaitUntil(() => isRunning == false);
				}
			}
		}
		
		// Reset parameters
		AlgorithmSelector.Instance.algorithmDropdown.value = 0;
		AlgorithmSelector.Instance.heuristicDropdown.value = 0;
		GoalBoundingManager.Instance.slider.value = 0;
		CostOverwriteManager.Instance.toggle.isOn = false;
		CostOverwriteManager.Instance.slider.value = 0f;
		Clear();
		
		// Open folder with statistics
		ResultDisplayer.SetText(1, "All data generated!");
		ResultDisplayer.SetText(2, string.Empty);
		ResultDisplayer.SetText(3, string.Empty);
		System.Diagnostics.Process.Start("Statistics");
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
