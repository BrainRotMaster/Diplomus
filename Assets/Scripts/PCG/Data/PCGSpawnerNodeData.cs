using PCG;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spawner Node", menuName = "PCG/Nodes/Spawner")]
public class PCGSpawnerNodeData : PCGNodeData
{
    [SerializeField] private GameObject prefab;

    public GameObject Prefab { get => prefab; set => prefab = value; }

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>
        {
            new PCGNodeParameter("Prefab", PCGParameterType.GameObject, prefab)
        };
    }

    public override void UpdateParameter(string name, object value)
    {
        if (name == "Prefab")
        {
            prefab = (GameObject)value;
        }
    }

    public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        if (inputPoints == null || inputPoints.Count == 0 || prefab == null)
        {
            return inputPoints ?? new List<PCGPoint>();
        }

        foreach (var point in inputPoints)
        {
            GameObject instance;
            if (context.worldRoot != null)
            {
                instance = Object.Instantiate(prefab, point.position, point.rotation, context.worldRoot);
            }
            else
            {
                instance = Object.Instantiate(prefab, point.position, point.rotation);
            }

            instance.transform.localScale = point.scale;
        }

        return inputPoints;
    }

    public override string GetViewTypeName() => "PCGSpawnerNodeView";
}
