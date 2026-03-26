namespace PCG
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Filter Node", menuName = "PCG/Nodes/Filter")]
    public class PCGFilterNodeData : PCGNodeData
    {
        public enum FilterType
        {
            RandomChance,
            DensityThreshold
        }

        public FilterType filterType = FilterType.RandomChance;
        public float randomChance = 0.5f;
        public float minDensity = 0.5f;

        public override List<PCGNodeParameter> GetParameters()
        {
            return new List<PCGNodeParameter>
        {
            new PCGNodeParameter("Filter Type", PCGParameterType.Dropdown, (int)filterType)
            {
                options = new[] { "Random Chance", "Density Threshold" }
            },
            new PCGNodeParameter("Random Chance", PCGParameterType.Float, randomChance)
            {
                minValue = 0f,
                maxValue = 1f
            },
            new PCGNodeParameter("Min Density", PCGParameterType.Float, minDensity)
            {
                minValue = 0f,
                maxValue = 1f
            }
        };
        }

        public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
        {
            if (inputPoints == null) return new List<PCGPoint>();

            // Обновляем параметры из UI
            foreach (var param in GetParameters())
            {
                switch (param.name)
                {
                    case "Filter Type": filterType = (FilterType)(int)param.value; break;
                    case "Random Chance": randomChance = (float)param.value; break;
                    case "Min Density": minDensity = (float)param.value; break;
                }
            }

            var filtered = new List<PCGPoint>();

            foreach (var point in inputPoints)
            {
                bool shouldKeep = false;

                switch (filterType)
                {
                    case FilterType.RandomChance:
                        shouldKeep = context.GetRandomFloat(0, 1) <= randomChance;
                        break;
                    case FilterType.DensityThreshold:
                        shouldKeep = point.density >= minDensity;
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

        public override string GetViewTypeName()
        {
            return "PCGFilterNodeView";
        }
    }
}
