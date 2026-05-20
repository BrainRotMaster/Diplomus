using PCG;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bounds Filter Node", menuName = "PCG/Nodes/Bounds Filter")]
public class PCGBoundsFilterNodeData : PCGNodeData
{
    [SerializeField] private string regionId;
    [SerializeField] private string regionName;
    [SerializeField] private bool invert;

    public string RegionId
    {
        get => regionId;
        set => regionId = value;
    }

    public string RegionName
    {
        get => regionName;
        set => regionName = value;
    }

    public bool Invert
    {
        get => invert;
        set => invert = value;
    }

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>
        {
            new PCGNodeParameter("Invert", PCGParameterType.Bool, invert)
        };
    }

    public override void UpdateParameter(string name, object value)
    {
        if (name == "Invert")
        {
            invert = (bool)value;
        }
    }

    public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        if (inputPoints == null || inputPoints.Count == 0)
        {
            return inputPoints ?? new List<PCGPoint>();
        }

        var region = PCGBoxRegion.FindById(regionId);
        if (region == null)
        {
            Debug.LogWarning($"Bounds Filter '{nodeName}' skipped because region '{regionName}' is missing.");
            return inputPoints;
        }

        var filtered = new List<PCGPoint>();
        foreach (var point in inputPoints)
        {
            bool containsPoint = region.Contains(point.position);
            bool shouldKeep = invert ? !containsPoint : containsPoint;

            if (shouldKeep)
            {
                filtered.Add(point);
            }
        }

        context.pointsFiltered += inputPoints.Count - filtered.Count;
        return filtered;
    }

    public override string GetViewTypeName() => "PCGBoundsFilterNodeView";
}
