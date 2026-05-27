namespace PCG.Windows
{
    using PCG.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class PCGEditorWindow : EditorWindow
    {
        private PCGGraphView graphView;
        private PCGGraphData currentGraph;
        private VisualElement mainContainer;
        private Label graphNameLabel;
        private VisualElement emptyStateLabel;

        [MenuItem("Tools/PCG Graph Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<PCGEditorWindow>();
            window.titleContent = new GUIContent("PCG Graph Editor");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        private void OnEnable()
        {
            mainContainer = new VisualElement();
            mainContainer.style.flexGrow = 1;
            mainContainer.style.flexDirection = FlexDirection.Column;
            rootVisualElement.Add(mainContainer);

            CreateToolbar();

            // Load the most recently opened graph, if it still exists.
            string lastGraphPath = EditorPrefs.GetString("PCG_LastGraph", "");
            if (!string.IsNullOrEmpty(lastGraphPath) && System.IO.File.Exists(lastGraphPath))
            {
                var lastGraph = AssetDatabase.LoadAssetAtPath<PCGGraphData>(lastGraphPath);
                if (lastGraph != null)
                {
                    currentGraph = lastGraph;
                    CreateGraphView();
                    return;
                }
            }

            ShowEmptyState();
        }

        private void ClearMainContainer()
        {
            // Remove the active content while keeping the toolbar in place.

            if (graphView != null)
            {
                graphView.Dispose();
                mainContainer.Remove(graphView);
                graphView = null;
            }

            if (emptyStateLabel != null)
            {
                mainContainer.Remove(emptyStateLabel);
                emptyStateLabel = null;
            }
        }

        private void ShowEmptyState()
        {
            ClearMainContainer();

            emptyStateLabel = new Label("No graph loaded.\n\n- Double-click a .asset graph file in Project window\n- Or click Load Graph button");
            emptyStateLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            emptyStateLabel.style.fontSize = 14;
            emptyStateLabel.style.color = Color.gray;
            emptyStateLabel.style.whiteSpace = WhiteSpace.Normal;
            emptyStateLabel.style.flexGrow = 1;
            mainContainer.Add(emptyStateLabel);
        }

        private void CreateToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.height = 30;
            toolbar.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            toolbar.style.paddingLeft = 5;
            toolbar.style.paddingRight = 5;

            toolbar.Add(new Button(LoadGraphFromFile) { text = "Load Graph", style = { marginLeft = 5, marginRight = 5 } });
            toolbar.Add(new Button(CreateNewGraph) { text = "New Graph", style = { marginLeft = 5, marginRight = 5 } });

            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            toolbar.Add(spacer);

            graphNameLabel = new Label();
            graphNameLabel.style.color = Color.white;
            graphNameLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            graphNameLabel.style.marginRight = 10;
            toolbar.Add(graphNameLabel);

            mainContainer.Add(toolbar);
        }

        private void CreateGraphView()
        {
            ClearMainContainer();

            if (currentGraph == null)
            {
                currentGraph = ScriptableObject.CreateInstance<PCGGraphData>();
                currentGraph.name = "Unsaved Graph";
            }

            graphView = new PCGGraphView(currentGraph, this);
            graphView.style.flexGrow = 1;
            mainContainer.Add(graphView);

            UpdateGraphNameLabel();
        }

        private void OnDisable()
        {
            graphView?.Dispose();
            graphView = null;
        }

        private void UpdateGraphNameLabel()
        {
            if (graphNameLabel != null && currentGraph != null)
            {
                graphNameLabel.text = $"Graph: {currentGraph.name}";
            }
        }

        private void CreateNewGraph()
        {
            string path = "Assets/PCG/Graphs";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            string assetPath = $"{path}/NewGraph_{System.Guid.NewGuid()}.asset";
            int counter = 1;
            while (System.IO.File.Exists(assetPath))
            {
                assetPath = $"{path}/NewGraph_{counter}.asset";
                counter++;
            }

            currentGraph = ScriptableObject.CreateInstance<PCGGraphData>();
            currentGraph.name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(currentGraph, assetPath);
            AssetDatabase.SaveAssets();

            EditorPrefs.SetString("PCG_LastGraph", assetPath);

            CreateGraphView();

            Debug.Log($"Created new graph: {assetPath}");
        }

        public void LoadGraph(PCGGraphData graphToLoad)
        {
            if (graphToLoad == null)
            {
                Debug.LogError("Cannot load null graph");
                return;
            }

            currentGraph = graphToLoad;

            string path = AssetDatabase.GetAssetPath(currentGraph);
            EditorPrefs.SetString("PCG_LastGraph", path);

            CreateGraphView();
            titleContent = new GUIContent($"PCG Graph: {currentGraph.name}");

            Debug.Log($"Graph loaded: {currentGraph.name}");
        }

        private void LoadGraphFromFile()
        {
            string path = EditorUtility.OpenFilePanel("Load PCG Graph", "Assets/PCG/Graphs", "asset");
            if (string.IsNullOrEmpty(path)) return;

            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            var loadedGraph = AssetDatabase.LoadAssetAtPath<PCGGraphData>(path);
            if (loadedGraph != null)
            {
                LoadGraph(loadedGraph);
            }
            else
            {
                Debug.LogError($"Failed to load graph from: {path}");
            }
        }
    }
}
