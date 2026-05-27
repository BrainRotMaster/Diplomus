using PCG;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Split By Bounds Node", menuName = "PCG/Nodes/Split By Bounds")]
public class PCGSplitByBoundsNodeData : PCGNodeData
{
    [SerializeField] private string regionId;
    [SerializeField] private string regionName;

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

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>();
    }

    public override void UpdateParameter(string name, object value)
    {
    }

    public override IEnumerable<string> GetOutputPortNames()
    {
        yield return "Inside";
        yield return "Outside";
    }

    public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        return inputPoints ?? new List<PCGPoint>();
    }

    public override PCGNodeOutput ProcessMulti(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        var output = new PCGNodeOutput();
        var insidePoints = new List<PCGPoint>();
        var outsidePoints = new List<PCGPoint>();

        if (inputPoints == null || inputPoints.Count == 0)
        {
            output.SetStream("Inside", insidePoints);
            output.SetStream("Outside", outsidePoints);
            return output;
        }

        var region = PCGRegionBase.FindById(regionId);
        if (region == null)
        {
            Debug.LogWarning($"Split By Bounds '{nodeName}' skipped because region '{regionName}' is missing.");
            output.SetStream("Inside", insidePoints);
            output.SetStream("Outside", new List<PCGPoint>(inputPoints));
            return output;
        }

        foreach (var point in inputPoints)
        {
            if (region.Contains(point.position))
            {
                insidePoints.Add(point);
            }
            else
            {
                outsidePoints.Add(point);
            }
        }

        output.SetStream("Inside", insidePoints);
        output.SetStream("Outside", outsidePoints);
        return output;
    }

    public override string GetViewTypeName() => "PCGSplitByBoundsNodeView";
}
