namespace PCG
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Spawner Node", menuName = "PCG/Nodes/Spawner")]
    public class PCGSpawnerNodeData : PCGNodeData
    {
        [SerializeField]
        private GameObject prefab;

        public bool randomizeRotation = true;

        public GameObject Prefab
        {
            get => prefab;
            set => prefab = value;
        }

        public override List<PCGNodeParameter> GetParameters()
        {
            return new List<PCGNodeParameter>
        {
            new PCGNodeParameter("Prefab", PCGParameterType.GameObject, prefab),
            new PCGNodeParameter("Random Rotation", PCGParameterType.Bool, randomizeRotation)
        };
        }

        public void UpdateParameter(string name, object value)
        {
            switch (name)
            {
                case "Prefab":
                    prefab = (GameObject)value;
                    break;
                case "Random Rotation":
                    randomizeRotation = (bool)value;
                    break;
            }
        }

        public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
        {
            Debug.Log($"Spawner node processing {inputPoints?.Count ?? 0} points");

            if (inputPoints == null || inputPoints.Count == 0)
            {
                Debug.LogWarning("No input points for spawner!");
                return inputPoints ?? new List<PCGPoint>();
            }

            if (prefab == null)
            {
                Debug.LogError($"No prefab assigned to spawner node! Node name: {name}, GUID: {GUID}");
                Debug.LogError("Please assign a prefab in the spawner node and click Save Graph");
                return inputPoints;
            }

            int spawnedCount = 0;

            foreach (var point in inputPoints)
            {
                Quaternion rotation = point.rotation;
                if (randomizeRotation)
                {
                    rotation = Quaternion.Euler(0, context.GetRandomFloat(0, 360), 0);
                }

                if (context.worldRoot != null)
                {
                    UnityEngine.Object.Instantiate(prefab, point.position, rotation, context.worldRoot);
                }
                else
                {
                    UnityEngine.Object.Instantiate(prefab, point.position, rotation);
                }

                spawnedCount++;
            }

            Debug.Log($"Spawned {spawnedCount} objects with prefab: {prefab.name}");
            return inputPoints;
        }

        public override string GetViewTypeName()
        {
            return "PCGSpawnerNodeView";
        }
    }
}
