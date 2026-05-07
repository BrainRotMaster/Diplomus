using PCG;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Random Scale Node", menuName = "PCG/Nodes/Random Scale")]
public class PCGRandomScaleNodeData : PCGNodeData
{
    public enum ScaleMode
    {
        Uniform,
        PerAxis
    }

    [SerializeField] private ScaleMode scaleMode = ScaleMode.Uniform;
    [SerializeField] private float uniformMin = 1f;
    [SerializeField] private float uniformMax = 1f;
    [SerializeField] private float minScaleX = 1f;
    [SerializeField] private float maxScaleX = 1f;
    [SerializeField] private float minScaleY = 1f;
    [SerializeField] private float maxScaleY = 1f;
    [SerializeField] private float minScaleZ = 1f;
    [SerializeField] private float maxScaleZ = 1f;

    public ScaleMode Mode
    {
        get => scaleMode;
        set => scaleMode = value;
    }

    public float UniformMin
    {
        get => uniformMin;
        set => uniformMin = value;
    }

    public float UniformMax
    {
        get => uniformMax;
        set => uniformMax = value;
    }

    public float MinScaleX
    {
        get => minScaleX;
        set => minScaleX = value;
    }

    public float MaxScaleX
    {
        get => maxScaleX;
        set => maxScaleX = value;
    }

    public float MinScaleY
    {
        get => minScaleY;
        set => minScaleY = value;
    }

    public float MaxScaleY
    {
        get => maxScaleY;
        set => maxScaleY = value;
    }

    public float MinScaleZ
    {
        get => minScaleZ;
        set => minScaleZ = value;
    }

    public float MaxScaleZ
    {
        get => maxScaleZ;
        set => maxScaleZ = value;
    }

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>
        {
            PCGNodeParameter.CreateEnum("Scale Mode", scaleMode),
            new PCGNodeParameter("Uniform Min", PCGParameterType.Float, uniformMin)
            {
                minValue = 0.01f, maxValue = 100f
            },
            new PCGNodeParameter("Uniform Max", PCGParameterType.Float, uniformMax)
            {
                minValue = 0.01f, maxValue = 100f
            },
            new PCGNodeParameter("Min Scale X", PCGParameterType.Float, minScaleX)
            {
                minValue = 0.01f, maxValue = 100f
            },
            new PCGNodeParameter("Max Scale X", PCGParameterType.Float, maxScaleX)
            {
                minValue = 0.01f, maxValue = 100f
            },
            new PCGNodeParameter("Min Scale Y", PCGParameterType.Float, minScaleY)
            {
                minValue = 0.01f, maxValue = 100f
            },
            new PCGNodeParameter("Max Scale Y", PCGParameterType.Float, maxScaleY)
            {
                minValue = 0.01f, maxValue = 100f
            },
            new PCGNodeParameter("Min Scale Z", PCGParameterType.Float, minScaleZ)
            {
                minValue = 0.01f, maxValue = 100f
            },
            new PCGNodeParameter("Max Scale Z", PCGParameterType.Float, maxScaleZ)
            {
                minValue = 0.01f, maxValue = 100f
            }
        };
    }

    public override void UpdateParameter(string name, object value)
    {
        switch (name)
        {
            case "Scale Mode": scaleMode = (ScaleMode)(int)value; break;
            case "Uniform Min": uniformMin = (float)value; break;
            case "Uniform Max": uniformMax = (float)value; break;
            case "Min Scale X": minScaleX = (float)value; break;
            case "Max Scale X": maxScaleX = (float)value; break;
            case "Min Scale Y": minScaleY = (float)value; break;
            case "Max Scale Y": maxScaleY = (float)value; break;
            case "Min Scale Z": minScaleZ = (float)value; break;
            case "Max Scale Z": maxScaleZ = (float)value; break;
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
            var scaledPoint = point;

            if (scaleMode == ScaleMode.Uniform)
            {
                float uniformValue = GetRandomBetween(context, uniformMin, uniformMax);
                scaledPoint.scale = Vector3.Scale(point.scale, Vector3.one * uniformValue);
            }
            else
            {
                var randomScale = new Vector3(
                    GetRandomBetween(context, minScaleX, maxScaleX),
                    GetRandomBetween(context, minScaleY, maxScaleY),
                    GetRandomBetween(context, minScaleZ, maxScaleZ));
                scaledPoint.scale = Vector3.Scale(point.scale, randomScale);
            }

            output.Add(scaledPoint);
        }

        return output;
    }

    public override string GetViewTypeName() => "PCGRandomScaleNodeView";

    private static float GetRandomBetween(PCGExecutionContext context, float minValue, float maxValue)
    {
        minValue = Mathf.Max(0.01f, minValue);
        maxValue = Mathf.Max(0.01f, maxValue);

        if (minValue > maxValue)
        {
            (minValue, maxValue) = (maxValue, minValue);
        }

        return context.GetRandomFloat(minValue, maxValue);
    }
}
