using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Diagnostics;
using System.Text;

public class PathFinder : MonoBehaviour
{
    public static PathFinder Instance { get; private set; }

    public Dropdown algorithmDropdown;
    public Dropdown heuristicDropdown;
    public InputField delayInput;
    public Slider delaySlider;
    public Button solveButton;
    public Button playButton;
    public Button pauseButton;
    public Button nextStepButton;
    public bool Fast { get; set; }

    public float Delay { get; private set; }
    public static (int, int, float)[] Directions { get; private set; } =
    {
        ( 0,  1, 1f),
        ( 1,  0, 1),
        ( 0, -1, 1),
        (-1,  0, 1),
        ( 1,  1, 1.41421f),
        ( 1, -1, 1.41421f),
        (-1, -1, 1.41421f),
        (-1,  1, 1.41421f),
    };
    public static Vector2Int StartCoordinate { get; private set; }
    public static Vector2Int EndCoordinate { get; private set; }
    public static bool IsPaused { get; private set; } = false;

    readonly Heuristic[] heuristics = {
        new Dijkstra(),
        new InversedDijkstra(),
        new Manhattan()
    };
    readonly Algorithm[] algorithms = {
        new AStar(),
        new Hillclimb(),
        new JPS()
    };

    public void Awake()
    {
        Instance = this;
    }
    public void Start()
    {
        UpdateButtonState();
        SetDelay();
        PlaySimulation();

        delaySlider.value = 0f;
        delayInput.text = delaySlider.value.ToString();

        PopulateAlgorithmsDropdown();
    }

    public void SetDelayFromInput()
    {
        if (string.IsNullOrWhiteSpace(delayInput.text))
            return;

        Delay = float.Parse(delayInput.text);
        delaySlider.value = Delay;
    }
    public void SetDelayFromSlider()
    {
        Delay = (float)Math.Round(delaySlider.value, 1);
        delayInput.text = Delay.ToString();
    }
    public void SetDelay()
    {
        if (!string.IsNullOrWhiteSpace(delayInput.text))
            Delay = float.Parse(delayInput.text);
    }

    void PopulateAlgorithmsDropdown()
    {
        algorithmDropdown.ClearOptions();
        var options = new List<Dropdown.OptionData>();
        foreach (var a in algorithms)
            options.Add(new Dropdown.OptionData(a.Name));
        algorithmDropdown.AddOptions(options);
    }
    public void OnAlgorithmSet()
    {
        UpdateHeuristicDropdown();
        DisplayDescription();
    }
    public void OnHeuristicSet()
    {
        DisplayDescription();
    }
    public void UpdateHeuristicDropdown()
    {
        PopulateHeuristicsDropdown(algorithms[algorithmDropdown.value]);
    }
    void PopulateHeuristicsDropdown(Algorithm algorithm)
    {
        heuristicDropdown.ClearOptions();
        var options = new List<Dropdown.OptionData>();
        foreach (var h in algorithm.AvaliableHeuristics)
            options.Add(new Dropdown.OptionData(h.Name));
        heuristicDropdown.AddOptions(options);
    }

    public void Solve()
    {
        StartCoordinate = Paint.StartCoordinates.Value;
        EndCoordinate = Paint.EndCoordinates.Value;

        Algorithm algorithm = algorithms[algorithmDropdown.value];
        Heuristic heuristic = algorithm.AvaliableHeuristics[heuristicDropdown.value];

        StartCoroutine(algorithm.Solve(heuristic, StartCoordinate, EndCoordinate));
    }
    
    

    // GUI
    public static void UpdateButtonState()
    {
        bool isActive = Map.RecentMap != null && Paint.StartCoordinates.HasValue && Paint.EndCoordinates.HasValue;
        Instance.solveButton.interactable = isActive;
    }
    public void PlaySimulation()
    {
        IsPaused = false;

        playButton.interactable = false;
        pauseButton.interactable = true;
        nextStepButton.interactable = false;
    }
    public void PauseSimulation()
    {
        IsPaused = true;

        playButton.interactable = true;
        pauseButton.interactable = false;
        nextStepButton.interactable = true;
    }
    public void MoveSimulationToNextStep()
    {
        StartCoroutine(NextStep());
    }
    IEnumerator NextStep()
    {
        IsPaused = false;
        yield return null;
        IsPaused = true;
    }

    public void DisplayDescription()
    {
        StringBuilder sb = new StringBuilder();
        Algorithm algorithm = algorithms[algorithmDropdown.value];
        Heuristic heuristic = algorithm.AvaliableHeuristics[heuristicDropdown.value];

        sb.AppendLine("ALGORITHM:");
        sb.AppendLine(algorithm.Description);
        sb.AppendLine();
        sb.AppendLine("HEURISTIC:");
        sb.AppendLine(heuristic.Description);

        OutputMessageManager.SetMessage(sb.ToString(), column: 3);
    }
}
