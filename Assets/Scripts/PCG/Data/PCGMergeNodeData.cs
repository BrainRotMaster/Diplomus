using PCG;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Merge Node", menuName = "PCG/Nodes/Merge")]
public class PCGMergeNodeData : PCGNodeData
{
    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>();
    }

    public override void UpdateParameter(string name, object value)
    {
    }

    public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        return inputPoints ?? new List<PCGPoint>();
    }

    public override string GetViewTypeName() => "PCGMergeNodeView";
}
