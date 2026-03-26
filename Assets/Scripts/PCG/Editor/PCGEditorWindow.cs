namespace PCG.Windows 
{
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class PCGEditorWindow : EditorWindow
    {
        private PCGGraphView graphView;
        private PCGGraphData currentGraph;
        private VisualElement mainContainer;
        private VisualElement toolbarContainer;
        private Label graphNameLabel;
        private bool isDirty;

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

            if (currentGraph == null)
            {
                CreateNewGraph();
            }
            else
            {
                CreateGraphView();
            }

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            if (isDirty && currentGraph != null)
            {
                SaveGraph();
            }
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode && isDirty)
            {
                SaveGraph();
            }
        }

        private void CreateToolbar()
        {
            if (toolbarContainer != null)
            {
                mainContainer.Remove(toolbarContainer);
            }

            toolbarContainer = new VisualElement();
            toolbarContainer.style.flexDirection = FlexDirection.Row;
            toolbarContainer.style.height = 30;
            toolbarContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            toolbarContainer.style.paddingLeft = 5;
            toolbarContainer.style.paddingRight = 5;

            var saveButton = new Button(() => SaveGraph());
            saveButton.text = "Save Graph";
            saveButton.style.marginLeft = 5;
            saveButton.style.marginRight = 5;
            toolbarContainer.Add(saveButton);

            var loadButton = new Button(() => LoadGraph());
            loadButton.text = "Load Graph";
            loadButton.style.marginLeft = 5;
            loadButton.style.marginRight = 5;
            toolbarContainer.Add(loadButton);

            var newButton = new Button(() => NewGraph());
            newButton.text = "New Graph";
            newButton.style.marginLeft = 5;
            newButton.style.marginRight = 5;
            toolbarContainer.Add(newButton);

            var generateButton = new Button(() => Generate());
            generateButton.text = "Generate";
            generateButton.style.marginLeft = 5;
            generateButton.style.marginRight = 5;
            generateButton.style.backgroundColor = new Color(0.2f, 0.5f, 0.2f);
            toolbarContainer.Add(generateButton);

            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            toolbarContainer.Add(spacer);

            graphNameLabel = new Label();
            graphNameLabel.style.color = Color.white;
            graphNameLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            graphNameLabel.style.marginRight = 10;
            UpdateGraphNameLabel();
            toolbarContainer.Add(graphNameLabel);

            mainContainer.Add(toolbarContainer);
        }

        private void CreateGraphView()
        {
            if (graphView != null)
            {
                mainContainer.Remove(graphView);
            }

            if (currentGraph == null)
            {
                currentGraph = ScriptableObject.CreateInstance<PCGGraphData>();
                currentGraph.name = "New PCG Graph";
            }

            graphView = new PCGGraphView(currentGraph, this);
            graphView.style.flexGrow = 1;
            mainContainer.Add(graphView);

            UpdateGraphNameLabel();
        }

        private void UpdateGraphNameLabel()
        {
            if (graphNameLabel != null)
            {
                if (currentGraph != null)
                {
                    graphNameLabel.text = $"Graph: {currentGraph.name} {(isDirty ? "*" : "")}";
                }
                else
                {
                    graphNameLabel.text = "Graph: None";
                }
            }
        }

        public void MarkDirty()
        {
            isDirty = true;
            UpdateGraphNameLabel();
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

            isDirty = false;
            CreateGraphView();

            Debug.Log($"Created new graph: {assetPath}");
        }

        private void SaveGraph()
        {
            if (currentGraph != null)
            {
                EditorUtility.SetDirty(currentGraph);

                // Сохраняем все ноды
                foreach (var node in currentGraph.nodes)
                {
                    EditorUtility.SetDirty(node);
                }

                AssetDatabase.SaveAssets();
                isDirty = false;
                UpdateGraphNameLabel();
                Debug.Log($"Graph saved: {currentGraph.name}");
            }
            else
            {
                Debug.LogWarning("No graph to save!");
            }
        }

        private void LoadGraph()
        {
            string path = EditorUtility.OpenFilePanel("Load PCG Graph", "Assets/PCG/Graphs", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }

                var loadedGraph = AssetDatabase.LoadAssetAtPath<PCGGraphData>(path);
                if (loadedGraph != null)
                {
                    currentGraph = loadedGraph;
                    isDirty = false;
                    CreateGraphView();
                    Debug.Log($"Graph loaded: {currentGraph.name}");
                }
                else
                {
                    Debug.LogError($"Failed to load graph from: {path}");
                }
            }
        }

        private void NewGraph()
        {
            CreateNewGraph();
        }

        private void Generate()
        {
            if (currentGraph == null)
            {
                Debug.LogError("No graph to generate!");
                return;
            }

            // Сохраняем перед генерацией
            if (isDirty)
            {
                SaveGraph();
            }

            var generator = FindObjectOfType<PCGGenerator>();
            if (generator == null)
            {
                var go = new GameObject("PCG Generator");
                generator = go.AddComponent<PCGGenerator>();
                generator.graph = currentGraph;
                Debug.Log("Created PCGGenerator component. Adjust bounds in inspector and click Generate again.");
                Selection.activeObject = generator.gameObject;
            }
            else
            {
                generator.graph = currentGraph;
                generator.Generate();
            }
        }
    }
}

