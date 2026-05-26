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
                var outputStreams = node.ProcessMulti(inputPoints, context) ?? new PCGNodeOutput();

                if (!outputsByNode.TryGetValue(node.GUID, out var outgoingEdgesByPort) || outgoingEdgesByPort.Count == 0)
                {
                    foreach (var stream in outputStreams.Streams)
                    {
                        finalResults.AddRange(stream.Value);
                    }

                    continue;
                }

                foreach (var stream in outputStreams.Streams)
                {
                    string outputPortName = NormalizePortName(stream.Key, PCGNodeData.DefaultOutputPortName);
                    List<PCGPoint> outputPoints = stream.Value ?? new List<PCGPoint>();

                    if (!outgoingEdgesByPort.TryGetValue(outputPortName, out var outgoingEdges) || outgoingEdges.Count == 0)
                    {
                        finalResults.AddRange(outputPoints);
                        continue;
                    }

                    foreach (var edge in outgoingEdges)
                    {
                        var childNode = graphData.GetNodeByGUID(edge.targetNodeGUID);
                        if (childNode == null)
                        {
                            continue;
                        }

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

        private Dictionary<string, Dictionary<string, List<PCGEdgeData>>> BuildOutputMap()
        {
            var outputsByNode = new Dictionary<string, Dictionary<string, List<PCGEdgeData>>>();

            foreach (var node in validNodes)
            {
                outputsByNode[node.GUID] = new Dictionary<string, List<PCGEdgeData>>();
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

                string sourcePortName = NormalizePortName(edge.sourcePortName, PCGNodeData.DefaultOutputPortName);
                if (!outputsByNode[edge.sourceNodeGUID].TryGetValue(sourcePortName, out var portEdges))
                {
                    portEdges = new List<PCGEdgeData>();
                    outputsByNode[edge.sourceNodeGUID][sourcePortName] = portEdges;
                }

                portEdges.Add(edge);
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

        private static string NormalizePortName(string portName, string fallback)
        {
            return string.IsNullOrEmpty(portName) ? fallback : portName;
        }
    }
}
