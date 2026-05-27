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
        private const string ClipboardDataType = "PCGGraphClipboard";
        private static readonly Vector2 PasteOffsetStep = new Vector2(30f, 30f);
        private int pasteOperationCount;
        private string lastPastedSerializedData;

        [Serializable]
        private class ClipboardPayload
        {
            public string type = ClipboardDataType;
            public List<ClipboardNodePayload> nodes = new List<ClipboardNodePayload>();
            public List<ClipboardEdgePayload> edges = new List<ClipboardEdgePayload>();
        }

        [Serializable]
        private class ClipboardNodePayload
        {
            public string sourceGuid;
            public string typeName;
            public string json;
        }

        [Serializable]
        private class ClipboardEdgePayload
        {
            public string sourceGuid;
            public string sourcePortName;
            public string targetGuid;
            public string targetPortName;
        }

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
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            serializeGraphElements = SerializeGraphElements;
            canPasteSerializedData = CanPasteSerializedData;
            unserializeAndPaste = UnserializeAndPaste;

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
            var nodesToCreate = new List<PCGNodeData>(graphData.nodes.Where(node => node != null));
            var validGuids = new HashSet<string>(
                nodesToCreate
                    .Where(node => !string.IsNullOrEmpty(node.GUID))
                    .Select(node => node.GUID));

            foreach (var nodeData in nodesToCreate)
            {
                CreateNodeFromData(nodeData);
            }

            var edgesToCreate = new List<PCGEdgeData>(graphData.edges.Where(edge =>
                edge != null &&
                !string.IsNullOrEmpty(edge.sourceNodeGUID) &&
                !string.IsNullOrEmpty(edge.targetNodeGUID) &&
                validGuids.Contains(edge.sourceNodeGUID) &&
                validGuids.Contains(edge.targetNodeGUID)));

            foreach (var edgeData in edgesToCreate)
            {
                var sourceNode = GetNodeByGUID(edgeData.sourceNodeGUID);
                var targetNode = GetNodeByGUID(edgeData.targetNodeGUID);

                if (sourceNode != null && targetNode != null)
                {
                    ConnectNodes(
                        sourceNode,
                        targetNode,
                        edgeData.sourcePortName,
                        edgeData.targetPortName);
                }
                else
                {
                    Debug.LogWarning($"Cannot create edge: source={edgeData.sourceNodeGUID}, target={edgeData.targetNodeGUID}");
                }
            }
        }

        private void CreateNodeFromData(PCGNodeData nodeData)
        {
            if (nodeData == null)
            {
                return;
            }

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
            return ConnectNodes(fromNode, toNode, PCGNodeData.DefaultOutputPortName, PCGNodeData.DefaultInputPortName);
        }

        public Edge ConnectNodes(PCGNodeView fromNode, PCGNodeView toNode, string sourcePortName, string targetPortName)
        {
            Port outputPort = fromNode.GetOutputPort(sourcePortName);
            Port inputPort = toNode.GetInputPort(targetPortName);

            if (outputPort == null || inputPort == null)
            {
                Debug.LogWarning($"Cannot connect: ports missing. FromNode={fromNode?.title}, ToNode={toNode?.title}, SourcePort={sourcePortName}, TargetPort={targetPortName}");
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

            SaveEdgeData(fromNode.GUID, toNode.GUID, outputPort.portName, inputPort.portName, "Connect PCG Nodes");

            return edge;
        }

        private void SaveEdgeData(string sourceGUID, string targetGUID, string sourcePortName, string targetPortName, string undoActionName = "Edit PCG Edge")
        {
            if (graphData == null) return;

            sourcePortName = string.IsNullOrEmpty(sourcePortName) ? PCGNodeData.DefaultOutputPortName : sourcePortName;
            targetPortName = string.IsNullOrEmpty(targetPortName) ? PCGNodeData.DefaultInputPortName : targetPortName;

            // Avoid duplicating serialized edges when the view reconnects an existing link.
            bool exists = graphData.edges.Any(e =>
                e.sourceNodeGUID == sourceGUID &&
                e.targetNodeGUID == targetGUID &&
                NormalizePortName(e.sourcePortName, PCGNodeData.DefaultOutputPortName) == sourcePortName &&
                NormalizePortName(e.targetPortName, PCGNodeData.DefaultInputPortName) == targetPortName);

            if (!exists)
            {
                Undo.RecordObject(graphData, undoActionName);
                var edgeData = new PCGEdgeData
                {
                    sourceNodeGUID = sourceGUID,
                    sourcePortName = sourcePortName,
                    targetNodeGUID = targetGUID,
                    targetPortName = targetPortName
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
                        e.targetNodeGUID == targetNode.GUID &&
                        NormalizePortName(e.sourcePortName, PCGNodeData.DefaultOutputPortName) == NormalizePortName(edge.output.portName, PCGNodeData.DefaultOutputPortName) &&
                        NormalizePortName(e.targetPortName, PCGNodeData.DefaultInputPortName) == NormalizePortName(edge.input.portName, PCGNodeData.DefaultInputPortName));

                    if (edgeToRemove != null)
                    {
                        Undo.RecordObject(graphData, "Disconnect PCG Nodes");
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
                            Undo.RecordObject(graphData, "Delete PCG Node");
                            graphData.nodes.Remove(nodeView.nodeData);
                            nodeDictionary.Remove(nodeView.GUID);
                            RemoveEdgesForNode(nodeView.GUID);
                            PCGGraphAssetUtility.DeleteNodeAsset(nodeView.nodeData, true);
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
                        SaveEdgeData(
                            fromNode.GUID,
                            toNode.GUID,
                            edge.output.portName,
                            edge.input.portName,
                            "Connect PCG Nodes");
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
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("Create PCG Node");
                int undoGroup = Undo.GetCurrentGroup();

                Undo.RecordObject(graphData, "Create PCG Node");
                PCGGraphAssetUtility.AddNodeToGraph(graphData, nodeData);
                Undo.RegisterCreatedObjectUndo(nodeData, "Create PCG Node");
                graphData.nodes.Add(nodeData);
                ScheduleGraphSave();
                Undo.CollapseUndoOperations(undoGroup);
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

        private new string SerializeGraphElements(IEnumerable<GraphElement> elements)
        {
            var selectedNodeViews = elements
                .OfType<PCGNodeView>()
                .Where(nodeView => nodeView.nodeData != null)
                .Distinct()
                .ToList();

            if (selectedNodeViews.Count == 0)
            {
                return string.Empty;
            }

            var selectedGuids = new HashSet<string>(selectedNodeViews.Select(nodeView => nodeView.GUID));
            var payload = new ClipboardPayload();

            foreach (var nodeView in selectedNodeViews)
            {
                payload.nodes.Add(new ClipboardNodePayload
                {
                    sourceGuid = nodeView.GUID,
                    typeName = nodeView.nodeData.GetType().AssemblyQualifiedName,
                    json = EditorJsonUtility.ToJson(nodeView.nodeData)
                });
            }

            foreach (var edgeData in graphData.edges)
            {
                if (edgeData == null)
                {
                    continue;
                }

                if (selectedGuids.Contains(edgeData.sourceNodeGUID) && selectedGuids.Contains(edgeData.targetNodeGUID))
                {
                    payload.edges.Add(new ClipboardEdgePayload
                    {
                        sourceGuid = edgeData.sourceNodeGUID,
                        sourcePortName = NormalizePortName(edgeData.sourcePortName, PCGNodeData.DefaultOutputPortName),
                        targetGuid = edgeData.targetNodeGUID,
                        targetPortName = NormalizePortName(edgeData.targetPortName, PCGNodeData.DefaultInputPortName)
                    });
                }
            }

            return JsonUtility.ToJson(payload);
        }

        private new bool CanPasteSerializedData(string serializedData)
        {
            if (string.IsNullOrEmpty(serializedData))
            {
                return false;
            }

            try
            {
                var payload = JsonUtility.FromJson<ClipboardPayload>(serializedData);
                return payload != null &&
                    payload.type == ClipboardDataType &&
                    payload.nodes != null &&
                    payload.nodes.Count > 0;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private void UnserializeAndPaste(string operationName, string serializedData)
        {
            if (graphData == null || string.IsNullOrEmpty(serializedData))
            {
                return;
            }

            ClipboardPayload payload;
            try
            {
                payload = JsonUtility.FromJson<ClipboardPayload>(serializedData);
            }
            catch (ArgumentException)
            {
                return;
            }

            if (payload == null || payload.type != ClipboardDataType || payload.nodes == null || payload.nodes.Count == 0)
            {
                return;
            }

            if (!string.Equals(lastPastedSerializedData, serializedData, StringComparison.Ordinal))
            {
                pasteOperationCount = 0;
                lastPastedSerializedData = serializedData;
            }

            pasteOperationCount++;
            Vector2 pasteOffset = PasteOffsetStep * pasteOperationCount;
            var guidMap = new Dictionary<string, string>();
            var pastedNodeViews = new List<PCGNodeView>();
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Paste PCG Nodes");
            int undoGroup = Undo.GetCurrentGroup();

            foreach (var nodePayload in payload.nodes)
            {
                Type nodeType = ResolveType(nodePayload.typeName);
                if (nodeType == null)
                {
                    Debug.LogWarning($"Cannot paste node because type was not found: {nodePayload.typeName}");
                    continue;
                }

                var pastedNodeData = ScriptableObject.CreateInstance(nodeType) as PCGNodeData;
                if (pastedNodeData == null)
                {
                    Debug.LogWarning($"Cannot paste node because type is not a PCG node: {nodePayload.typeName}");
                    continue;
                }

                EditorJsonUtility.FromJsonOverwrite(nodePayload.json, pastedNodeData);
                pastedNodeData.GUID = Guid.NewGuid().ToString();
                pastedNodeData.position += pasteOffset;

                Undo.RecordObject(graphData, "Paste PCG Nodes");
                PCGGraphAssetUtility.AddNodeToGraph(graphData, pastedNodeData);
                Undo.RegisterCreatedObjectUndo(pastedNodeData, "Paste PCG Nodes");
                graphData.nodes.Add(pastedNodeData);
                CreateNodeFromData(pastedNodeData);

                guidMap[nodePayload.sourceGuid] = pastedNodeData.GUID;

                var pastedNodeView = GetNodeByGUID(pastedNodeData.GUID);
                if (pastedNodeView != null)
                {
                    pastedNodeViews.Add(pastedNodeView);
                }
            }

            foreach (var edgePayload in payload.edges)
            {
                if (!guidMap.TryGetValue(edgePayload.sourceGuid, out var pastedSourceGuid) ||
                    !guidMap.TryGetValue(edgePayload.targetGuid, out var pastedTargetGuid))
                {
                    continue;
                }

                var sourceNode = GetNodeByGUID(pastedSourceGuid);
                var targetNode = GetNodeByGUID(pastedTargetGuid);
                if (sourceNode != null && targetNode != null)
                {
                    ConnectNodes(sourceNode, targetNode, edgePayload.sourcePortName, edgePayload.targetPortName);
                }
            }

            ClearSelection();
            foreach (var pastedNodeView in pastedNodeViews)
            {
                AddToSelection(pastedNodeView);
            }

            ScheduleGraphSave();
            Undo.CollapseUndoOperations(undoGroup);
        }

        private static Type ResolveType(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName))
            {
                return null;
            }

            Type resolvedType = Type.GetType(assemblyQualifiedName);
            if (resolvedType != null)
            {
                return resolvedType;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                resolvedType = assembly.GetType(assemblyQualifiedName);
                if (resolvedType != null)
                {
                    return resolvedType;
                }
            }

            return null;
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
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void RemoveEdgesForNode(string nodeGuid)
        {
            if (graphData == null || string.IsNullOrEmpty(nodeGuid))
            {
                return;
            }

            graphData.edges.RemoveAll(edge =>
                edge != null &&
                (edge.sourceNodeGUID == nodeGuid || edge.targetNodeGUID == nodeGuid));
        }

        private static string NormalizePortName(string portName, string fallback)
        {
            return string.IsNullOrEmpty(portName) ? fallback : portName;
        }

        private void OnUndoRedoPerformed()
        {
            pasteOperationCount = 0;
            RebuildViewFromGraph();
        }

        private void RebuildViewFromGraph()
        {
            graphViewChanged -= OnGraphViewChanged;

            foreach (var element in graphElements.ToList())
            {
                RemoveElement(element);
            }

            nodeDictionary.Clear();
            graphViewChanged += OnGraphViewChanged;

            if (graphData != null)
            {
                LoadNodes();
            }
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


