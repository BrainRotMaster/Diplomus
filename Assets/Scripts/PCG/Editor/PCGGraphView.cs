namespace PCG
{
    using PCG.Windows;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class PCGGraphView : GraphView
    {
        private PCGGraphData graphData;
        private PCGGraphData currentGraph;
        private Dictionary<string, PCGNodeView> nodeDictionary = new Dictionary<string, PCGNodeView>();
        private PCGEditorWindow editorWindow;

        public PCGGraphView(PCGGraphData data, PCGEditorWindow window)
        {
            graphData = data;
            currentGraph = data;
            editorWindow = window;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            grid.StretchToParentSize();
            Insert(0, grid);

            graphViewChanged += OnGraphViewChanged;

            if (graphData != null)
            {
                LoadNodes();
            }
        }

        private void LoadNodes()
        {
            // Use list copies to avoid collection-modified errors while rebuilding the view.
            var nodesToCreate = new List<PCGNodeData>(graphData.nodes);

            foreach (var nodeData in nodesToCreate)
            {
                CreateNodeFromData(nodeData);
            }

            var edgesToCreate = new List<PCGEdgeData>(graphData.edges);

            foreach (var edgeData in edgesToCreate)
            {
                var sourceNode = GetNodeByGUID(edgeData.sourceNodeGUID);
                var targetNode = GetNodeByGUID(edgeData.targetNodeGUID);

                if (sourceNode != null && targetNode != null)
                {
                    ConnectNodes(sourceNode, targetNode);
                }
                else
                {
                    Debug.LogWarning($"Cannot create edge: source={edgeData.sourceNodeGUID}, target={edgeData.targetNodeGUID}");
                }
            }
        }

        private void CreateNodeFromData(PCGNodeData nodeData)
        {
            string viewTypeName = nodeData.GetViewTypeName();

            // Resolve the node view type at runtime because the view lives in an editor assembly.
            Type viewType = null;

            // Search through loaded assemblies for the matching node view type.
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                viewType = assembly.GetType(viewTypeName);
                if (viewType != null) break;

                // Try the default namespace used by the editor views.
                viewType = assembly.GetType($"PCG.{viewTypeName}");
                if (viewType != null) break;
            }

            if (viewType != null)
            {
                var nodeView = Activator.CreateInstance(viewType) as PCGNodeView;

                if (nodeView != null)
                {
                    nodeView.OnNodeChanged = MarkGraphDirty;
                    nodeView.Initialize(nodeData, nodeData.position);
                    nodeView.SetPosition(new Rect(nodeData.position, new Vector2(250, 150)));

                    AddElement(nodeView);
                    RegisterNode(nodeView);
                }
                else
                {
                    Debug.LogError($"Failed to create instance of {viewTypeName}");
                }
            }
            else
            {
                Debug.LogError($"View type not found: {viewTypeName}. Make sure the view class is in the Editor folder and has the correct namespace.");
            }
        }

        public void RegisterNode(PCGNodeView node)
        {
            if (!nodeDictionary.ContainsKey(node.GUID))
            {
                nodeDictionary[node.GUID] = node;
            }
        }

        public PCGNodeView GetNodeByGUID(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            nodeDictionary.TryGetValue(guid, out var node);
            return node;
        }

        public Edge ConnectNodes(PCGNodeView fromNode, PCGNodeView toNode)
        {
            Port outputPort = fromNode.OutputPort;
            Port inputPort = toNode.InputPort;

            if (outputPort == null || inputPort == null)
            {
                Debug.LogWarning($"Cannot connect: ports missing. FromNode={fromNode?.title}, ToNode={toNode?.title}");
                return null;
            }

            var edge = new Edge
            {
                output = outputPort,
                input = inputPort
            };

            AddElement(edge);

            outputPort.Connect(edge);
            inputPort.Connect(edge);

            SaveEdgeData(fromNode.GUID, toNode.GUID);

            return edge;
        }

        private void SaveEdgeData(string sourceGUID, string targetGUID)
        {
            if (graphData == null) return;

            // Avoid duplicating serialized edges when the view reconnects an existing link.
            bool exists = graphData.edges.Any(e =>
                e.sourceNodeGUID == sourceGUID && e.targetNodeGUID == targetGUID);

            if (!exists)
            {
                var edgeData = new PCGEdgeData
                {
                    sourceNodeGUID = sourceGUID,
                    targetNodeGUID = targetGUID
                };

                graphData.edges.Add(edgeData);
                PersistGraphChange(true);
            }
        }

        private void RemoveEdge(Edge edge)
        {
            RemoveElement(edge);

            if (graphData != null)
            {
                var sourceNode = edge.output.node as PCGNodeView;
                var targetNode = edge.input.node as PCGNodeView;

                if (sourceNode != null && targetNode != null)
                {
                    var edgeToRemove = graphData.edges.Find(e =>
                        e.sourceNodeGUID == sourceNode.GUID &&
                        e.targetNodeGUID == targetNode.GUID);

                    if (edgeToRemove != null)
                    {
                        graphData.edges.Remove(edgeToRemove);
                        PersistGraphChange(true);
                    }
                }
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is PCGNodeView nodeView)
                    {
                        if (graphData != null)
                        {
                            graphData.nodes.Remove(nodeView.nodeData);
                            nodeDictionary.Remove(nodeView.GUID);
                            PersistGraphChange(true);
                        }
                    }
                    else if (element is Edge edge)
                    {
                        RemoveEdge(edge);
                    }
                }
            }

            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    var fromNode = edge.output.node as PCGNodeView;
                    var toNode = edge.input.node as PCGNodeView;

                    if (fromNode != null && toNode != null)
                    {
                        SaveEdgeData(fromNode.GUID, toNode.GUID);
                    }
                }
            }

            return change;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            Vector2 mousePosition = evt.localMousePosition;

            evt.menu.AppendAction("Add Node/Source Node", (action) =>
            {
                CreateSourceNode(mousePosition);
            });

            evt.menu.AppendAction("Add Node/Filter Node", (action) =>
            {
                CreateFilterNode(mousePosition);
            });

            evt.menu.AppendAction("Add Node/Spawner Node", (action) =>
            {
                CreateSpawnerNode(mousePosition);
            });
        }

        private void CreateSourceNode(Vector2 position)
        {
            var nodeData = ScriptableObject.CreateInstance<PCGSourceNodeData>();
            nodeData.nodeName = "Source Node";
            nodeData.GUID = Guid.NewGuid().ToString();
            nodeData.position = position;

            string assetPath = GetAssetPathForNode(nodeData.GUID, "Source");
            AssetDatabase.CreateAsset(nodeData, assetPath);

            if (graphData != null)
            {
                graphData.nodes.Add(nodeData);
                PersistGraphChange(true);
            }

            CreateNodeFromData(nodeData);
        }

        private void CreateFilterNode(Vector2 position)
        {
            var nodeData = ScriptableObject.CreateInstance<PCGFilterNodeData>();
            nodeData.nodeName = "Filter Node";
            nodeData.GUID = Guid.NewGuid().ToString();
            nodeData.position = position;

            string assetPath = GetAssetPathForNode(nodeData.GUID, "Filter");
            AssetDatabase.CreateAsset(nodeData, assetPath);

            if (graphData != null)
            {
                graphData.nodes.Add(nodeData);
                PersistGraphChange(true);
            }

            CreateNodeFromData(nodeData);
        }

        private void CreateSpawnerNode(Vector2 position)
        {
            var nodeData = ScriptableObject.CreateInstance<PCGSpawnerNodeData>();
            nodeData.nodeName = "Spawner Node";
            nodeData.GUID = Guid.NewGuid().ToString();
            nodeData.position = position;

            string assetPath = GetAssetPathForNode(nodeData.GUID, "Spawner");
            AssetDatabase.CreateAsset(nodeData, assetPath);

            if (graphData != null)
            {
                graphData.nodes.Add(nodeData);
                PersistGraphChange(true);
            }

            CreateNodeFromData(nodeData);
        }

        private string GetAssetPathForNode(string guid, string type)
        {
            string basePath = "Assets/PCG/Graphs";
            if (!System.IO.Directory.Exists(basePath))
            {
                System.IO.Directory.CreateDirectory(basePath);
            }

            string graphName = currentGraph != null ? currentGraph.name : "Graph";
            string graphFolder = $"{basePath}/{graphName}_Nodes";

            if (!System.IO.Directory.Exists(graphFolder))
            {
                System.IO.Directory.CreateDirectory(graphFolder);
            }

            return $"{graphFolder}/{type}_{guid}.asset";
        }

        public void AddNode(PCGNodeData nodeData, Vector2 position)
        {
            if (graphData == null) return;

            nodeData.position = position;
            nodeData.GUID = Guid.NewGuid().ToString();

            graphData.nodes.Add(nodeData);
            PersistGraphChange(false);

            CreateNodeFromData(nodeData);
        }

        private void MarkGraphDirty()
        {
            PersistGraphChange(false);
        }

        private void PersistGraphChange(bool saveAssets)
        {
            if (graphData == null)
            {
                return;
            }

            EditorUtility.SetDirty(graphData);
            editorWindow?.MarkDirty();

            if (saveAssets)
            {
                AssetDatabase.SaveAssets();
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            foreach (var port in ports.ToList())
            {
                if (port.direction != startPort.direction &&
                    port.node != startPort.node &&
                    port.portType == startPort.portType)
                {
                    compatiblePorts.Add(port);
                }
            }

            return compatiblePorts;
        }
    }
}

