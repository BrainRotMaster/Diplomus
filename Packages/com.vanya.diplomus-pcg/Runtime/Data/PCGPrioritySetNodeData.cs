using PCG;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Priority Set Node", menuName = "PCG/Nodes/Priority Set")]
public class PCGPrioritySetNodeData : PCGNodeData
{
    [SerializeField] private int priorityValue;

    public int PriorityValue
    {
        get => priorityValue;
        set => priorityValue = value;
    }

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>
        {
            new PCGNodeParameter("Priority", PCGParameterType.Int, priorityValue)
        };
    }

    public override void UpdateParameter(string name, object value)
    {
        if (name == "Priority")
        {
            priorityValue = (int)value;
        }
    }

    public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        if (inputPoints == null || inputPoints.Count == 0)
        {
            return inputPoints ?? new List<PCGPoint>();
        }

        var modifiedPoints = new List<PCGPoint>(inputPoints.Count);
        foreach (var point in inputPoints)
        {
            var modifiedPoint = point;
            modifiedPoint.priority = priorityValue;
            modifiedPoints.Add(modifiedPoint);
        }

        return modifiedPoints;
    }

    public override string GetViewTypeName() => "PCGPrioritySetNodeView";
}
