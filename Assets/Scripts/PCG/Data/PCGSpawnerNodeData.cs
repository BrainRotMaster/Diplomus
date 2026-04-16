using PCG;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spawner Node", menuName = "PCG/Nodes/Spawner")]
public class PCGSpawnerNodeData : PCGNodeData
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private bool randomizeRotation = true;

    public GameObject Prefab { get => prefab; set => prefab = value; }
    public bool RandomizeRotation { get => randomizeRotation; set => randomizeRotation = value; }

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>
        {
            new PCGNodeParameter("Prefab", PCGParameterType.GameObject, prefab),
            new PCGNodeParameter("Random Rotation", PCGParameterType.Bool, randomizeRotation)
        };
    }

    public override void UpdateParameter(string name, object value)
    {
        switch (name)
        {
            case "Prefab": prefab = (GameObject)value; break;
            case "Random Rotation": randomizeRotation = (bool)value; break;
        }
    }

    public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        if (inputPoints == null || inputPoints.Count == 0 || prefab == null)
            return inputPoints ?? new List<PCGPoint>();

        foreach (var point in inputPoints)
        {
            Quaternion rotation = point.rotation;
            if (randomizeRotation)
            {
                rotation = Quaternion.Euler(0, context.GetRandomFloat(0, 360), 0);
            }

            if (context.worldRoot != null)
            {
                Instantiate(prefab, point.position, rotation, context.worldRoot);
            }
            else
            {
                Instantiate(prefab, point.position, rotation);
            }
        }

        return inputPoints;
    }

    public override string GetViewTypeName() => "PCGSpawnerNodeView";
}