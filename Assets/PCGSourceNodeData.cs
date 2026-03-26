using System.Collections.Generic;
using UnityEngine;

namespace PCG
{

    [CreateAssetMenu(fileName = "Source Node", menuName = "PCG/Nodes/Source")]
    public class PCGSourceNodeData : PCGNodeData
    {
        public enum SourceType
        {
            Grid,
            Random,
            SurfaceRaycast,
            Curve
        }

        public SourceType sourceType = SourceType.Grid;
        public Vector2 gridSize = new Vector2(10, 10);
        public float spacing = 1f;
        public int randomPointCount = 100;
        public LayerMask surfaceLayer = ~0;

        public override List<PCGNodeParameter> GetParameters()
        {
            return new List<PCGNodeParameter>
        {
            new PCGNodeParameter { name = "Source Type", type = PCGParameterType.Dropdown,
                value = (int)sourceType, options = new[] { "Grid", "Random", "Surface Raycast", "Curve" } },
            new PCGNodeParameter { name = "Grid Width", type = PCGParameterType.Int, value = (int)gridSize.x, minValue = 1, maxValue = 100 },
            new PCGNodeParameter { name = "Grid Height", type = PCGParameterType.Int, value = (int)gridSize.y, minValue = 1, maxValue = 100 },
            new PCGNodeParameter { name = "Spacing", type = PCGParameterType.Float, value = spacing, minValue = 0.1f, maxValue = 10f },
            new PCGNodeParameter { name = "Point Count", type = PCGParameterType.Int, value = randomPointCount, minValue = 1, maxValue = 10000 },
            new PCGNodeParameter { name = "Surface Layer", type = PCGParameterType.LayerMask, value = surfaceLayer.value }
        };
        }

        public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
        {
            var points = new List<PCGPoint>();

            switch (sourceType)
            {
                case SourceType.Grid:
                    points = GenerateGrid(context);
                    break;
                case SourceType.Random:
                    points = GenerateRandom(context);
                    break;
                case SourceType.SurfaceRaycast:
                    points = GenerateSurfaceRaycast(context);
                    break;
            }

            context.pointsGenerated += points.Count;
            return points;
        }

        private List<PCGPoint> GenerateGrid(PCGExecutionContext context)
        {
            var points = new List<PCGPoint>();
            var bounds = context.generationBounds;

            float startX = bounds.min.x;
            float startZ = bounds.min.z;

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int z = 0; z < gridSize.y; z++)
                {
                    var pos = new Vector3(
                        startX + x * spacing,
                        bounds.center.y,
                        startZ + z * spacing
                    );

                    points.Add(new PCGPoint(pos));
                }
            }

            return points;
        }

        private List<PCGPoint> GenerateRandom(PCGExecutionContext context)
        {
            var points = new List<PCGPoint>();
            var bounds = context.generationBounds;

            for (int i = 0; i < randomPointCount; i++)
            {
                var pos = new Vector3(
                    context.GetRandomFloat(bounds.min.x, bounds.max.x),
                    bounds.center.y,
                    context.GetRandomFloat(bounds.min.z, bounds.max.z)
                );

                points.Add(new PCGPoint(pos));
            }

            return points;
        }

        private List<PCGPoint> GenerateSurfaceRaycast(PCGExecutionContext context)
        {
            var points = new List<PCGPoint>();
            var bounds = context.generationBounds;

            // Генерируем точки в сетке и проецируем на поверхность
            int gridSteps = Mathf.CeilToInt(Mathf.Sqrt(randomPointCount));
            float stepX = bounds.size.x / gridSteps;
            float stepZ = bounds.size.z / gridSteps;

            for (int x = 0; x < gridSteps; x++)
            {
                for (int z = 0; z < gridSteps; z++)
                {
                    float posX = bounds.min.x + x * stepX + stepX * 0.5f;
                    float posZ = bounds.min.z + z * stepZ + stepZ * 0.5f;

                    RaycastHit hit;
                    if (Physics.Raycast(new Vector3(posX, bounds.max.y, posZ), Vector3.down, out hit, bounds.size.y, surfaceLayer))
                    {
                        var point = new PCGPoint(hit.point);
                        point.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                        point.tag = 1; // Ground
                        points.Add(point);
                    }
                }
            }

            return points;
        }

        public override System.Type GetViewType()
        {
            return typeof(PCGSourceNodeView);
        }
    }
}
