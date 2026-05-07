namespace PCG
{
    using PCG.Editor;
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
        private Dictionary<string, PCGNodeView> nodeDictionary = new Dictionary<string, PCGNodeView>();
        private PCGEditorWindow editorWindow;
        private PCGNodeSearchWindow nodeSearchWindow;
        private bool hasPendingSave;
        private double nextSaveTime;
        private const double SaveDelaySeconds = 0.75;

        public PCGGraphView(PCGGraphData data, PCGEditorWindow window)
        {
            graphData = data;
            editorWindow = window;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            grid.StretchToParentSize();
            Insert(0, grid);

            graphViewChanged += OnGraphViewChanged;
            EditorApplication.update += OnEditorUpdate;
            RegisterCallback<KeyDownEvent>(OnKeyDown);

            nodeSearchWindow = ScriptableObject.CreateInstance<PCGNodeSearchWindow>();
            nodeSearchWindow.Initialize(this);

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
                ScheduleGraphSave();
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
                        ScheduleGraphSave();
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
                            PCGGraphAssetUtility.DeleteNodeAsset(nodeView.nodeData);
                            ScheduleGraphSave();
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
            evt.menu.AppendAction("Add Node...", _ => OpenNodeSearch(mousePosition));
        }

        public void AddNode(PCGNodeData nodeData, Vector2 position)
        {
            if (graphData == null) return;

            nodeData.position = position;
            nodeData.GUID = Guid.NewGuid().ToString();

            PCGGraphAssetUtility.AddNodeToGraph(graphData, nodeData);
            graphData.nodes.Add(nodeData);
            ScheduleGraphSave();

            CreateNodeFromData(nodeData);
        }

        public void CreateNodeFromDescriptor(PCGNodeDescriptor descriptor, Vector2 position)
        {
            if (descriptor == null)
            {
                return;
            }

            CreateNode(descriptor.NodeType, position, descriptor.DisplayName);
        }

        private void CreateNode(Type nodeType, Vector2 position, string displayName)
        {
            var nodeData = ScriptableObject.CreateInstance(nodeType) as PCGNodeData;
            if (nodeData == null)
            {
                Debug.LogError($"Failed to create node for type {nodeType?.Name}");
                return;
            }

            nodeData.nodeName = displayName;
            nodeData.GUID = Guid.NewGuid().ToString();
            nodeData.position = position;

            if (graphData != null)
            {
                PCGGraphAssetUtility.AddNodeToGraph(graphData, nodeData);
                graphData.nodes.Add(nodeData);
                ScheduleGraphSave();
            }

            CreateNodeFromData(nodeData);
        }

        private void OpenNodeSearch(Vector2 graphMousePosition)
        {
            if (nodeSearchWindow == null || editorWindow == null)
            {
                return;
            }

            nodeSearchWindow.SetCreatePosition(ConvertToContentPosition(graphMousePosition));

            Vector2 windowPosition = this.ChangeCoordinatesTo(editorWindow.rootVisualElement, graphMousePosition);
            Vector2 screenMousePosition = editorWindow.position.position + windowPosition;
            SearchWindow.Open(new SearchWindowContext(screenMousePosition), nodeSearchWindow);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Space)
            {
                return;
            }

            Vector2 centerPosition = new Vector2(layout.width * 0.5f, layout.height * 0.5f);
            OpenNodeSearch(centerPosition);
            evt.StopPropagation();
        }

        private Vector2 ConvertToContentPosition(Vector2 graphViewLocalPosition)
        {
            return this.ChangeCoordinatesTo(contentViewContainer, graphViewLocalPosition);
        }

        private void MarkGraphDirty()
        {
            ScheduleGraphSave();
        }

        public void FlushPendingSave()
        {
            if (!hasPendingSave)
            {
                return;
            }

            SaveGraphAssets();
        }

        public void Dispose()
        {
            FlushPendingSave();
            EditorApplication.update -= OnEditorUpdate;
            UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void ScheduleGraphSave()
        {
            if (graphData == null)
            {
                return;
            }

            EditorUtility.SetDirty(graphData);
            foreach (var node in graphData.nodes)
            {
                if (node != null)
                {
                    EditorUtility.SetDirty(node);
                }
            }

            hasPendingSave = true;
            nextSaveTime = EditorApplication.timeSinceStartup + SaveDelaySeconds;
        }

        private void OnEditorUpdate()
        {
            if (!hasPendingSave || EditorApplication.timeSinceStartup < nextSaveTime)
            {
                return;
            }

            SaveGraphAssets();
        }

        private void SaveGraphAssets()
        {
            if (graphData == null)
            {
                hasPendingSave = false;
                return;
            }

            EditorUtility.SetDirty(graphData);
            foreach (var node in graphData.nodes)
            {
                if (node != null)
                {
                    EditorUtility.SetDirty(node);
                }
            }

            AssetDatabase.SaveAssets();
            hasPendingSave = false;
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


