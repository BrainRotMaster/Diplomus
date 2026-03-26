using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PCG
{

    public class PCGGraphExecutor
    {
        private PCGGraphData graphData;

        public PCGGraphExecutor(PCGGraphData data)
        {
            graphData = data;
            graphData.InitCache();
        }

        // Основной метод генерации
        public List<PCGPoint> Execute(PCGExecutionContext context)
        {
            // Найти стартовые ноды (которые не имеют входов)
            var startNodes = GetStartNodes();

            var results = new List<PCGPoint>();

            foreach (var startNode in startNodes)
            {
                var points = ExecuteNode(startNode, null, context);
                results.AddRange(points);
            }

            Debug.Log($"[PCG] Generated {results.Count} points. Stats: Generated={context.pointsGenerated}, Filtered={context.pointsFiltered}");

            return results;
        }

        private List<PCGPoint> ExecuteNode(PCGNodeData node, List<PCGPoint> input, PCGExecutionContext context)
        {
            // Получаем выходные соединения
            var outputNodes = graphData.GetOutputNodes(node.GUID);

            // Выполняем текущую ноду
            var output = node.Process(input ?? new List<PCGPoint>(), context);

            // Рекурсивно выполняем следующие ноды
            foreach (var outputNode in outputNodes)
            {
                ExecuteNode(outputNode, output, context);
            }

            return output;
        }

        private List<PCGNodeData> GetStartNodes()
        {
            var allNodes = graphData.nodes;
            var nodesWithInputs = new HashSet<string>();

            foreach (var edge in graphData.edges)
            {
                nodesWithInputs.Add(edge.targetNodeGUID);
            }

            return allNodes.Where(n => !nodesWithInputs.Contains(n.GUID)).ToList();
        }

        // Топологическая сортировка для правильного порядка выполнения
        public List<PCGNodeData> GetTopologicalOrder()
        {
            // Реализация топологической сортировки для графа
            // Нужно для отладки и оптимизации
            var result = new List<PCGNodeData>();
            var visited = new HashSet<string>();
            var stack = new Stack<PCGNodeData>();

            foreach (var node in graphData.nodes)
            {
                if (!visited.Contains(node.GUID))
                    TopologicalSortUtil(node, visited, stack);
            }

            while (stack.Count > 0)
                result.Add(stack.Pop());

            return result;
        }

        private void TopologicalSortUtil(PCGNodeData node, HashSet<string> visited, Stack<PCGNodeData> stack)
        {
            visited.Add(node.GUID);

            foreach (var outputNode in graphData.GetOutputNodes(node.GUID))
            {
                if (!visited.Contains(outputNode.GUID))
                    TopologicalSortUtil(outputNode, visited, stack);
            }

            stack.Push(node);
        }
    }
}
