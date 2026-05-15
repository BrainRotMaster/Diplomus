namespace PCG
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "New PCG Graph", menuName = "PCG/Graph")]
    public class PCGGraphData : ScriptableObject
    {
        public List<PCGNodeData> nodes = new List<PCGNodeData>();
        public List<PCGEdgeData> edges = new List<PCGEdgeData>();
        public string entryNodeGUID;

        private Dictionary<string, PCGNodeData> nodeDictionary;

        public void InitCache()
        {
            nodeDictionary = new Dictionary<string, PCGNodeData>();
            foreach (var node in nodes)
            {
                if (node != null && !string.IsNullOrEmpty(node.GUID))
                {
                    nodeDictionary[node.GUID] = node;
                }
            }
        }

        public PCGNodeData GetNodeByGUID(string guid)
        {
            if (nodeDictionary == null) InitCache();
            if (nodeDictionary == null) return null;
            nodeDictionary.TryGetValue(guid, out var node);
            return node;
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
    }
}
