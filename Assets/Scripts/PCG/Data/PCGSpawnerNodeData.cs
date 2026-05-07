using PCG;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spawner Node", menuName = "PCG/Nodes/Spawner")]
public class PCGSpawnerNodeData : PCGNodeData
{
    [System.Serializable]
    public class WeightedPrefabEntry
    {
        public GameObject prefab;
        public float weight = 1f;
    }

    [SerializeField] private List<WeightedPrefabEntry> prefabEntries = new List<WeightedPrefabEntry>
    {
        new WeightedPrefabEntry()
    };

    public List<WeightedPrefabEntry> PrefabEntries => prefabEntries;

    public void AddPrefabEntry()
    {
        prefabEntries.Add(new WeightedPrefabEntry());
    }

    public void RemovePrefabEntryAt(int index)
    {
        if (index < 0 || index >= prefabEntries.Count)
        {
            return;
        }

        prefabEntries.RemoveAt(index);
    }

    public override List<PCGNodeParameter> GetParameters()
    {
        return new List<PCGNodeParameter>();
    }

    public override void UpdateParameter(string name, object value)
    {
    }

    public override List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        if (inputPoints == null || inputPoints.Count == 0)
        {
            return inputPoints ?? new List<PCGPoint>();
        }

        var validEntries = GetValidEntries();
        if (validEntries.Count == 0)
        {
            return inputPoints;
        }

        float totalWeight = GetTotalWeight(validEntries);
        if (totalWeight <= 0f)
        {
            return inputPoints;
        }

        foreach (var point in inputPoints)
        {
            var prefab = SelectPrefab(validEntries, totalWeight, context);
            if (prefab == null)
            {
                continue;
            }

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

    private List<WeightedPrefabEntry> GetValidEntries()
    {
        var validEntries = new List<WeightedPrefabEntry>();
        foreach (var entry in prefabEntries)
        {
            if (entry != null && entry.prefab != null && entry.weight > 0f)
            {
                validEntries.Add(entry);
            }
        }

        return validEntries;
    }

    private static float GetTotalWeight(List<WeightedPrefabEntry> entries)
    {
        float totalWeight = 0f;
        foreach (var entry in entries)
        {
            totalWeight += entry.weight;
        }

        return totalWeight;
    }

    private static GameObject SelectPrefab(List<WeightedPrefabEntry> entries, float totalWeight, PCGExecutionContext context)
    {
        float randomValue = context.GetRandomFloat(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach (var entry in entries)
        {
            cumulativeWeight += entry.weight;
            if (randomValue <= cumulativeWeight)
            {
                return entry.prefab;
            }
        }

        return entries[entries.Count - 1].prefab;
    }
}
