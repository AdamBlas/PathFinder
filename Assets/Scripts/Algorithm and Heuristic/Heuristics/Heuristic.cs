using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Heuristic
{
	[Tooltip("Name of the heuristic")]
	public string name;
	
	[Tooltip("Description of the heuristic")]
	public string description;
	
	[Tooltip("Lookup table with sine values for angles")]
	public static readonly float[] sines = { 0f, 0.008726535f, 0.01745241f, 0.02617695f, 0.0348995f, 0.04361939f, 0.05233596f, 0.06104854f, 0.06975647f, 0.0784591f, 0.08715574f, 0.09584575f, 0.1045285f, 0.1132032f, 0.1218693f, 0.1305262f, 0.1391731f, 0.1478094f, 0.1564345f, 0.1650476f, 0.1736482f, 0.1822355f, 0.190809f, 0.1993679f, 0.2079117f, 0.2164396f, 0.224951f, 0.2334454f, 0.2419219f, 0.25038f, 0.258819f, 0.2672384f, 0.2756374f, 0.2840154f, 0.2923717f, 0.3007058f, 0.309017f, 0.3173046f, 0.3255681f, 0.3338069f, 0.3420201f, 0.3502074f, 0.3583679f, 0.3665012f, 0.3746066f, 0.3826835f, 0.3907311f, 0.3987491f, 0.4067366f, 0.4146932f, 0.4226183f, 0.4305111f, 0.4383712f, 0.4461978f, 0.4539905f, 0.4617486f, 0.4694716f, 0.4771588f, 0.4848096f, 0.4924236f, 0.5f, 0.5075384f, 0.5150381f, 0.5224985f, 0.5299193f, 0.5372996f, 0.5446391f, 0.551937f, 0.5591929f, 0.5664063f, 0.5735765f, 0.580703f, 0.5877852f, 0.5948228f, 0.601815f, 0.6087614f, 0.6156615f, 0.6225147f, 0.6293204f, 0.6360782f, 0.6427876f, 0.649448f, 0.656059f, 0.6626201f, 0.6691306f, 0.6755902f, 0.6819984f, 0.6883546f, 0.6946584f, 0.7009093f, 0.7071068f, 0.7132505f, 0.7193398f, 0.7253743f, 0.7313537f, 0.7372773f, 0.7431449f, 0.7489557f, 0.7547095f, 0.760406f, 0.7660444f, 0.7716246f, 0.7771459f, 0.7826082f, 0.7880107f, 0.7933533f, 0.7986355f, 0.8038568f, 0.809017f, 0.8141155f, 0.8191521f, 0.8241262f, 0.8290375f, 0.8338858f, 0.8386706f, 0.8433915f, 0.8480481f, 0.8526402f, 0.8571673f, 0.8616291f, 0.8660254f, 0.8703557f, 0.8746197f, 0.8788171f, 0.8829476f, 0.8870109f, 0.8910065f, 0.8949344f, 0.8987941f, 0.9025853f, 0.9063078f, 0.9099613f, 0.9135455f, 0.9170601f, 0.9205048f, 0.9238795f, 0.9271839f, 0.9304176f, 0.9335804f, 0.9366722f, 0.9396926f, 0.9426415f, 0.9455186f, 0.9483237f, 0.9510565f, 0.9537169f, 0.9563047f, 0.9588197f, 0.9612617f, 0.9636304f, 0.9659258f, 0.9681476f, 0.9702957f, 0.9723699f, 0.9743701f, 0.976296f, 0.9781476f, 0.9799247f, 0.9816272f, 0.9832549f, 0.9848077f, 0.9862856f, 0.9876884f, 0.9890159f, 0.9902681f, 0.9914448f, 0.9925461f, 0.9935719f, 0.9945219f, 0.9953962f, 0.9961947f, 0.9969173f, 0.9975641f, 0.9981348f, 0.9986295f, 0.9990482f, 0.9993908f, 0.9996573f, 0.9998477f, 0.9999619f, 1f };
	
	[Tooltip("Lookup table with sine values multiplied by goal bounding strength")]
	public static float[] goalBoundingSines = new float[181];
	
	
	
	
	
	/// <summary>
	/// Calculates node's cost
	/// </summary>
	/// <param name="node"> Node to calculate cost </param>
	public abstract void CalculateCost(Node node);
	
	/// <summary>
	/// Applies goal bounding to the node's cost
	/// </summary>
	/// <param name="node"> Node for which goal bounding should be applied </param>
	protected void ApplyGoalBounding(Node node)
	{
		// Apply goal bounging if neccesary
		if (GoalBoundingManager.shouldApply)
		{
			// Calculate angle to the target
			Vector2 fromStart = new Vector2(node.x - StartGoalManager.startCol, node.y - StartGoalManager.startRow);
			Vector2 toGoal = new Vector3(StartGoalManager.goalCol - node.parentNode.x, StartGoalManager.goalRow - node.parentNode.y);
			float angle = Vector2.Angle(fromStart, toGoal);
			
			// Increase cost based on precalculated goal bounding value
			node.goalBoundCost = node.baseCost + node.baseCost * goalBoundingSines[(int)angle];
		}
		else
		{
			// No goal bounding should be applied, just copy value
			node.goalBoundCost = node.baseCost;
		}
	}
}
