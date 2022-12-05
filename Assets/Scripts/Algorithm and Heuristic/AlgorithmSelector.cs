using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgorithmSelector : MonoBehaviour
{
	[Tooltip("Singleton")]
	static AlgorithmSelector Instance;
	
	[Tooltip("Dropdown with all possible algorithms")]
	public TMPro.TMP_Dropdown algorithmDropdown;
	
	[Tooltip("Dropdown with all possible heuristics")]
	public TMPro.TMP_Dropdown heuristicDropdown;
	
	[Tooltip("Selected algorithm's description")]
	public TMPro.TMP_Text algorithmDescription;
	
	[Tooltip("Selected heuristic's description")]
	public TMPro.TMP_Text heuristicDescription;
	
	[Tooltip("All possible algorithms")]
	Algorithm[] algorithms;
	
	[Tooltip("All possible heuristics")]
	Heuristic[] heuristics;
	
	[Tooltip("Solver's coroutine")]
	
	
	
	
	
	
	// Awake is called when the script instance is being loaded.
	protected void Awake()
	{
		// Create singleton
		Instance = this;
		
		// Subscribe events
		algorithmDropdown.onValueChanged.AddListener(OnAlgorithmChanged);
		heuristicDropdown.onValueChanged.AddListener(OnHeuristicChanged);
		
		// Clear descriptions from Lorem Ipsum
		algorithmDescription.text = string.Empty;
		heuristicDescription.text = string.Empty;
		
		// Create algorithms and heuristics objects
		InitializeAlgorithmsAndHeuristics();
		
		// Populate algorithms and heuristics dropdown
		PopulateAlgorithmsDropdown();
		PopulateHeuristicsDropdown(algorithms[0]);
	}
	
	/// <summary>
	/// Creates algorithms and heuristics objects
	/// </summary>
	void InitializeAlgorithmsAndHeuristics()
	{
		// Create heuristics objects
		heuristics = new Heuristic[]
		{
			new Dijkstra(),
			new Manhattan(),
		};
		
		// Create algorithms objects
		algorithms = new Algorithm[]
		{
			new AStar(Dijkstra.Instance, Manhattan.Instance),
			new HPAStar(Dijkstra.Instance, Manhattan.Instance),
			new JPS(Dijkstra.Instance)
		};
	}
	
	/// <summary>
	/// Creates algorithm dropdowns options
	/// </summary>
	void PopulateAlgorithmsDropdown()
	{
		// Clear current options
		algorithmDropdown.ClearOptions();
		
		// For each algorithm, create dropdown item
		foreach (Algorithm alg in algorithms)
			algorithmDropdown.options.Add(new TMPro.TMP_Dropdown.OptionData() { text = alg.name });
			
		// Select first algorithm
		algorithmDropdown.value = 0;
		algorithmDropdown.onValueChanged.Invoke(0);
	}
	
	/// <summary>
	/// Creates heuristic dropdowns options based on currently selected algorithm
	/// </summary>
	void PopulateHeuristicsDropdown(Algorithm selectedAlgorithm)
	{
		// Clear current options
		heuristicDropdown.ClearOptions();
		
		// For each heuristic for selected algorithm, create dropdown item
		foreach (Heuristic heur in selectedAlgorithm.heuristics)
			heuristicDropdown.options.Add(new TMPro.TMP_Dropdown.OptionData() { text = heur.name });
			
		// Select first heuristic
		heuristicDropdown.value = 0;
		heuristicDropdown.onValueChanged.Invoke(0);
	}
	
	/// <summary>
	/// Action invoked when algorithm's dropdown item changes
	/// </summary>
	void OnAlgorithmChanged(int index)
	{
		// Display algorithm's description
		algorithmDescription.text = algorithms[index].description;
		
		// Fill heuristics' dropdown with available heuristics
		PopulateHeuristicsDropdown(algorithms[index]);
		
		// Disable button responsible for details file
		DetailsButtonManager.DisableButton();
	}
	
	/// <summary>
	/// Action invoked when heuristic's dropdown item changes
	/// </summary>
	void OnHeuristicChanged(int index)
	{
		// Display heuristic's description
		heuristicDescription.text = heuristics[index].description;
		
		// Update dropdown layer (it can bug and don't display heuristic's name)
		heuristicDropdown.captionText.text = heuristics[index].name;
	}
	
	/// <returns> Currently selected algorithm </returns>
	public static Algorithm GetAlgorithm() => Instance.algorithms[Instance.algorithmDropdown.value];
	
	/// <returns> Currently selected heuristic </returns>
	public static Heuristic GetHeuristic() => Instance.heuristics[Instance.heuristicDropdown.value];
}
