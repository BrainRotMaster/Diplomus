using PCG;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attribute Set Node", menuName = "PCG/Nodes/Attribute Set")]
public class PCGAttributeSetNodeData : PCGNodeData
{
    [SerializeField] private bool overrideDensity = true;
    [SerializeField] private float density = 1f;
    [SerializeField] private bool overrideTag;
    [SerializeField] private int tagValue;
    [SerializeField] private bool overrideUniformScale;
    [SerializeField] private float uniformScale = 1f;

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>
        {
            new PCGNodeParameter("Override Density", PCGParameterType.Bool, overrideDensity),
            new PCGNodeParameter("Density", PCGParameterType.Float, density)
            {
                minValue = 0f, maxValue = 1f
            },
            new PCGNodeParameter("Override Tag", PCGParameterType.Bool, overrideTag),
            new PCGNodeParameter("Tag", PCGParameterType.Int, tagValue),
            new PCGNodeParameter("Override Scale", PCGParameterType.Bool, overrideUniformScale),
            new PCGNodeParameter("Uniform Scale", PCGParameterType.Float, uniformScale)
            {
                minValue = 0.01f, maxValue = 100f
            }
        };
    }

    public override void UpdateParameter(string name, object value)
    {
        switch (name)
        {
            case "Override Density": overrideDensity = (bool)value; break;
            case "Density": density = (float)value; break;
            case "Override Tag": overrideTag = (bool)value; break;
            case "Tag": tagValue = (int)value; break;
            case "Override Scale": overrideUniformScale = (bool)value; break;
            case "Uniform Scale": uniformScale = (float)value; break;
        }
    }

    public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        var output = new List<PCGPoint>();
        if (inputPoints == null)
        {
            return output;
        }

        float clampedUniformScale = Mathf.Max(0.01f, uniformScale);

        foreach (var point in inputPoints)
        {
            var modifiedPoint = point;

            if (overrideDensity)
            {
                modifiedPoint.density = Mathf.Clamp01(density);
            }

            if (overrideTag)
            {
                modifiedPoint.tag = tagValue;
            }

            if (overrideUniformScale)
            {
                modifiedPoint.scale = Vector3.one * clampedUniformScale;
            }

            output.Add(modifiedPoint);
        }

        return output;
    }

    public override string GetViewTypeName() => "PCGAttributeSetNodeView";
}
