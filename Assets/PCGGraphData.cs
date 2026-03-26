using System.Collections.Generic;
using UnityEngine;

namespace PCG
{
    [CreateAssetMenu(fileName = "New PCG Graph", menuName = "PCG/Graph")]
    public class PCGGraphData : ScriptableObject
    {
        public List<PCGNodeData> nodes = new List<PCGNodeData>();
        public List<PCGEdgeData> edges = new List<PCGEdgeData>();

        // Входная нода (откуда начинается генерация)
        public string entryNodeGUID;

        // Кэш для быстрого доступа
        [System.NonSerialized]
        private Dictionary<string, PCGNodeData> nodeDictionary;

        public void InitCache()
        {
            nodeDictionary = new Dictionary<string, PCGNodeData>();
            foreach (var node in nodes)
            {
                nodeDictionary[node.GUID] = node;
            }
        }

        public PCGNodeData GetNodeByGUID(string guid)
        {
            if (nodeDictionary == null) InitCache();
            return nodeDictionary.GetValueOrDefault(guid);
        }

        public List<PCGNodeData> GetOutputNodes(string nodeGUID)
        {
            var result = new List<PCGNodeData>();
            foreach (var edge in edges)
            {
                if (edge.sourceNodeGUID == nodeGUID)
                {
                    var targetNode = GetNodeByGUID(edge.targetNodeGUID);
                    if (targetNode != null) result.Add(targetNode);
                }
            }
            return result;
        }

        public List<PCGNodeData> GetInputNodes(string nodeGUID)
        {
            var result = new List<PCGNodeData>();
            foreach (var edge in edges)
            {
                if (edge.targetNodeGUID == nodeGUID)
                {
                    var sourceNode = GetNodeByGUID(edge.sourceNodeGUID);
                    if (sourceNode != null) result.Add(sourceNode);
                }
            }
            return result;
        }
    }

    [System.Serializable]
    public class PCGEdgeData
    {
        public string sourceNodeGUID;
        public string sourcePortName;
        public string targetNodeGUID;
        public string targetPortName;
    }
}
