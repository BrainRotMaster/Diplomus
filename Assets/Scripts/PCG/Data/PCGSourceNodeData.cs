namespace PCG
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Source Node", menuName = "PCG/Nodes/Source")]
    public class PCGSourceNodeData : PCGNodeData
    {
        public enum SourceType
        {
            Grid,
            Random
        }

        public SourceType sourceType = SourceType.Grid;
        public int gridWidth = 10;
        public int gridHeight = 10;
        public float spacing = 1f;
        public int randomPointCount = 100;

        public override List<PCGNodeParameter> GetParameters()
        {
            return new List<PCGNodeParameter>
        {
            new PCGNodeParameter("Source Type", PCGParameterType.Dropdown, (int)sourceType)
            {
                options = new[] { "Grid", "Random" }
            },
            new PCGNodeParameter("Grid Width", PCGParameterType.Int, gridWidth)
            {
                minValue = 1,
                maxValue = 100
            },
            new PCGNodeParameter("Grid Height", PCGParameterType.Int, gridHeight)
            {
                minValue = 1,
                maxValue = 100
            },
            new PCGNodeParameter("Spacing", PCGParameterType.Float, spacing)
            {
                minValue = 0.1f,
                maxValue = 10f
            },
            new PCGNodeParameter("Point Count", PCGParameterType.Int, randomPointCount)
            {
                minValue = 1,
                maxValue = 10000
            }
        };
        }

        public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
        {
            var points = new List<PCGPoint>();
            var bounds = context.generationBounds;

            // Обновляем параметры из UI
            foreach (var param in GetParameters())
            {
                switch (param.name)
                {
                    case "Source Type": sourceType = (SourceType)(int)param.value; break;
                    case "Grid Width": gridWidth = (int)param.value; break;
                    case "Grid Height": gridHeight = (int)param.value; break;
                    case "Spacing": spacing = (float)param.value; break;
                    case "Point Count": randomPointCount = (int)param.value; break;
                }
            }

            switch (sourceType)
            {
                case SourceType.Grid:
                    for (int x = 0; x < gridWidth; x++)
                    {
                        for (int z = 0; z < gridHeight; z++)
                        {
                            var pos = new Vector3(
                                bounds.min.x + x * spacing,
                                bounds.center.y,
                                bounds.min.z + z * spacing
                            );
                            points.Add(new PCGPoint(pos));
                        }
                    }
                    break;

                case SourceType.Random:
                    for (int i = 0; i < randomPointCount; i++)
                    {
                        var pos = new Vector3(
                            context.GetRandomFloat(bounds.min.x, bounds.max.x),
                            bounds.center.y,
                            context.GetRandomFloat(bounds.min.z, bounds.max.z)
                        );
                        points.Add(new PCGPoint(pos));
                    }
                    break;
            }

            context.pointsGenerated += points.Count;
            return points;
        }

        public override string GetViewTypeName()
        {
            return "PCGSourceNodeView";
        }
    }
}
