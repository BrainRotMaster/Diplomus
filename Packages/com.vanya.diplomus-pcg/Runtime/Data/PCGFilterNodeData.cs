using PCG;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Filter Node", menuName = "PCG/Nodes/Filter")]
public class PCGFilterNodeData : PCGNodeData
{
    public enum FilterType { RandomChance, DensityThreshold, TagEquals }

    [SerializeField] private FilterType filterType = FilterType.RandomChance;
    [SerializeField] private float randomChance = 0.5f;
    [SerializeField] private float minDensity = 0.5f;
    [SerializeField] private int requiredTag;

    public FilterType FilterTypeValue { get => filterType; set => filterType = value; }
    public float RandomChance { get => randomChance; set => randomChance = value; }
    public float MinDensity { get => minDensity; set => minDensity = value; }
    public int RequiredTag { get => requiredTag; set => requiredTag = value; }

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>
        {
            PCGNodeParameter.CreateEnum("Filter Type", filterType),
            new PCGNodeParameter("Random Chance", PCGParameterType.Float, randomChance)
            {
                minValue = 0f, maxValue = 1f
            },
            new PCGNodeParameter("Min Density", PCGParameterType.Float, minDensity)
            {
                minValue = 0f, maxValue = 1f
            },
            new PCGNodeParameter("Tag", PCGParameterType.Int, requiredTag)
        };
    }

    public override void UpdateParameter(string name, object value)
    {
        switch (name)
        {
            case "Filter Type": filterType = (FilterType)(int)value; break;
            case "Random Chance": randomChance = (float)value; break;
            case "Min Density": minDensity = (float)value; break;
            case "Tag": requiredTag = (int)value; break;
        }
    }

    public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        if (inputPoints == null) return new List<PCGPoint>();

        var filtered = new List<PCGPoint>();

        foreach (var point in inputPoints)
        {
            bool shouldKeep = false;

            switch (filterType)
            {
                case FilterType.RandomChance:
                    shouldKeep = context.GetRandomFloat(0, 1) <= randomChance;
                    break;
                case FilterType.DensityThreshold:
                    shouldKeep = point.density >= minDensity;
                    break;
                case FilterType.TagEquals:
                    shouldKeep = point.tag == requiredTag;
                    break;
            }

            if (shouldKeep) filtered.Add(point);
        }

        context.pointsFiltered += inputPoints.Count - filtered.Count;
        return filtered;
    }

    public override string GetViewTypeName() => "PCGFilterNodeView";
}
