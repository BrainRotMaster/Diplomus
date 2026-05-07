using PCG;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Random Rotation Node", menuName = "PCG/Nodes/Random Rotation")]
public class PCGRandomRotationNodeData : PCGNodeData
{
    [SerializeField] private float minRotationX;
    [SerializeField] private float maxRotationX;
    [SerializeField] private float minRotationY;
    [SerializeField] private float maxRotationY = 360f;
    [SerializeField] private float minRotationZ;
    [SerializeField] private float maxRotationZ;

    public float MinRotationX
    {
        get => minRotationX;
        set => minRotationX = value;
    }

    public float MaxRotationX
    {
        get => maxRotationX;
        set => maxRotationX = value;
    }

    public float MinRotationY
    {
        get => minRotationY;
        set => minRotationY = value;
    }

    public float MaxRotationY
    {
        get => maxRotationY;
        set => maxRotationY = value;
    }

    public float MinRotationZ
    {
        get => minRotationZ;
        set => minRotationZ = value;
    }

    public float MaxRotationZ
    {
        get => maxRotationZ;
        set => maxRotationZ = value;
    }

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>
        {
            new PCGNodeParameter("Min Rotation X", PCGParameterType.Float, minRotationX),
            new PCGNodeParameter("Max Rotation X", PCGParameterType.Float, maxRotationX),
            new PCGNodeParameter("Min Rotation Y", PCGParameterType.Float, minRotationY),
            new PCGNodeParameter("Max Rotation Y", PCGParameterType.Float, maxRotationY),
            new PCGNodeParameter("Min Rotation Z", PCGParameterType.Float, minRotationZ),
            new PCGNodeParameter("Max Rotation Z", PCGParameterType.Float, maxRotationZ)
        };
    }

    public override void UpdateParameter(string name, object value)
    {
        switch (name)
        {
            case "Min Rotation X": minRotationX = (float)value; break;
            case "Max Rotation X": maxRotationX = (float)value; break;
            case "Min Rotation Y": minRotationY = (float)value; break;
            case "Max Rotation Y": maxRotationY = (float)value; break;
            case "Min Rotation Z": minRotationZ = (float)value; break;
            case "Max Rotation Z": maxRotationZ = (float)value; break;
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
            var rotatedPoint = point;
            var randomRotation = Quaternion.Euler(
                GetRandomBetween(context, minRotationX, maxRotationX),
                GetRandomBetween(context, minRotationY, maxRotationY),
                GetRandomBetween(context, minRotationZ, maxRotationZ));

            rotatedPoint.rotation = point.rotation * randomRotation;
            output.Add(rotatedPoint);
        }

        return output;
    }

    public override string GetViewTypeName() => "PCGRandomRotationNodeView";

    private static float GetRandomBetween(PCGExecutionContext context, float minValue, float maxValue)
    {
        if (minValue > maxValue)
        {
            (minValue, maxValue) = (maxValue, minValue);
        }

        return context.GetRandomFloat(minValue, maxValue);
    }
}
