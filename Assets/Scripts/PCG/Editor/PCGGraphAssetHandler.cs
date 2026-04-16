using PCG.Windows;
using UnityEditor;
using UnityEngine;

namespace PCG.Editor
{
    /// <summary>
    /// Обработчик двойного клика по ассету PCGGraphData
    /// </summary>
    public class PCGGraphAssetHandler
    {
        [UnityEditor.Callbacks.OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);

            if (obj is PCGGraphData graph)
            {
                var window = EditorWindow.GetWindow<PCGEditorWindow>();
                window.titleContent = new GUIContent($"PCG Graph: {graph.name}");
                window.LoadGraph(graph);
                window.Show();
                return true;
            }

            return false;
        }
    }
}