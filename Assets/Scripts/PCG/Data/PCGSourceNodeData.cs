using PCG;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Source Node", menuName = "PCG/Nodes/Source")]
public class PCGSourceNodeData : PCGNodeData
{
    public enum SourceType { Grid, Random }

    [SerializeField] private SourceType sourceType = SourceType.Grid;
    [SerializeField] private float spacing = 1f;
    [SerializeField] private int randomPointCount = 100;

    public SourceType SourceTypeValue { get => sourceType; set => sourceType = value; }
    public float Spacing { get => spacing; set => spacing = value; }
    public int RandomPointCount { get => randomPointCount; set => randomPointCount = value; }

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>
        {
            PCGNodeParameter.CreateEnum("Source Type", sourceType),
            new PCGNodeParameter("Spacing", PCGParameterType.Float, spacing)
            {
                minValue = 0.1f, maxValue = 100f
            },
            new PCGNodeParameter("Point Count", PCGParameterType.Int, randomPointCount)
            {
                minValue = 1, maxValue = 10000
            }
        };
    }

    public override void UpdateParameter(string name, object value)
    {
        switch (name)
        {
            case "Source Type": sourceType = (SourceType)(int)value; break;
            case "Spacing": spacing = (float)value; break;
            case "Point Count": randomPointCount = (int)value; break;
        }
    }

    public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        var points = new List<PCGPoint>();
        var bounds = context.generationBounds;

        switch (sourceType)
        {
            case SourceType.Grid:
                float clampedSpacing = Mathf.Max(0.01f, spacing);
                int xCount = Mathf.Max(1, Mathf.FloorToInt(bounds.size.x / clampedSpacing) + 1);
                int zCount = Mathf.Max(1, Mathf.FloorToInt(bounds.size.z / clampedSpacing) + 1);

                for (int x = 0; x < xCount; x++)
                {
                    for (int z = 0; z < zCount; z++)
                    {
                        var localPos = new Vector3(
                            bounds.min.x + x * clampedSpacing,
                            bounds.center.y,
                            bounds.min.z + z * clampedSpacing
                        );

                        points.Add(new PCGPoint(TransformPoint(context, localPos)));
                    }
                }
                break;

            case SourceType.Random:
                for (int i = 0; i < randomPointCount; i++)
                {
                    var localPos = new Vector3(
                        context.GetRandomFloat(bounds.min.x, bounds.max.x),
                        bounds.center.y,
                        context.GetRandomFloat(bounds.min.z, bounds.max.z)
                    );

                    points.Add(new PCGPoint(TransformPoint(context, localPos)));
                }
                break;
        }

        context.pointsGenerated += points.Count;
        return points;
    }

    public override string GetViewTypeName() => "PCGSourceNodeView";

    private static Vector3 TransformPoint(PCGExecutionContext context, Vector3 localPoint)
    {
        if (context.generatorTransform == null)
        {
            return localPoint;
        }

        return context.generatorTransform.TransformPoint(localPoint);
    }
}
