namespace PCG
{
    using System.Collections.Generic;
    using UnityEngine;

    public class PCGGraphExecutor
    {
        private readonly PCGGraphData graphData;
        private readonly List<PCGNodeData> validNodes = new List<PCGNodeData>();

        public PCGGraphExecutor(PCGGraphData data)
        {
            graphData = data;
            graphData.InitCache();
            CacheValidNodes();
        }

        public List<PCGPoint> Execute(PCGExecutionContext context)
        {
            var indegreeByNode = BuildIndegreeMap();
            var outputsByNode = BuildOutputMap();
            var accumulatedInputs = new Dictionary<string, List<PCGPoint>>();
            var readyQueue = new Queue<PCGNodeData>();
            var finalResults = new List<PCGPoint>();
            int processedNodes = 0;

            foreach (var node in validNodes)
            {
                if (indegreeByNode[node.GUID] == 0)
                {
                    readyQueue.Enqueue(node);
                }
            }

            Debug.Log($"Executing PCG graph with {readyQueue.Count} start nodes");

            while (readyQueue.Count > 0)
            {
                var node = readyQueue.Dequeue();
                processedNodes++;

                accumulatedInputs.TryGetValue(node.GUID, out var inputPoints);
                inputPoints ??= new List<PCGPoint>();

                Debug.Log($"Executing node: {node.name} with {inputPoints.Count} input points");
                var outputPoints = node.Process(inputPoints, context) ?? new List<PCGPoint>();

                if (!outputsByNode.TryGetValue(node.GUID, out var childNodes) || childNodes.Count == 0)
                {
                    finalResults.AddRange(outputPoints);
                    continue;
                }

                foreach (var childNode in childNodes)
                {
                    if (!accumulatedInputs.TryGetValue(childNode.GUID, out var childInputs))
                    {
                        childInputs = new List<PCGPoint>();
                        accumulatedInputs[childNode.GUID] = childInputs;
                    }

                    childInputs.AddRange(outputPoints);
                    indegreeByNode[childNode.GUID]--;

                    if (indegreeByNode[childNode.GUID] == 0)
                    {
                        readyQueue.Enqueue(childNode);
                    }
                }
            }

            if (processedNodes != validNodes.Count)
            {
                Debug.LogError("PCG graph execution failed because the graph contains a cycle or invalid dependency chain.");
            }

            Debug.Log($"PCG execution complete. Final point count: {finalResults.Count}");
            return finalResults;
        }

        private Dictionary<string, int> BuildIndegreeMap()
        {
            var indegreeByNode = new Dictionary<string, int>();

            foreach (var node in validNodes)
            {
                indegreeByNode[node.GUID] = 0;
            }

            foreach (var edge in graphData.edges)
            {
                if (edge != null && !string.IsNullOrEmpty(edge.targetNodeGUID) && indegreeByNode.ContainsKey(edge.targetNodeGUID))
                {
                    indegreeByNode[edge.targetNodeGUID]++;
                }
            }

            return indegreeByNode;
        }

        private Dictionary<string, List<PCGNodeData>> BuildOutputMap()
        {
            var outputsByNode = new Dictionary<string, List<PCGNodeData>>();

            foreach (var node in validNodes)
            {
                outputsByNode[node.GUID] = new List<PCGNodeData>();
            }

            foreach (var edge in graphData.edges)
            {
                if (edge == null || string.IsNullOrEmpty(edge.targetNodeGUID) || string.IsNullOrEmpty(edge.sourceNodeGUID))
                {
                    continue;
                }

                var targetNode = graphData.GetNodeByGUID(edge.targetNodeGUID);
                if (targetNode == null || !outputsByNode.ContainsKey(edge.sourceNodeGUID))
                {
                    continue;
                }

                outputsByNode[edge.sourceNodeGUID].Add(targetNode);
            }

            return outputsByNode;
        }

        private void CacheValidNodes()
        {
            validNodes.Clear();

            int skippedNodeCount = 0;
            foreach (var node in graphData.nodes)
            {
                if (node == null || string.IsNullOrEmpty(node.GUID))
                {
                    skippedNodeCount++;
                    continue;
                }

                validNodes.Add(node);
            }

            if (skippedNodeCount > 0)
            {
                Debug.LogWarning($"PCG graph executor skipped {skippedNodeCount} missing or invalid node entries.");
            }
        }
    }
}
