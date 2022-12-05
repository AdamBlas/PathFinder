using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static NodeType;
using static Node;

public class JPS : Algorithm
{
	/// <summary>
	/// Class represents single jump point in the JPS algorithm
	/// </summary>
	class JumpPoint
	{
		// Flags indicating whether or not there is another jump point in that direction
		public bool N, NE, E, SE, S, SW, W, NW;
		
		public override string ToString()
		{
			// Prepare result variable
			string result = string.Empty;

			// Add directions to result string
			if (N)  result += "N, ";
			if (NE) result += "NE, ";
			if (E)  result += "E, ";
			if (SE) result += "SE, ";
			if (S)  result += "S, ";
			if (SW) result += "SW, ";
			if (W)  result += "W, ";
			if (NW) result += "NW, ";
			
			// Return final value
			return result;
		}
	}
	
	/// <summary>
	/// Class represents single node in a JPS map
	/// </summary>
	class DistancenNode
	{
		[Tooltip("Memory usage per single JpsNode")]
		public const int MEMORY_USAGE = 4 + 4 + (8 * 5) + 1;
		// int = 4 bytes
		// Nullable int = 5 bytes
		// bool = 1 byte
		
		// Node's coordinates
		int x, y;
		
		// Distances to jump points or walls
		public int? N, NE, E, SE, S, SW, W, NW;
		
		// Flag indiacting whether or not node is free
		public bool free = true;
	
		
		
		
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="row"> Y coordinate of the node</param>
		/// <param name="col"> X coordinate of the node </param>
		public DistancenNode(int row, int col)
		{
			// Assign coords
			x = col;
			y = row;
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="row"> Y coordinate of the node </param>
		/// <param name="col"> X coordinate of the node </param>
		/// <param name="N"> Distance to jump point or wall in the north </param>
		/// <param name="E"> Distance to jump point or wall in the east </param>
		/// <param name="S"> Distance to jump point or wall in the south </param>
		/// <param name="W"> Distance to jump point or wall in the west </param>
		/// <returns></returns>
		public DistancenNode(int row, int col, int N, int E, int S, int W)
		{
			// Assign coords
			x = col;
			y = row;
			
			// Assign distances
			this.N = N;
			this.E = E;
			this.S = S;
			this.W = W;
		}
		
		public override string ToString()
		{
			// Prepare result variable
			string result = string.Empty;
			
			// Add directions to result string
			if (N.HasValue)  result += "N" + N.Value + ", ";
			if (NE.HasValue) result += "NE" + NE.Value + ", ";
			if (E.HasValue)  result += "E" + E.Value + ", ";
			if (SE.HasValue) result += "SE" + SE.Value + ", ";
			if (S.HasValue)  result += "S" + S.Value + ", ";
			if (SW.HasValue) result += "SW" + SW.Value + ", ";
			if (W.HasValue)  result += "W" + W.Value + ", ";
			if (NW.HasValue) result += "NW" + NW.Value + ", ";
			
			return result;
		}
		
		/// <summary>
		/// Returns that node in a string array version, perfect for graphical interpretation
		/// </summary>
		public string[] ToStringArray(int x, int y, bool isPrimary, bool isStraight, bool isDiagonal)
		{
			// Prepare array for results
			string[] result = new string[9];
			
			// If node is free, print that node with all distances
			if (free)
			{
				result[0] = "┌─────────────────┐";
				result[1] = "│ " + (NW.HasValue ? NW.Value.ToString().PadRight(5) : "     ") + (N.HasValue ? N.Value.ToString().PadRight(3).PadLeft(5) : "     ") + (NE.HasValue ? NE.Value.ToString().PadLeft(5) : "     ") + " │";
				result[2] = "│                 │";
				result[3] = "│" + (x + "×" + y).PadRight(10).PadLeft(17) + "│";
				result[4] = "│ " + (W.HasValue ? W.Value.ToString().PadRight(5) : "     ") + (isPrimary ? " PRM " : "     ") + (E.HasValue ? E.Value.ToString().PadLeft(5) : "     ") + " │";
				result[5] = "│       " + (isStraight ? "STR" : "   ") + "       │";
				result[6] = "│       " + (isDiagonal ? "DGN" : "   ") + "       │";
				result[7] = "│ " + (SW.HasValue ? SW.Value.ToString().PadRight(5) : "     ") + (S.HasValue ? S.Value.ToString().PadRight(3).PadLeft(5) : "     ") + (SE.HasValue ? SE.Value.ToString().PadLeft(5) : "     ") + " │";
				result[8] = "└─────────────────┘";
				
				return result;
			}
			// If node is obstacle, create solid block
			else
			{
				result[0] = "███████████████████";
				result[1] = "███████████████████";
				result[2] = "███████████████████";
				result[3] = "███████████████████";
				result[4] = "███████████████████";
				result[5] = "███████████████████";
				result[6] = "███████████████████";
				result[7] = "███████████████████";
				result[8] = "███████████████████";
				
				return result;
			}
		}
	}
	
	class NodeWithDir : Node
	{
		public enum Dir { N, NE, E, SE, S, SW, W, NW }
		
		public Dir? travelDir = null;
		
		public NodeWithDir(int x, int y, float cost) : base(x, y, cost) { }

		public NodeWithDir(int x, int y, Node parentNode, Heuristic heuristic) : base(x, y, parentNode, heuristic) { }
	}

	
	
	
	
	
	
	
	
	
	[Tooltip("Algorithm's singleton")]
	public static JPS Instance;
	
	[Tooltip("Array of primary jump points")]
	JumpPoint[,] primary;
	
	[Tooltip("Array of straight jump points")]
	JumpPoint[,] straight;
	
	[Tooltip("Array of diagonal jump points")]
	JumpPoint[,] diagonal;
 	
	[Tooltip("Array with information about distances")]
	DistancenNode[,] distanceMap;

	[Tooltip("Time required for algorithm to find path in a distance map")]
	float timeToFindPath;
 	
	[Tooltip("FInal node in the path")]
	Node finalNode;
 	
 	
 	
 	
 	
	
	
	/// <summary>
	/// Constructor
	/// </summary>
	public JPS(params Heuristic[] heuristics) : base(heuristics)
	{
		// Set name and description
		name = "JPS+";
		description = "JPS+ (Jump Point Search) - Algorithm first precalculates map creating data structure containing information of points that can be part of the path and then uses it it find the final path.";
	
		// Create singleton
		Instance = this;
	}
	
	/// <summary>
	/// Returns 1 if value is positive, 0 if is zero and -1 if value is negative
	/// </summary>
	/// <param name="value"> Value to check </param>
	int Sign(int value)
	{
		if (value == 0) return 0;
		if (value > 0) return 1;
		return -1;
	}
	
	/// <summary>
	/// Prints path from final node back to the goal node. It's a special version for JPS purposes
	/// </summary>
	/// <param name="node"> Final node in the path </param>
	/// <param name="nodesAmount"> Out parameter, amount of nodes in the path </param>
	/// <param name="pathLength"> Out parameter, length of the path </param>
	new void PrintPath(Node node, out int nodesAmount, out float pathlLength)
	{
		// Paint all pixels on displayer and calculate path
		nodesAmount = 0;
		pathlLength = 0;
		
		while (node.parentNode != null)
		{
			// Get distances to parent
			int distanceToParentX = node.x - node.parentNode.x;
			int distanceToParentY = node.y - node.parentNode.y;
			
			// Get single value
			int distanceToParent = Mathf.Max(Mathf.Abs(distanceToParentX), Mathf.Abs(distanceToParentY));
			
			// Get offsets per iteration
			int xOffset = Sign(distanceToParentX);
			int yOffset = Sign(distanceToParentY);
			
			// Paint all pixels in a path from this node to parent node
			for (int i = 0; i < distanceToParent; i++)
			{
				// Paint pixel
				if (i == 0)
					// Let Jump Point pixel has different color
					Displayer.Instance.PaintPath(node.x, node.y, Displayer.Instance.pathColor);
				else
					Displayer.Instance.PaintPath(node.x - (i * xOffset), node.y - (i * yOffset), Displayer.Instance.subpathColor1);
			}
			
			// Add distances
			nodesAmount += distanceToParent;
			pathlLength += Mathf.Sqrt(Mathf.Pow(node.x - node.parentNode.x, 2) + Mathf.Pow(node.y - node.parentNode.y, 2));
			
			// Switch to parent
			node = node.parentNode;
		}
	}
	
	/// <summary>
	/// Solves current problem using A* algorithm
	/// </summary>
	/// <param name="heuristic"> Heuristic used to calculate node's cost </param>
	public override IEnumerator Solve(Heuristic heuristic)
	{
		// Prepare timer
		var timer = new System.Diagnostics.Stopwatch();
		timer.Start();
		
		// Save heuristic
		this.heuristic = heuristic;
		
		// Generate map with all jump points and distances
		GenerateJpsMap();
		timer.Stop();
		float timeToGenerateMap = (float)timer.Elapsed.TotalMilliseconds;
		timer.Reset();
			
		// Set details button action to generate file with detailed data
		DetailsButtonManager.EnableButton();
		DetailsButtonManager.ButtonComponent.onClick.RemoveAllListeners();
		DetailsButtonManager.ButtonComponent.onClick.AddListener(() => Solver.Instance.StartCoroutine(PrintMapToFile()));
		
		// Get final node in the path
		yield return FindPath();
		
		// Check if path was found
		if (finalNode == null)
		{
			ResultDisplayer.SetText(1, "FAILURE:\nPath not found.");
			ResultDisplayer.SetText(2, "");
			ResultDisplayer.SetText(3, "");
			yield break;
		}
		
		// Get amount of nodes allocated BEFORE painting path
		// Doing so after will result in middle nodes in a path to be counted as allocated too
		int nodesAllocated = Displayer.GetAmountOfNodesAllocated();
		
		// Print path
		PrintPath(finalNode, out int nodesLength, out float pathLength);
		
		// Prepare messages to display
		string msg1 = "TIME";
		msg1 += "\nMap creation:\t" + timeToGenerateMap.ToString("f3") + " ms";
		msg1 += "\nTo find path:\t\t" + timeToFindPath.ToString("f3") + " ms";
		msg1 += "\nTotal:\t\t\t" + (timeToGenerateMap + timeToFindPath).ToString("f3") + " ms";
		msg1 += "\n\nPATH LENGTH";
		msg1 += "\nNodes:\t" + nodesLength;
		msg1 += "\nDistance:\t" + pathLength.ToString("f2");
		
		string msg2 = "NODES AMOUNT";
		msg2 += "\nAnalyzed:\t\t" + nodesAnalyzed;
		msg2 += "\nAllocated:\t\t" + nodesAllocated;
		
		string msg3 = "MEMORY USAGE";
		msg3 += "\nPer node:\t\t" + Node.MEMORY_USAGE + " B";
		msg3 += "\nPr map point:\t" + DistancenNode.MEMORY_USAGE + " B";
		msg3 += "\nFull path:\t\t" + (Node.MEMORY_USAGE * nodesLength) + " B";
		msg3 += "\nFull map:\t\t" + (DistancenNode.MEMORY_USAGE * Map.width * Map.height) + " B";
		msg3 += "\nTotal:\t\t\t" + ((Node.MEMORY_USAGE * nodesLength) + (DistancenNode.MEMORY_USAGE * Map.width * Map.height)) + " B";
		
		// Print messages
		ResultDisplayer.SetText(1, msg1);
		ResultDisplayer.SetText(2, msg2);
		ResultDisplayer.SetText(3, msg3);
	}
	
	/// <summary>
	/// Tries to expand node to a target position
	/// </summary>
	/// <param name="x"> Target X coordinate </param>
	/// <param name="y"> target Y coordinate </param>
	/// <param name="distance"> Distance in that direction from distance map </param>
	/// <param name="node"> Parent node </param>
	void TryToExpand(int x, int y, int distance, Node node, NodeWithDir.Dir direction)
	{
		// Check if there is point in expanding
		if (distance == 0)
			return;
		
		// Prepare variable that will store found coordinates. If after searching this value is still null, that means that no target jump point was found
		Vector2Int? newNodeCoords = null;
		
		// Precalculate absolute value fo teh distance
		int distanceAbs = Mathf.Abs(distance);
		
		if (x == node.x || y == node.y)
		{
			// Movement is cardinal, check if there is straight path to goal
			if ((x == StartGoalManager.goalCol || y == StartGoalManager.goalRow) &&
				Mathf.Abs(x - StartGoalManager.goalCol + y - StartGoalManager.goalRow) <= distanceAbs)
			{
				// There is straight path to the goal, save coordinates for a new Target Jump Point
				newNodeCoords = new Vector2Int(StartGoalManager.goalCol, StartGoalManager.goalRow);
			}
			else
			{
				// Check if we are trying to move towards the goal
				int movementDirX = x - node.x;
				int movementDirY = y - node.y;
				int goalDirX = StartGoalManager.goalCol - node.x;
				int goalDirY = StartGoalManager.goalRow - node.y;
				
				if (movementDirX * goalDirX > 0 || movementDirY * goalDirY > 0)
				{
					// Movement and goal direction have the same signs
					// Calculate desired length of the movement
					int	desiredMovementLength;
					
					if (movementDirX == 0)
						desiredMovementLength = Mathf.Abs(movementDirY) - Mathf.Abs(goalDirX);
					else
						desiredMovementLength = Mathf.Abs(movementDirX) - Mathf.Abs(goalDirY);
					
					// Check if we can go such distance
					if (desiredMovementLength <= distanceAbs)
					{
						int newX, newY;
						
						if (movementDirX == 0)
						{
							newX = node.x;
							newY = node.y + (desiredMovementLength * Sign(movementDirY));
						}
						else
						{
							newX = node.x + (desiredMovementLength * Sign(movementDirX));
							newY = node.y;
						}
						
						// Assign coordinates
						//newNodeCoords = new Vector2Int(newX, newY);
					}
				}
			}
		}
		else
		{
			// Movement is not cardinal, so it must be diagonal
			//Debug.Log("Diagonal");
			
			// Check if goal node is in that direction
			int movementDirX = x - node.x;
			int movementDirY = y - node.y;
			int goalDirX = StartGoalManager.goalCol - node.x;
			int goalDirY = StartGoalManager.goalRow - node.y;
			
			if (movementDirX * goalDirX >= 0 || movementDirY * goalDirY >= 0)
			{
				// Movement direction and direction to goal have the same sign, so goal is in that direction
				// Check if we can get there without hitting wall or other jump point
				int goalDirXAbs = Mathf.Abs(goalDirX);
				int goalDirYAbs = Mathf.Abs(goalDirY);
				if (goalDirXAbs <= distanceAbs || goalDirYAbs <= distanceAbs)
				{
					// Get smaller value
					int min = Mathf.Min(Mathf.Abs(goalDirX), Mathf.Abs(goalDirY));
					
					// Secure value, as it cannot be equal to zero (that would result in no movement)
					if (min == 0) min = 1;
					
					// Calculate target positions
					int newX = node.x + (min * Sign(movementDirX));
					int newY = node.y + (min * Sign(movementDirY));
					
					// Save coordinates for a new Target Jump Point
					newNodeCoords = new Vector2Int(newX, newY);
				}
			}	
		}
		
		// Check if target jump point coords were defined
		if (newNodeCoords == null)
		{
			// No target jump point coorde were defined, check if there is point in creating onde
			if (distance > 0)
				// There is point in going in that direction
				newNodeCoords = new Vector2Int(x, y);
			else
				// There is no point in going in that direction
				return;
		}
		
		// Check if that point was already visited
		if (nodesVisited[newNodeCoords.Value.x, newNodeCoords.Value.y] == null)
		{
			// Point wasn't visited before, create new node and assign precalculated costs
			nodesVisited[newNodeCoords.Value.x, newNodeCoords.Value.y] = new NodeWithDir(newNodeCoords.Value.x, newNodeCoords.Value.y, node, heuristic);
			((NodeWithDir)nodesVisited[newNodeCoords.Value.x, newNodeCoords.Value.y]).travelDir = direction;
			
			// Add that point to the list and mark it on displayer
			list.Add(nodesVisited[newNodeCoords.Value.x, newNodeCoords.Value.y]);
			Displayer.Instance.PaintPath(newNodeCoords.Value.x, newNodeCoords.Value.y, Displayer.Instance.toAnalyzeColor);
		}
		else
		{
			// Node was already visited, check if new cost is better than old one
			heuristic.GetCosts(newNodeCoords.Value.x, newNodeCoords.Value.y, node, out float baseCost, out float goalBoundingCost);
			
			// Since cost overwriting is implemented in JPS+ by default, don't use error margin
			if (goalBoundingCost < nodesVisited[newNodeCoords.Value.x, newNodeCoords.Value.y].goalBoundCost)
			{
				// New node values are better, overwrite them
				nodesVisited[newNodeCoords.Value.x, newNodeCoords.Value.y].parentNode = node;
				nodesVisited[newNodeCoords.Value.x, newNodeCoords.Value.y].baseCost = baseCost;
				nodesVisited[newNodeCoords.Value.x, newNodeCoords.Value.y].goalBoundCost = goalBoundingCost;
				((NodeWithDir)nodesVisited[newNodeCoords.Value.x, newNodeCoords.Value.y]).travelDir = direction;
				
				// Re-add it ot the list
				list.Add(nodesVisited[newNodeCoords.Value.x, newNodeCoords.Value.y]);
			}
		}
	}
	
	/// <summary>
	/// Finds path in generated distance map and saves last node in a finalNode variable
	/// </summary>
	IEnumerator FindPath()
	{
		// Create timer
		System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
		timer.Start();
		
		// Create open list and initialize it with start node
		NodeWithDir node = new NodeWithDir(StartGoalManager.startCol, StartGoalManager.startRow, 0);
		list = new NodeSortedList(node);
		
		// Prepare array with information about previously visited nodes
		nodesVisited = new NodeWithDir[Map.width, Map.height];
		nodesVisited[StartGoalManager.startCol, StartGoalManager.startRow] = (Node)node;
		
		// Prepare variables that will store result
		nodesAnalyzed = 0;
		bool pathFound = false;
		
		// This counter will secure loop from running infinitely
		int securityCounter = 0;
		int mapSize = Map.width * Map.height * 2;
		
		// Start loop
		while (true)
		{
			// Check security counter
			if (securityCounter++ >= mapSize)
			{
				Debug.LogWarning("WARNING: Solving loop stopped for security reasons");
				break;
			}
			
			// Check if there is node to analyze
			if (list.count == 0)
				break;
				
			// Get best node from list
			node = (NodeWithDir)list.PopAtZero();
			nodesAnalyzed++;
			
			// Check if this is goal node
			if (node.x == StartGoalManager.goalCol && node.y == StartGoalManager.goalRow)
			{
				pathFound = true;
				break;
			}
			
			// Shortcut variables
			int x = node.x;
			int y = node.y;
			DistancenNode dist = distanceMap[x, y];

			// Expand in certain direction based on travel direction
			int N, NE, E, SE, S, SW, W, NW;
			
			switch (node.travelDir)
			{
			case null:
				// Prepare absolute values
				N = Mathf.Abs(dist.N.Value);
				NE = Mathf.Abs(dist.NE.Value);
				E = Mathf.Abs(dist.E.Value);
				SE = Mathf.Abs(dist.SE.Value);
				S = Mathf.Abs(dist.S.Value);
				SW = Mathf.Abs(dist.SW.Value);
				W = Mathf.Abs(dist.W.Value);
				NW = Mathf.Abs(dist.NW.Value);
				
				// Expand in given directions
				TryToExpand(x, y - N, dist.N.Value, node, NodeWithDir.Dir.N);
				TryToExpand(x + NE, y - NE, dist.NE.Value, node, NodeWithDir.Dir.NE);
				TryToExpand(x + E, y, dist.E.Value, node, NodeWithDir.Dir.E);
				TryToExpand(x + SE, y + SE, dist.SE.Value, node, NodeWithDir.Dir.SE);
				TryToExpand(x, y + S, dist.S.Value, node, NodeWithDir.Dir.S);
				TryToExpand(x - SW, y + SW, dist.SW.Value, node, NodeWithDir.Dir.SW);
				TryToExpand(x - W, y, dist.W.Value, node, NodeWithDir.Dir.W);
				TryToExpand(x - NW, y - NW, dist.NW.Value, node, NodeWithDir.Dir.NW);
				break;
			case NodeWithDir.Dir.N:
				// Prepare absolute values
				E = Mathf.Abs(dist.E.Value);
				NE = Mathf.Abs(dist.NE.Value);
				N = Mathf.Abs(dist.N.Value);
				NW = Mathf.Abs(dist.NW.Value);
				W = Mathf.Abs(dist.W.Value);
			
				// Expand in given directions
				TryToExpand(x + E, y, dist.E.Value, node, NodeWithDir.Dir.E);
				TryToExpand(x + NE, y - NE, dist.NE.Value, node, NodeWithDir.Dir.NE);
				TryToExpand(x, y - N, dist.N.Value, node, NodeWithDir.Dir.N);
				TryToExpand(x - NW, y - NW, dist.NW.Value, node, NodeWithDir.Dir.NW);
				TryToExpand(x - W, y, dist.W.Value, node, NodeWithDir.Dir.W);
				break;
			case NodeWithDir.Dir.NE:
				// Prepare absolute values
				E = Mathf.Abs(dist.E.Value);
				NE = Mathf.Abs(dist.NE.Value);
				N = Mathf.Abs(dist.N.Value);
			
				// Expand in given directions
				TryToExpand(x + E, y, dist.E.Value, node, NodeWithDir.Dir.E);
				TryToExpand(x + NE, y - NE, dist.NE.Value, node, NodeWithDir.Dir.NE);
				TryToExpand(x, y - N, dist.N.Value, node, NodeWithDir.Dir.N);
				break;
			case NodeWithDir.Dir.E:
				// Prepare absolute values
				S = Mathf.Abs(dist.S.Value);
				SE = Mathf.Abs(dist.SE.Value);
				E = Mathf.Abs(dist.E.Value);
				NE = Mathf.Abs(dist.NE.Value);
				N = Mathf.Abs(dist.N.Value);
			
				// Expand in given directions
				TryToExpand(x, y + S, dist.S.Value, node, NodeWithDir.Dir.S);
				TryToExpand(x + SE, y + SE, dist.SE.Value, node, NodeWithDir.Dir.SE);
				TryToExpand(x + E, y, dist.E.Value, node, NodeWithDir.Dir.E);
				TryToExpand(x + NE, y - NE, dist.NE.Value, node, NodeWithDir.Dir.NE);
				TryToExpand(x, y - N, dist.N.Value, node, NodeWithDir.Dir.N);
				break;
			case NodeWithDir.Dir.SE:
				// Prepare absolute values
				S = Mathf.Abs(dist.S.Value);
				SE = Mathf.Abs(dist.SE.Value);
				E = Mathf.Abs(dist.E.Value);
			
				// Expand in given directions
				TryToExpand(x, y + S, dist.S.Value, node, NodeWithDir.Dir.S);
				TryToExpand(x + SE, y + SE, dist.SE.Value, node, NodeWithDir.Dir.SE);
				TryToExpand(x + E, y, dist.E.Value, node, NodeWithDir.Dir.E);
				break;
			case NodeWithDir.Dir.S:
				// Prepare absolute values
				W = Mathf.Abs(dist.W.Value);
				SW = Mathf.Abs(dist.SW.Value);
				S = Mathf.Abs(dist.S.Value);
				SE = Mathf.Abs(dist.SE.Value);
				E = Mathf.Abs(dist.E.Value);
			
				// Expand in given directions
				TryToExpand(x - W, y, dist.W.Value, node, NodeWithDir.Dir.W);
				TryToExpand(x - SW, y + SW, dist.SW.Value, node, NodeWithDir.Dir.SW);
				TryToExpand(x, y + S, dist.S.Value, node, NodeWithDir.Dir.S);
				TryToExpand(x + SE, y + SE, dist.SE.Value, node, NodeWithDir.Dir.SE);
				TryToExpand(x + E, y, dist.E.Value, node, NodeWithDir.Dir.E);
				break;
			case NodeWithDir.Dir.SW:
				// Prepare absolute values
				W = Mathf.Abs(dist.W.Value);
				SW = Mathf.Abs(dist.SW.Value);
				S = Mathf.Abs(dist.S.Value);
			
				// Expand in given directions
				TryToExpand(x - W, y, dist.W.Value, node, NodeWithDir.Dir.W);
				TryToExpand(x - SW, y + SW, dist.SW.Value, node, NodeWithDir.Dir.SW);
				TryToExpand(x, y + S, dist.S.Value, node, NodeWithDir.Dir.S);	
				break;
			case NodeWithDir.Dir.W:
				// Prepare absolute values
				N = Mathf.Abs(dist.N.Value);
				NW = Mathf.Abs(dist.NW.Value);
				W = Mathf.Abs(dist.W.Value);
				SW = Mathf.Abs(dist.SW.Value);
				S = Mathf.Abs(dist.S.Value);
			
				// Expand in given directions
				TryToExpand(x, y - N, dist.N.Value, node, NodeWithDir.Dir.N);
				TryToExpand(x - NW, y - NW, dist.NW.Value, node, NodeWithDir.Dir.NW);
				TryToExpand(x - W, y, dist.W.Value, node, NodeWithDir.Dir.W);
				TryToExpand(x - SW, y + SW, dist.SW.Value, node, NodeWithDir.Dir.SW);
				TryToExpand(x, y + S, dist.S.Value, node, NodeWithDir.Dir.S);
				break;
			case NodeWithDir.Dir.NW:
				// Prepare absolute values
				N = Mathf.Abs(dist.N.Value);
				NW = Mathf.Abs(dist.NW.Value);
				W = Mathf.Abs(dist.W.Value);
			
				// Expand in given directions
				TryToExpand(x, y - N, dist.N.Value, node, NodeWithDir.Dir.N);
				TryToExpand(x - NW, y - NW, dist.NW.Value, node, NodeWithDir.Dir.NW);
				TryToExpand(x - W, y, dist.W.Value, node, NodeWithDir.Dir.W);
				break;
			}
			// Node expanded in every possible direction
			
			// Sort list
			list.Sort();
			
			// Update displayer
			Displayer.Instance.PaintPath(node.x, node.y, Displayer.Instance.analyzedColor);

			// If solver is supposed to be animated, pause it for some time
			if (Solver.animateSolvingProcess)
			{
				// If delay is small enough, animate only every 10th frame, otherwise even one frame delay will take long
				if (Solver.delay > 0.2f || securityCounter % 10 == 0)
				{
					// Pause timer
					timer.Stop();
					
					// Pause program for a short duration
					yield return new WaitForSeconds(Solver.delay);
					
					// Resume timer
					timer.Start();
				}
			}
		}
		
		// Stop the timer and save value
		timer.Stop();
		timeToFindPath = (float)timer.Elapsed.TotalMilliseconds;
		
		// Loop has ended, either path was found or all nodes were analyzed and no path was found
		if (pathFound == false)
		{
			finalNode = null;
			Debug.Log("Path not found");
			yield break;
		}
		// Path was found
		finalNode = node;
	}
	
	/// <summary>
	/// Generates jump points' maps
	/// </summary>
	void GenerateJpsMap()
	{
		// Prepare arrays of jump points
		primary = new JumpPoint[Map.width, Map.height];
		straight = new JumpPoint[Map.width, Map.height];
		diagonal = new JumpPoint[Map.width, Map.height];
		
		// Find primary and straight jump points
		GetPrimaryJumpPoints();
		GetStraightJumpPoints();
		
		// Now that we have all primary and straight jump points, let's calculate cardinal distances
		CalculateStraightDistances();
		
		// Now let's find diagonal jump points
		GetDiagonalJumpPoints();
		
		// Fill diagonal distances
		CalculateDiagonalDistances();
	}
	
	/// <summary>
	/// Generates primary jump points' map
	/// </summary>
	void GetPrimaryJumpPoints()
	{
		// Iterate through map
		for (int row = 0; row < Map.height; row++)
			for (int col = 0; col < Map.width; col++)
			{
				// Skip nodes that are not obstacles (primary jump points occur only next to obstacles)
				if (Map.map[row, col] != OBSTACLE)
					continue;
					
				// Prepare flags indicating if cardinal neighbours are free
				bool upFree = row - 1 >= 0 && Map.map[row - 1, col] == FREE;
				bool rightFree = col + 1 < Map.width && Map.map[row, col + 1] == FREE;
				bool bottomFree = row + 1 < Map.height && Map.map[row + 1, col] == FREE;
				bool leftFree = col - 1 >= 0 && Map.map[row, col - 1] == FREE;
					
				// Check if bottom-right neighbour is primary jump point
				if (rightFree && bottomFree && Map.map[row + 1, col] == FREE && Map.map[row, col + 1] == FREE && Map.map[row + 1, col + 1] == FREE)
				{
					// Create jump point if there wasn't one alreay
					if (primary[col + 1, row + 1] == null)
						primary[col + 1, row + 1] = new JumpPoint();
						
					// Set flags
					primary[col + 1, row + 1].N = true;
					primary[col + 1, row + 1].W = true;
				}

				// Check if top-right neighbour is primary jump point
				if (rightFree && upFree && Map.map[row - 1, col] == FREE && Map.map[row, col + 1] == FREE && Map.map[row - 1, col + 1] == FREE)
				{
					// Create jump point if there wasn't one already
					if (primary[col + 1, row - 1] == null)
						primary[col + 1, row - 1] = new JumpPoint();
					
					// Set flags
					primary[col + 1, row - 1].S = true;
					primary[col + 1, row - 1].W = true;
				}
				
				// Check if bottom-right neighbour is primary jump point
				if (leftFree && bottomFree && Map.map[row + 1, col] == FREE && Map.map[row, col - 1] == FREE && Map.map[row + 1, col - 1] == FREE)
				{
					// Create jump point if there wasn't one already
					if (primary[col - 1, row + 1] == null)
						primary[col - 1, row + 1] = new JumpPoint();
						
					// Set flags
					primary[col - 1, row + 1].N = true;
					primary[col - 1, row + 1].E = true;
				}
				
				// Check if top-left neightbour is primary jump point
				if (leftFree && upFree && Map.map[row - 1, col] == FREE && Map.map[row, col - 1] == FREE && Map.map[row - 1, col - 1] == FREE)
				{
					// Create jump point if there wasn't one already
					if (primary[col - 1, row - 1] == null)
						primary[col - 1, row - 1] = new JumpPoint();
						
					// Set flags
					primary[col - 1, row - 1].S = true;
					primary[col - 1, row - 1].E = true;
				}
			}
	}
	
	/// <summary>
	/// Generates straight jump points' map
	/// </summary>
	void GetStraightJumpPoints()
	{
		// Iterate through map
		for (int row = 0; row < Map.height; row++)
			for (int col = 0; col < Map.width; col++)
			{
				// Skip cells that are not free
				if (Map.map[row, col] != FREE)
					continue;
					
				// Skip nodes that are not primary jump points (straight jump points require primary jump point)
				if (primary[col, row] == null)
					continue;
					
				// Fill all nodes above primary jump point
				for (int x = 1; ; x++)
				{
					// Finish loop when getting out of map or hitting a wall
					if (row - x < 0 || Map.map[row - x, col] == OBSTACLE)
						break;
						
					// Create jump point if there wasn't one already
					if (straight[col, row - x] == null)
						straight[col, row - x] = new JumpPoint();
						
					// Set flag
					straight[col, row - x].S = true;
				}
				
				// Fill all nodes below primary jump point
				for (int x = 1; ; x++)
				{
					// Finish loop when getting out of map or hitting a wall
					if (row + x >= Map.height || Map.map[row + x, col] == OBSTACLE)
						break;
						
					// Create jump point if there wasn't one already
					if (straight[col, row + x] == null)
						straight[col, row + x] = new JumpPoint();
						
					// Set flag
					straight[col, row + x].N = true;
				}
				
				// Fill all nodes on the right from the primary jump point
				for (int x = 1; ; x++)
				{
					// Finish loop when getting out of map or hitting a wall
					if (col + x >= Map.width || Map.map[row, col + x] == OBSTACLE)
						break;
						
					// Create jump point if there wasn't one already
					if (straight[col + x, row] == null)
						straight[col + x, row] = new JumpPoint();
						
					// Set flag
					straight[col + x, row].W = true;
				}
				
				// Fill all nodes on the left from the primary jump point
				for (int x = 1; ; x++)
				{
					// Finish loop when getting out of map or hitting a wall
					if (col - x < 0 || Map.map[row, col - x] == OBSTACLE)
						break;
						
					// Create jump point if there wasn't one already
					if (straight[col - x, row] == null)
						straight[col - x, row] = new JumpPoint();
						
					// Set flag
					straight[col - x, row].E = true;
				}
			}
			
	}
	
	/// <summary>
	/// Calculates straight distances to jump points
	/// </summary>
	void CalculateStraightDistances()
	{
		// Create array with distances' informations
		distanceMap = new DistancenNode[Map.width, Map.height];
		
		// Iterate through map
		for (int row = 0; row < Map.height; row++)
			for (int col = 0; col < Map.width; col++)
			{
				// If node is obstacle, mark it as one
				if (Map.map[row, col] == OBSTACLE)
				{
					distanceMap[col, row] = new DistancenNode(row, col);
					distanceMap[col, row].free = false;		
					continue;
				}
				
				// Prepare variables that will store distances
				int n, e, s, w;
				
				// Prepare offset variable
				int x;
				
				// Check NORTH distance
				for (x = 1; ; x++)
				{
					// Check if node is outside of map
					if (row - x < 0)
					{
						// We are outside of map, save distance as negative
						n = -x + 1;
						break;
					
						// SENSITIVE POint, originally only break was here
					}
						
					// Check if node is a primary jump point that allows for movement in this direction
					if (primary[col, row - x] != null && primary[col, row - x].S)
					{
						// We hit primary jump point, save distance and exit loop
						n = x;
						break;
					}
					
					// Check if node is an obstacle
					if (Map.map[row - x, col] == OBSTACLE)
					{
						// We hit wall, save distance as negative
						n = -x + 1;
						break;
						
						// SENSITIVE POint, originally only break was here
					}
				}
				
				// Check SOUTH distance
				for (x = 1; ; x++)
				{
					// Check if node is outside of map
					if (row + x >= Map.height)
					{
						// We are outside of map, save distance as negative
						s = -x + 1;
						break;
					
						// SENSITIVE POint, originally only break was here
					}
						
					// Check if node is a primary jump point that allows for movement in this direction
					if (primary[col, row + x] != null && primary[col, row + x].N)
					{
						// We hit primary jump point, save distance and exit loop
						s = x;
						break;
					}
					
					// Check if node is an obstacle
					if (Map.map[row + x, col] == OBSTACLE)
					{
						// We hit wall, save distance as negative
						s = -x + 1;
						break;
						
						// SENSITIVE POint, originally only break was here
					}
				}
				
				// Check EAST distance
				for (x = 1; ; x++)
				{
					// Check if node is outside of map
					if (col + x >= Map.width)
					{
						// We are outside of map, save distance as negative
						e = -x + 1;
						break;
					
						// SENSITIVE POint, originally only break was here
					}
						
					// Check if node is a primary jump point that allows for movement in this direction
					if (primary[col + x, row] != null && primary[col + x, row].W)
					{
						// We hit primary jump point, save distance and exit loop
						e = x;
						break;
					}
					
					// Check if node is an obstacle
					if (Map.map[row, col + x] == OBSTACLE)
					{
						// We hit wall, save distance as negative
						e = -x + 1;
						break;
						
						// SENSITIVE POint, originally only break was here
					}
				}
				
				// Check WEST distance
				for (x = 1; ; x++)
				{
					// Check if node is outside of map
					if (col - x < 0)
					{
						// We are outside of map, save distance as negative
						w = -x + 1;
						break;
					
						// SENSITIVE POint, originally only break was here
					}
						
					// Check if node is a primary jump point that allows for movement in this direction
					if (primary[col - x, row] != null && primary[col - x, row].E)
					{
						// We hit primary jump point, save distance and exit loop
						w = x;
						break;
					}
					
					// Check if node is an obstacle
					if (Map.map[row, col - x] == OBSTACLE)
					{
						// We hit wall, save distance as negative
						w = -x + 1;
						break;
						
						// SENSITIVE POint, originally only break was here
					}
				}
				
				// Save distances
				distanceMap[col, row] = new DistancenNode(row, col, n, e, s, w);
			}
		
	}
	
	/// <summary>
	/// Generates diagonal jump points' map
	/// </summary>
	void GetDiagonalJumpPoints()
	{
		// Iterate through map
		for (int row = 0; row < Map.height; row++)
			for (int col = 0; col < Map.width; col++)
			{
				// Skip obstacles
				if (Map.map[row, col] == OBSTACLE)
					continue;
					
				// Skip nodes that are not jump points
				if (primary[col, row] == null && straight[col, row] == null)
					continue;
					
				// Save references for jump points in this node
				JumpPoint prim = primary[col, row];
				JumpPoint str = straight[col, row];
					
				// Try to fill top-left neighbours
				if ((prim != null && (prim.S || prim.W)) ||
					(str != null && (str.S || str.W)))
				{
					// Node is diagonal jump point, fill distances
					for (int x = 1; ; x++)
					{
						// Check if we got outside of map or hit a wall
						if (row - x < 0 || col + x >= Map.width ||
							Map.map[row - x + 1, col + x] == OBSTACLE ||
							Map.map[row - x, col + x - 1] == OBSTACLE ||
							Map.map[row - x, col + x] == OBSTACLE)
							break;
							
						// Create jump point if there wasn't one already
						if (diagonal[col + x, row - x] == null)
							diagonal[col + x, row - x] = new JumpPoint();
						
						// Set flag and save distance
						diagonal[col + x, row - x].SW = true;
						distanceMap[col + x, row - x].SW = x;
					}
				}
				
				// Try to fill bottom-right neighbours
				if ((prim != null && (prim.N || prim.W)) ||
					(str != null && (str.N || str.W)))
				{
					// Node is diagonal jump point, fill distances
					for (int x = 1; ; x++)
					{
						// Check if we got outisde of map or hit a wall
						if (row + x >= Map.height || col + x >= Map.width ||
							Map.map[row + x - 1, col + x] == OBSTACLE ||
							Map.map[row + x, col + x - 1] == OBSTACLE ||
							Map.map[row + x, col + x] == OBSTACLE)
							break;
							
						// Create diagonal jump point if there wasn't one aready
						if (diagonal[col + x, row + x] == null)
							diagonal[col + x, row + x] = new JumpPoint();
							
						// Set flag and save distance
						diagonal[col + x, row + x].NW = true;
						distanceMap[col + x, row + x].NW = x;
					}
				}
				
				// Try to fill bottom-left neighbours
				if ((prim != null && (prim.N || prim.E)) ||
					(str != null && (str.N || str.E)))
				{
					// Node is diagonal jump point, fill distances
					for (int x = 1; ; x++)
					{
						// Check if we got outside of map or hit a wall
						if (row + x >= Map.height || col - x < 0 ||
							Map.map[row + x - 1, col - x] == OBSTACLE ||
							Map.map[row + x, col - x + 1] == OBSTACLE ||
							Map.map[row + x, col - x] == OBSTACLE)
							break;
							
						// Create diagonal jump point if there wans't one already
						if (diagonal[col - x, row + x] == null)
							diagonal[col - x, row + x] = new JumpPoint();
							
						// Set flag and save distance
						diagonal[col - x, row + x].NE = true;
						distanceMap[col - x, row + x].NE = x;
					}
				}
				
				// Try to fill top-left neighbours
				if ((prim != null && (prim.S || prim.E)) ||
					(str != null && (str.S || str.E)))
				{
					// Node is diagonal jump point, fill distances
					for (int x = 1; ; x++)
					{
						// Check if we got outside of mp or hit a wall
						if (row - x < 0 || col - x < 0 ||
							Map.map[row - x + 1, col - x] == OBSTACLE ||
							Map.map[row - x, col - x + 1] == OBSTACLE ||
							Map.map[row - x, col - x] == OBSTACLE)
							break;
							
						// Create diagonal jump point if there wasn't one already
						if (diagonal[col - x, row - x] == null)
							diagonal[col - x, row - x] = new JumpPoint();
							
						// Set flag and save distance
						diagonal[col - x, row - x].SE = true;
						distanceMap[col - x, row - x].SE = x;
					}
				}
			}
	}
	
	/// <summary>
	/// Calculates diagonal distances to walls
	/// </summary>
	void CalculateDiagonalDistances()
	{
		// Iterate through map
		for (int row = 0; row < Map.height; row++)
			for (int col = 0; col < Map.width; col++)
			{
				// Check top-right nodes
				if (distanceMap[col, row].NE.HasValue == false)
				{
					// Caulculate distance
					for (int x = 1; ; x++)
					{
						// Check if we got outside of map or hit a wall
						if (row - x < 0 || col + x >= Map.width ||
							Map.map[row - x + 1, col + x] == OBSTACLE ||
							Map.map[row - x, col + x - 1] == OBSTACLE ||
							Map.map[row - x, col + x] == OBSTACLE)
						{
							distanceMap[col, row].NE = -x + 1;
							break;
						}
						
						// Check if we met another node that we can use to calculate distance
						if (distanceMap[col + x, row - x].NE.HasValue)
						{
							distanceMap[col, row].NE = distanceMap[col + x, row - x].NE.Value - x;
							break;
						}
					}
				}
				
				// Check bottom-right nodes
				if (distanceMap[col, row].SE.HasValue == false)
				{
					// Calculate distance
					for (int x = 1; ; x++)
					{
						// Check if we got outside of map or hit a wall
						if (row + x >= Map.height || col + x >= Map.width ||
							Map.map[row + x - 1, col + x] == OBSTACLE ||
							Map.map[row + x, col + x - 1] == OBSTACLE ||
							Map.map[row + x, col + x] == OBSTACLE)
						{
							distanceMap[col, row].SE = -x + 1;
							break;
						}
						
						// Check if we met another ndde that we can use to calculate distance
						if (distanceMap[col + x, row + x].SE.HasValue)
						{
							distanceMap[col, row].SE = distanceMap[col + x, row + x].SE.Value - x;
							break;
						}
					}
				}
				
				// Check bottom-left nodes
				if (distanceMap[col, row].SW.HasValue == false)
				{
					// Calculate distance
					for (int x = 1; ; x++)
					{
						// Check if we got outisde of map or hit a wall
						if (row + x >= Map.height || col - x < 0 ||
							Map.map[row + x - 1, col - x] == OBSTACLE ||
							Map.map[row + x, col - x + 1] == OBSTACLE ||
							Map.map[row + x, col - x] == OBSTACLE)
						{
							distanceMap[col, row].SW = -x + 1;
							break;
						}
						
						// Check if we met another node that we can use to calculate distance
						if (distanceMap[col - x, row + x].SW.HasValue)
						{
							distanceMap[col, row].SW = distanceMap[col - x, row + x].SW.Value - x;
							break;
						}
					}
				}
				
				// Check top-left nodes
				if (distanceMap[col, row].NW.HasValue == false)
				{
					// Calculate distance
					for (int x = 1; ; x++)
					{
						// Check if we got outside of map or hit a wall
						if (row - x < 0 || col - x < 0 ||
							Map.map[row - x + 1, col - x] == OBSTACLE ||
							Map.map[row - x, col - x + 1] == OBSTACLE ||
							Map.map[row - x, col - x] ==  OBSTACLE)
						{
							distanceMap[col, row].NW = -x + 1;
							break;
						}
						
						// Check if we met another node that we can use to calculate distance
						if (distanceMap[col - x, row - x].NW.HasValue)
						{
							distanceMap[col, row].NW = distanceMap[col - x, row - x].NW.Value - x;
							break;
						}
					}
				}
			}
	}
	
	/// <summary>
	/// Exports map to .txt file
	/// </summary>
	IEnumerator PrintMapToFile()
	{
		// Prepare output array
		string[,] output = new string[Map.width, Map.height * 9];
		
		// Prepare progress values
		int dataProgress = 0;
		int maxDataProgress = Map.height * Map.width;
		int fileProgress = 0;
		int maxFileProgress = Map.height * 9 * Map.width;
		int lastPercentage = 0;
		
		// Iterate through map
		for (int row = 0; row < Map.height; row++)
			for (int col = 0; col < Map.width; col++)
			{
				// Get node as string array
				string[] nodeStr = distanceMap[col, row].ToStringArray(col, row, primary[col, row] != null, straight[col, row] != null, diagonal[col, row] != null);
				
				// Use this array to fill result array
				for (int i = 0; i < 9; i++)
					output[col, (row * 9) + i] = nodeStr[i];
					
				dataProgress++;
				int percentage = 100 * dataProgress / maxDataProgress;
				if (percentage != lastPercentage)
				{
					ClearLog();
					Debug.Log("Data progress: " + percentage + "%");
					lastPercentage = percentage;
					yield return null;
				}
			}
			
		// Convert array to a single string
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		for (int row = 0; row < Map.height * 9; row++)
		{
			for (int col = 0; col < Map.width; col++)
			{
				sb.Append(output[col, row]);
				
				fileProgress++;
				int percentage = 100 * fileProgress / maxFileProgress;
				if (percentage != lastPercentage)
				{
					ClearLog();
					Debug.Log("File progress: " + percentage + "%");
					lastPercentage = percentage;
					yield return null;
				}
			}
			sb.AppendLine();
		}
		
		// Save data to file
		System.IO.File.WriteAllText("JpsMap.txt", sb.ToString());
		
		// Open new,y generated file
		System.Diagnostics.Process.Start("JpsMap.txt");
	}
	
	public void ClearLog()
	{
		var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
		var type = assembly.GetType("UnityEditor.LogEntries");
		var method = type.GetMethod("Clear");
		method.Invoke(new object(), null);
	}
}
