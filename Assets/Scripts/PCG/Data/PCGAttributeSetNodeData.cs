using PCG;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tag Set Node", menuName = "PCG/Nodes/Tag Set")]
public class PCGAttributeSetNodeData : PCGNodeData
{
    [SerializeField] private int tagValue;

    public int TagValue
    {
        get => tagValue;
        set => tagValue = value;
    }

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>
        {
            new PCGNodeParameter("Tag", PCGParameterType.Int, tagValue)
        };
    }

    public override void UpdateParameter(string name, object value)
    {
        switch (name)
        {
            case "Tag": tagValue = (int)value; break;
        }
    }

    public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        var output = new List<PCGPoint>();
        if (inputPoints == null)
        {
            return output;
        }

        foreach (var point in inputPoints)
        {
            var modifiedPoint = point;
            modifiedPoint.tag = tagValue;
            output.Add(modifiedPoint);
        }

        return output;
    }

    public override string GetViewTypeName() => "PCGAttributeSetNodeView";
}
