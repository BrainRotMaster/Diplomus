using System.Collections.Generic;
using UnityEngine;

namespace PCG
{

    [CreateAssetMenu(fileName = "Spawner Node", menuName = "PCG/Nodes/Spawner")]
    public class PCGSpawnerNodeData : PCGNodeData
    {
        public GameObject[] prefabs;
        public bool useRandomPrefab = true;
        public bool randomizeRotation = true;
        public bool randomizeScale = false;
        public Vector2 scaleRange = new Vector2(0.8f, 1.2f);

        public override List<PCGNodeParameter> GetParameters()
        {
            return new List<PCGNodeParameter>
        {
            new PCGNodeParameter { name = "Prefabs", type = PCGParameterType.GameObject, value = prefabs },
            new PCGNodeParameter { name = "Random Prefab", type = PCGParameterType.Bool, value = useRandomPrefab },
            new PCGNodeParameter { name = "Random Rotation", type = PCGParameterType.Bool, value = randomizeRotation },
            new PCGNodeParameter { name = "Random Scale", type = PCGParameterType.Bool, value = randomizeScale },
            new PCGNodeParameter { name = "Scale Range Min", type = PCGParameterType.Float, value = scaleRange.x },
            new PCGNodeParameter { name = "Scale Range Max", type = PCGParameterType.Float, value = scaleRange.y }
        };
        }

        public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
        {
            if (inputPoints == null || prefabs == null || prefabs.Length == 0)
                return new List<PCGPoint>();

            foreach (var point in inputPoints)
            {
                // Выбор префаба
                GameObject prefab = prefabs[0];
                if (useRandomPrefab && prefabs.Length > 1)
                {
                    int index = context.GetRandomInt(0, prefabs.Length);
                    prefab = prefabs[index];
                }

                // Вычисление финальной позиции
                Vector3 finalPosition = point.position;

                // Вычисление финального поворота
                Quaternion finalRotation = point.rotation;
                if (randomizeRotation)
                {
                    finalRotation = Quaternion.Euler(0, context.GetRandomFloat(0, 360), 0);
                }

                // Вычисление финального масштаба
                Vector3 finalScale = point.scale;
                if (randomizeScale)
                {
                    float scale = context.GetRandomFloat(scaleRange.x, scaleRange.y);
                    finalScale = Vector3.one * scale;
                }

                // Спавн объекта
                if (context.worldRoot != null)
                {
                    var instance = Object.Instantiate(prefab, finalPosition, finalRotation, context.worldRoot);
                    instance.transform.localScale = finalScale;
                }
                else
                {
                    var instance = Object.Instantiate(prefab, finalPosition, finalRotation);
                    instance.transform.localScale = finalScale;
                }
            }

            return inputPoints;
        }

        public override System.Type GetViewType()
        {
            return typeof(PCGSpawnerNodeView);
        }
    }
}
