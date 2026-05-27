namespace PCG
{
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(fileName = "New PCG Graph", menuName = "PCG/Graph")]
    public class PCGGraphData : ScriptableObject
    {
        public List<PCGNodeData> nodes = new List<PCGNodeData>();
        public List<PCGEdgeData> edges = new List<PCGEdgeData>();

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
    }
}
