using PCG;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Transform Node", menuName = "PCG/Nodes/Transform")]
public class PCGTransformNodeData : PCGNodeData
{
    [SerializeField] private float offsetX;
    [SerializeField] private float offsetY;
    [SerializeField] private float offsetZ;
    [SerializeField] private float rotationX;
    [SerializeField] private float rotationY;
    [SerializeField] private float rotationZ;
    [SerializeField] private float scaleMultiplier = 1f;

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>
        {
            new PCGNodeParameter("Offset X", PCGParameterType.Float, offsetX),
            new PCGNodeParameter("Offset Y", PCGParameterType.Float, offsetY),
            new PCGNodeParameter("Offset Z", PCGParameterType.Float, offsetZ),
            new PCGNodeParameter("Rotation X", PCGParameterType.Float, rotationX),
            new PCGNodeParameter("Rotation Y", PCGParameterType.Float, rotationY),
            new PCGNodeParameter("Rotation Z", PCGParameterType.Float, rotationZ),
            new PCGNodeParameter("Scale Multiplier", PCGParameterType.Float, scaleMultiplier)
            {
                minValue = 0.01f, maxValue = 100f
            }
        };
    }

    public override void UpdateParameter(string name, object value)
    {
        switch (name)
        {
            case "Offset X": offsetX = (float)value; break;
            case "Offset Y": offsetY = (float)value; break;
            case "Offset Z": offsetZ = (float)value; break;
            case "Rotation X": rotationX = (float)value; break;
            case "Rotation Y": rotationY = (float)value; break;
            case "Rotation Z": rotationZ = (float)value; break;
            case "Scale Multiplier": scaleMultiplier = (float)value; break;
        }
    }

    public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        var output = new List<PCGPoint>();
        if (inputPoints == null)
        {
            return output;
        }

        var offset = new Vector3(offsetX, offsetY, offsetZ);
        var rotationOffset = Quaternion.Euler(rotationX, rotationY, rotationZ);
        float clampedScaleMultiplier = Mathf.Max(0.01f, scaleMultiplier);

        foreach (var point in inputPoints)
        {
            var transformedPoint = point;
            transformedPoint.position += offset;
            transformedPoint.rotation = point.rotation * rotationOffset;
            transformedPoint.scale = point.scale * clampedScaleMultiplier;
            output.Add(transformedPoint);
        }

        return output;
    }

    public override string GetViewTypeName() => "PCGTransformNodeView";
}
