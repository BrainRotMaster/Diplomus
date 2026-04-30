using PCG;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Density Noise Node", menuName = "PCG/Nodes/Density Noise")]
public class PCGDensityNoiseNodeData : PCGNodeData
{
    [SerializeField] private float noiseScale = 0.1f;
    [SerializeField] private float densityMultiplier = 1f;
    [SerializeField] private float offsetX;
    [SerializeField] private float offsetZ;

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>
        {
            new PCGNodeParameter("Noise Scale", PCGParameterType.Float, noiseScale)
            {
                minValue = 0.001f, maxValue = 10f
            },
            new PCGNodeParameter("Density Multiplier", PCGParameterType.Float, densityMultiplier)
            {
                minValue = 0f, maxValue = 10f
            },
            new PCGNodeParameter("Offset X", PCGParameterType.Float, offsetX),
            new PCGNodeParameter("Offset Z", PCGParameterType.Float, offsetZ)
        };
    }

    public override void UpdateParameter(string name, object value)
    {
        switch (name)
        {
            case "Noise Scale": noiseScale = (float)value; break;
            case "Density Multiplier": densityMultiplier = (float)value; break;
            case "Offset X": offsetX = (float)value; break;
            case "Offset Z": offsetZ = (float)value; break;
        }
    }

    public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        var output = new List<PCGPoint>();
        if (inputPoints == null)
        {
            return output;
        }

        float clampedNoiseScale = Mathf.Max(0.001f, noiseScale);
        float clampedDensityMultiplier = Mathf.Max(0f, densityMultiplier);

        foreach (var point in inputPoints)
        {
            var modifiedPoint = point;
            float noiseValue = Mathf.PerlinNoise(
                point.position.x * clampedNoiseScale + offsetX,
                point.position.z * clampedNoiseScale + offsetZ);

            modifiedPoint.density = Mathf.Clamp01(point.density * noiseValue * clampedDensityMultiplier);
            output.Add(modifiedPoint);
        }

        return output;
    }

    public override string GetViewTypeName() => "PCGDensityNoiseNodeView";
}
