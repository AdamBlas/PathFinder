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
    class Node: IComparable<Node>
    {
        public int x, y;
        public float value;
        public (int, int) previousNode;

        public Node(int x, int y, float value)
        {
            this.x = x;
            this.y = y;
            this.value = value;
        }
        public Node(int x, int y, float value, (int, int) previousNode) : this(x, y, value)
        {
            this.previousNode = previousNode;
        }
        public int CompareTo(Node other)
        {
            if (value <= other.value)
            {
                if (value == 0)
                    return 1;
                return -1;
            }
            return 1;
        }
    }
    class SortedList
    {
        public int Count { get; private set; } = 0;
        readonly List<Node> sortedList = new List<Node>();
        readonly List<Node> newElements = new List<Node>();

        public void Add(Node newElement)
        {
            newElements.Add(newElement);
            Count += 1;
        }
        public void RemoveAtZero()
        {
            sortedList.RemoveAt(0);
            Count -= 1;
        }
        public Node GetAtZero()
        {
            return sortedList[0];
        }
        public void Sort()
        {
            foreach (Node element in newElements)
            {
                int index = sortedList.BinarySearch(element);
                if (index < 0)
                    index = -index - 1;
                sortedList.Insert(index, element);
            }
            newElements.Clear();
        }
    }

    static PathFinder instance;

    public Dropdown algorithm;
    public Dropdown heuristic;
    public InputField delayInput;
    public Slider delaySlider;
    public Button solveButton;
    public Button playButton;
    public Button pauseButton;
    public Button nextStepButton;
    public bool Fast { get; set; }

    float delay;
    (int, int, float)[] directions =
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
    Vector2Int start;
    Vector2Int end;
    bool isPaused = false;


    public void Awake()
    {
        instance = this;
    }
    public void Start()
    {
        UpdateButtonState();
        SetDelay();
        PlaySimulation();

        delaySlider.value = 0.1f;
        delayInput.text = delaySlider.value.ToString();
    }

    public void SetDelayFromInput()
    {
        if (string.IsNullOrWhiteSpace(delayInput.text))
            return;

        delay = float.Parse(delayInput.text);
        delaySlider.value = delay;
    }
    public void SetDelayFromSlider()
    {
        delay = (float)Math.Round(delaySlider.value, 1);
        delayInput.text = delay.ToString();
    }

    public void SetDelay()
    {
        if (!string.IsNullOrWhiteSpace(delayInput.text))
            delay = float.Parse(delayInput.text);
    }

    // Node value calculations
    delegate float Heuristic(Node sourceNode, int x, int y, float offset);
    float Dijkstra(Node sourceNode, int x, int y, float offset)
    {
        return sourceNode.value + offset;
    }
    float InversedDijkstra(Node sourceNode, int x, int y, float offset)
    {
        return Mathf.Pow(end.x - x, 2) + Mathf.Pow(end.y - y, 2);
    }

    public void Solve()
    {
        start = Paint.StartCoordinates.Value;
        end = Paint.EndCoordinates.Value;

        string algorithm = this.algorithm.options[this.algorithm.value].text;
        string heuristic = this.heuristic.options[this.heuristic.value].text;

        Heuristic h = heuristic switch
        {
            "Dijkstra" => Dijkstra,
            "Inversed Dijkstra" => InversedDijkstra,
            _ => throw new NotImplementedException(),
        };

        _ = algorithm switch
        {
            "A*" => StartCoroutine(AStar(h)),
            "Hillclimb" => StartCoroutine(Hillclimb(h)),
            _ => throw new NotImplementedException(),
        };
    }
    IEnumerator AStar(Heuristic heuristic)
    {
        Node lastNode = null;
        Node[,] nodes = new Node[Map.Width, Map.Height];
        SortedList list = new SortedList();

        for (int x = 0; x < Map.Width; x++)
            for (int y = 0; y < Map.Height; y++)
                nodes[x, y] = new Node(x, y, float.PositiveInfinity);

        list.Add(nodes[start.x, start.y]);
        nodes[start.x, start.y].previousNode = (-1, -1);
        bool endFound = false;

        while (list.Count != 0)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            list.Sort();
            Node node = list.GetAtZero();

            if (node.x == end.x && node.y == end.y)
                break;

            list.RemoveAtZero();
            foreach (var dir in directions)
            {
                int newX = node.x + dir.Item1;
                if (newX < 0 || newX >= Map.Width)
                    continue;

                int newY = node.y + dir.Item2;
                if (newY < 0 || newY >= Map.Height)
                    continue;

                Map.Node type = Map.RecentMap[newX, newY];
                if (type == Map.Node.Free)
                {
                    Map.RecentMap[newX, newY] = Map.Node.ToSearch;
                    float value = heuristic(node, node.x + dir.Item1, node.y + dir.Item2, dir.Item3);
                    Node newNode = new Node(newX, newY, value, (node.x, node.y));
                    list.Add(newNode);
                    nodes[newX, newY] = newNode;
                }
                else if (type == Map.Node.End)
                {
                    lastNode = node;
                    endFound = true;
                    break;
                }
            }

            if (!Fast)
            {
                Map.RecentMap[node.x, node.y] = Map.Node.Searched;
                ImageDisplayer.RefreshPixel(node.x, node.y);
                yield return new WaitForSeconds(delay);
            }

            if (endFound)
            {
                break;
            }
        }

        PrintOutputData(lastNode, nodes);
    }
    IEnumerator Hillclimb(Heuristic heuristic)
    {
        Node lastNode = null;
        Node[,] nodes = new Node[Map.Width, Map.Height];

        for (int x = 0; x < Map.Width; x++)
            for (int y = 0; y < Map.Height; y++)
                nodes[x, y] = new Node(x, y, float.PositiveInfinity);

        nodes[start.x, start.y].previousNode = (-1, -1);
        bool endFound = false;

        Node node = nodes[start.x, start.y];
        while (true)
        {
            if (isPaused)
            {
                yield return null;
                continue;
            }

            if (node.x == end.x && node.y == end.y)
                break;

            foreach (var dir in directions)
            {
                int newX = node.x + dir.Item1;
                if (newX < 0 || newX >= Map.Width)
                    continue;

                int newY = node.y + dir.Item2;
                if (newY < 0 || newY >= Map.Height)
                    continue;

                Map.Node type = Map.RecentMap[newX, newY];
                if (type == Map.Node.Free)
                {
                    float value = heuristic(node, node.x + dir.Item1, node.y + dir.Item2, dir.Item3);
                    if (value < node.value)
                    {
                        Node newNode = new Node(newX, newY, value, (node.x, node.y));
                        nodes[newX, newY] = newNode;
                        node = newNode;

                        break;
                    }
                }
                else if (type == Map.Node.End)
                {
                    lastNode = node;
                    endFound = true;
                    break;
                }
            }

            if (!Fast)
            {
                Map.RecentMap[node.x, node.y] = Map.Node.Searched;
                ImageDisplayer.RefreshPixel(node.x, node.y);
                yield return new WaitForSeconds(delay);
            }

            if (endFound)
            {
                break;
            }
        }

        PrintOutputData(lastNode, nodes);
    }
    void PrintOutputData(Node endNode, Node[,] map)
    {
        int searched = 0;
        int path = 0;
        int free = 0;

        for  (int x = 0; x < Map.Width; x++)
        {
            for (int y = 0; y < Map.Height; y++)
            {
                switch(Map.RecentMap[x, y])
                {
                    case Map.Node.Free:
                        free++;
                        break;
                    case Map.Node.Searched:
                        searched++;
                        break;
                    case Map.Node.Path:
                        path++;
                        break;
                }
            }
        }

        if (path == 0)
        {
            OutputMessageManager.SetMessage("Path not found!");
            return;
        }

        float length = 0;
        Node node = endNode;
        while (true)
        {
            int xDiff = node.x - node.previousNode.Item1;
            int yDiff = node.y - node.previousNode.Item2;
            length += Mathf.Sqrt(xDiff * xDiff + yDiff * yDiff);
            node = map[node.previousNode.Item1, node.previousNode.Item2];

            if (node.previousNode.Item1 == -1)
                break;
        }

        OutputMessageManager.SetMessage(
                "Path found!\n" +
                "Length:\n" +
                "Nodes in Path: \n" +
                "Nodes Searched:",
                column: 1);

        OutputMessageManager.SetMessage(
            "\n" +
            length + "\n" +
            path + "\n" +
            searched,
            column: 2);
    }


    public static void UpdateButtonState()
    {
        bool isActive = Map.RecentMap != null && Paint.StartCoordinates.HasValue && Paint.EndCoordinates.HasValue;
        instance.solveButton.interactable = isActive;
    }
    public void PlaySimulation()
    {
        isPaused = false;

        playButton.interactable = false;
        pauseButton.interactable = true;
        nextStepButton.interactable = false;
    }
    public void PauseSimulation()
    {
        isPaused = true;

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
        isPaused = false;
        yield return null;
        isPaused = true;
    }

    public void OnAlgorithmSet()
    {
        string algorithm = this.algorithm.options[this.algorithm.value].text;

        switch (algorithm)
        {
            case "A*":
                heuristic.interactable = true;
                break;
            case "Hillclimb":
                heuristic.interactable = false;
                heuristic.value = 1;
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public void DisplayDescription()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("ALGORITHM:");

        switch (algorithm.options[algorithm.value].text)
        {
            case "A*":
                sb.AppendLine("Examines nearby nodes. Assigns value to every node and picks the best one.");
                sb.AppendLine("Node's value is determined by heuristic.");
                break;
            case "Hillclimb":
                sb.AppendLine("Examines nearby nodes one by one. If it's value is better than current one, skip further examination and switch node.");
                sb.AppendLine("Node's value is determined by heuristic.");
                sb.AppendLine("There is rist of being stuck in local extremum.");
                break;
            default:
                throw new NotImplementedException();
        }

        sb.AppendLine();
        sb.AppendLine("HEURISTIC:");

        switch (heuristic.options[heuristic.value].text)
        {
            case "Dijkstra":
                sb.AppendLine("The best node is the one closest to the start node.");
                break;
            case "Inversed Dijkstra":
                sb.AppendLine("The best node is the one closest to the end node.");
                break;
            default:
                throw new NotImplementedException();
        }

        OutputMessageManager.SetMessage(sb.ToString(), column: 3);
    }
}
