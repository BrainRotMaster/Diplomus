namespace PCG
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class PCGGraphExecutor
    {
        private PCGGraphData graphData;

        public PCGGraphExecutor(PCGGraphData data)
        {
            graphData = data;
            graphData.InitCache();
        }

        public List<PCGPoint> Execute(PCGExecutionContext context)
        {
            var startNodes = GetStartNodes();
            var results = new List<PCGPoint>();

            Debug.Log($"Found {startNodes.Count} start nodes");

            foreach (var startNode in startNodes)
            {
                Debug.Log($"Executing start node: {startNode.name}");
                var points = ExecuteNode(startNode, null, context);
                results.AddRange(points);
                Debug.Log($"Node {startNode.name} generated {points.Count} points");
            }

            Debug.Log($"Total points after execution: {results.Count}");
            return results;
        }

        private List<PCGPoint> ExecuteNode(PCGNodeData node, List<PCGPoint> input, PCGExecutionContext context)
        {
            Debug.Log($"Executing node: {node.name}");

            var outputNodes = graphData.GetOutputNodes(node.GUID);
            var output = node.Process(input ?? new List<PCGPoint>(), context);

            Debug.Log($"Node {node.name} output points count: {output.Count}");

            if (outputNodes.Count == 0)
            {
                return output;
            }

            var finalResults = new List<PCGPoint>();

            foreach (var outputNode in outputNodes)
            {
                Debug.Log($"Passing to child node: {outputNode.name}");
                finalResults.AddRange(ExecuteNode(outputNode, output, context));
            }

            return finalResults;
        }

        private List<PCGNodeData> GetStartNodes()
        {
            var nodesWithInputs = new HashSet<string>();

            foreach (var edge in graphData.edges)
            {
                nodesWithInputs.Add(edge.targetNodeGUID);
            }

            return graphData.nodes.Where(n => !nodesWithInputs.Contains(n.GUID)).ToList();
        }
    }
}
