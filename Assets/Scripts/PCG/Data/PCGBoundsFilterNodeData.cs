using PCG;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bounds Filter Node", menuName = "PCG/Nodes/Bounds Filter")]
public class PCGBoundsFilterNodeData : PCGNodeData
{
    [SerializeField] private float centerX;
    [SerializeField] private float centerY;
    [SerializeField] private float centerZ;
    [SerializeField] private float sizeX = 10f;
    [SerializeField] private float sizeY = 10f;
    [SerializeField] private float sizeZ = 10f;
    [SerializeField] private bool invert;

    public float CenterX
    {
        get => centerX;
        set => centerX = value;
    }

    public float CenterY
    {
        get => centerY;
        set => centerY = value;
    }

    public float CenterZ
    {
        get => centerZ;
        set => centerZ = value;
    }

    public float SizeX
    {
        get => sizeX;
        set => sizeX = value;
    }

    public float SizeY
    {
        get => sizeY;
        set => sizeY = value;
    }

    public float SizeZ
    {
        get => sizeZ;
        set => sizeZ = value;
    }

    public bool Invert
    {
        get => invert;
        set => invert = value;
    }

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>
        {
            new PCGNodeParameter("Center X", PCGParameterType.Float, centerX),
            new PCGNodeParameter("Center Y", PCGParameterType.Float, centerY),
            new PCGNodeParameter("Center Z", PCGParameterType.Float, centerZ),
            new PCGNodeParameter("Size X", PCGParameterType.Float, sizeX)
            {
                minValue = 0.01f, maxValue = 100000f
            },
            new PCGNodeParameter("Size Y", PCGParameterType.Float, sizeY)
            {
                minValue = 0.01f, maxValue = 100000f
            },
            new PCGNodeParameter("Size Z", PCGParameterType.Float, sizeZ)
            {
                minValue = 0.01f, maxValue = 100000f
            },
            new PCGNodeParameter("Invert", PCGParameterType.Bool, invert)
        };
    }

    public override void UpdateParameter(string name, object value)
    {
        switch (name)
        {
            case "Center X": centerX = (float)value; break;
            case "Center Y": centerY = (float)value; break;
            case "Center Z": centerZ = (float)value; break;
            case "Size X": sizeX = (float)value; break;
            case "Size Y": sizeY = (float)value; break;
            case "Size Z": sizeZ = (float)value; break;
            case "Invert": invert = (bool)value; break;
        }
    }

    public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        if (inputPoints == null || inputPoints.Count == 0)
        {
            return inputPoints ?? new List<PCGPoint>();
        }

        var filtered = new List<PCGPoint>();
        var bounds = new Bounds(
            new Vector3(centerX, centerY, centerZ),
            new Vector3(
                Mathf.Max(0.01f, sizeX),
                Mathf.Max(0.01f, sizeY),
                Mathf.Max(0.01f, sizeZ)));

        foreach (var point in inputPoints)
        {
            bool containsPoint = bounds.Contains(point.position);
            bool shouldKeep = invert ? !containsPoint : containsPoint;

            if (shouldKeep)
            {
                filtered.Add(point);
            }
        }

        context.pointsFiltered += inputPoints.Count - filtered.Count;
        return filtered;
    }

    public override string GetViewTypeName() => "PCGBoundsFilterNodeView";
}
