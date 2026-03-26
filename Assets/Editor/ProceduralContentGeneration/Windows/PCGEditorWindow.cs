using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace PCG.Windows 
{
    using Utilities;
    public class PCGEditorWindow : EditorWindow
    {
        [MenuItem("Window/PCG/PCG Graph")]
        public static void ShowExample()
        {
            PCGEditorWindow wnd = GetWindow<PCGEditorWindow>("PCG Graph");
        }

        private void OnEnable()
        {
            AddGraphView();

            AddStyles();
        }

        #region Elements Addition
        private void AddGraphView()
        {
            PCGGraphView graphView = new PCGGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets(
                "ProceduralContentGeneration/PCGVariables.uss"
                );
        }
        #endregion
    }
}

