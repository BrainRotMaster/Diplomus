using PCG;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Distance Filter Node", menuName = "PCG/Nodes/Distance Filter")]
public class PCGDistanceFilterNodeData : PCGNodeData
{
    [SerializeField] private float minimumDistance = 1f;

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>
        {
            new PCGNodeParameter("Minimum Distance", PCGParameterType.Float, minimumDistance)
            {
                minValue = 0f, maxValue = 100f
            }
        };
    }

    public override void UpdateParameter(string name, object value)
    {
        if (name == "Minimum Distance")
        {
            minimumDistance = (float)value;
        }
    }

    public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        if (inputPoints == null || inputPoints.Count == 0)
        {
            return inputPoints ?? new List<PCGPoint>();
        }

        float minDistanceSqr = minimumDistance * minimumDistance;
        var filtered = new List<PCGPoint>();

        foreach (var point in inputPoints)
        {
            bool isTooClose = false;
            foreach (var existingPoint in filtered)
            {
                if ((existingPoint.position - point.position).sqrMagnitude < minDistanceSqr)
                {
                    isTooClose = true;
                    break;
                }
            }

            if (!isTooClose)
            {
                filtered.Add(point);
            }
        }

        context.pointsFiltered += inputPoints.Count - filtered.Count;
        return filtered;
    }

    public override string GetViewTypeName() => "PCGDistanceFilterNodeView";
}
