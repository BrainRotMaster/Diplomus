using PCG;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Project To Surface Node", menuName = "PCG/Nodes/Project To Surface")]
public class PCGProjectToSurfaceNodeData : PCGNodeData
{
    [SerializeField] private float rayStartOffset = 100f;
    [SerializeField] private float maxDistance = 200f;
    [SerializeField] private float maxSurfaceAngle = 45f;
    [SerializeField] private bool alignToSurfaceNormal = true;
    [SerializeField] private bool discardMisses;
    [SerializeField] private int layerMask = ~0;

    public float RayStartOffset
    {
        get => rayStartOffset;
        set => rayStartOffset = value;
    }

    public float MaxDistance
    {
        get => maxDistance;
        set => maxDistance = value;
    }

    public float MaxSurfaceAngle
    {
        get => maxSurfaceAngle;
        set => maxSurfaceAngle = value;
    }

    public bool AlignToSurfaceNormal
    {
        get => alignToSurfaceNormal;
        set => alignToSurfaceNormal = value;
    }

    public bool DiscardMisses
    {
        get => discardMisses;
        set => discardMisses = value;
    }

    public int LayerMaskValue
    {
        get => layerMask;
        set => layerMask = value;
    }

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>
        {
            new PCGNodeParameter("Ray Start Offset", PCGParameterType.Float, rayStartOffset)
            {
                minValue = 0f, maxValue = 10000f
            },
            new PCGNodeParameter("Max Distance", PCGParameterType.Float, maxDistance)
            {
                minValue = 0.01f, maxValue = 10000f
            },
            new PCGNodeParameter("Max Surface Angle", PCGParameterType.Float, maxSurfaceAngle)
            {
                minValue = 0f, maxValue = 180f
            },
            new PCGNodeParameter("Align To Normal", PCGParameterType.Bool, alignToSurfaceNormal),
            new PCGNodeParameter("Discard Misses", PCGParameterType.Bool, discardMisses),
            new PCGNodeParameter("Layer Mask", PCGParameterType.Int, layerMask)
        };
    }

    public override void UpdateParameter(string name, object value)
    {
        switch (name)
        {
            case "Ray Start Offset": rayStartOffset = (float)value; break;
            case "Max Distance": maxDistance = (float)value; break;
            case "Max Surface Angle": maxSurfaceAngle = (float)value; break;
            case "Align To Normal": alignToSurfaceNormal = (bool)value; break;
            case "Discard Misses": discardMisses = (bool)value; break;
            case "Layer Mask": layerMask = (int)value; break;
        }
    }

    public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        var output = new List<PCGPoint>();
        if (inputPoints == null || inputPoints.Count == 0)
        {
            return inputPoints ?? output;
        }

        float clampedStartOffset = Mathf.Max(0f, rayStartOffset);
        float clampedMaxDistance = Mathf.Max(0.01f, maxDistance);
        float clampedMaxSurfaceAngle = Mathf.Clamp(maxSurfaceAngle, 0f, 180f);
        Vector3 upDirection = context.generatorTransform != null ? context.generatorTransform.up : Vector3.up;
        Vector3 castDirection = -upDirection.normalized;

        foreach (var point in inputPoints)
        {
            Vector3 rayOrigin = point.position + upDirection * clampedStartOffset;

            if (Physics.Raycast(rayOrigin, castDirection, out var hit, clampedMaxDistance, layerMask))
            {
                float surfaceAngle = Vector3.Angle(hit.normal, upDirection);
                if (surfaceAngle > clampedMaxSurfaceAngle)
                {
                    if (!discardMisses)
                    {
                        output.Add(point);
                    }

                    continue;
                }

                var projectedPoint = point;
                projectedPoint.position = hit.point;

                if (alignToSurfaceNormal)
                {
                    Vector3 pointUp = point.rotation * Vector3.up;
                    Quaternion surfaceRotation = Quaternion.FromToRotation(pointUp, hit.normal);
                    projectedPoint.rotation = surfaceRotation * point.rotation;
                }

                output.Add(projectedPoint);
                continue;
            }

            if (!discardMisses)
            {
                output.Add(point);
            }
        }

        if (discardMisses)
        {
            context.pointsFiltered += inputPoints.Count - output.Count;
        }

        return output;
    }

    public override string GetViewTypeName() => "PCGProjectToSurfaceNodeView";
}
