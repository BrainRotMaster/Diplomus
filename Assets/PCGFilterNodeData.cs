using System.Collections.Generic;
using UnityEngine;

namespace PCG
{

    [CreateAssetMenu(fileName = "Filter Node", menuName = "PCG/Nodes/Filter")]
    public class PCGFilterNodeData : PCGNodeData
    {
        public enum FilterType
        {
            DensityThreshold,
            RandomChance,
            TagMask,
            DistanceFromCenter
        }

        public FilterType filterType = FilterType.DensityThreshold;
        public float minDensity = 0.5f;
        public float randomChance = 0.5f;
        public int requiredTag = 1;
        public float minDistance = 0f;
        public float maxDistance = 100f;

        public override List<PCGNodeParameter> GetParameters()
        {
            return new List<PCGNodeParameter>
        {
            new PCGNodeParameter { name = "Filter Type", type = PCGParameterType.Dropdown,
                value = (int)filterType, options = new[] { "Density Threshold", "Random Chance", "Tag Mask", "Distance From Center" } },
            new PCGNodeParameter { name = "Min Density", type = PCGParameterType.Float, value = minDensity, minValue = 0f, maxValue = 1f },
            new PCGNodeParameter { name = "Random Chance", type = PCGParameterType.Float, value = randomChance, minValue = 0f, maxValue = 1f },
            new PCGNodeParameter { name = "Required Tag", type = PCGParameterType.Int, value = requiredTag, minValue = 0, maxValue = 31 },
            new PCGNodeParameter { name = "Min Distance", type = PCGParameterType.Float, value = minDistance },
            new PCGNodeParameter { name = "Max Distance", type = PCGParameterType.Float, value = maxDistance }
        };
        }

        public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
        {
            if (inputPoints == null) return new List<PCGPoint>();

            var filtered = new List<PCGPoint>();
            var center = context.generationBounds.center;

            foreach (var point in inputPoints)
            {
                bool shouldKeep = false;

                switch (filterType)
                {
                    case FilterType.DensityThreshold:
                        shouldKeep = point.density >= minDensity;
                        break;
                    case FilterType.RandomChance:
                        shouldKeep = context.GetRandomFloat(0, 1) <= randomChance;
                        break;
                    case FilterType.TagMask:
                        shouldKeep = (point.tag & requiredTag) != 0;
                        break;
                    case FilterType.DistanceFromCenter:
                        float dist = Vector3.Distance(point.position, center);
                        shouldKeep = dist >= minDistance && dist <= maxDistance;
                        break;
                }

                if (shouldKeep)
                {
                    filtered.Add(point);
                }
            }

            context.pointsFiltered += inputPoints.Count - filtered.Count;
            return filtered;
        }

        public override System.Type GetViewType()
        {
            return typeof(PCGFilterNodeView);
        }
    }
}
