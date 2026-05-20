namespace PCG.Editor
{
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;

    [CustomEditor(typeof(PCGBoxRegion))]
    public class PCGBoxRegionEditor : UnityEditor.Editor
    {
        private readonly BoxBoundsHandle boundsHandle = new BoxBoundsHandle();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var region = (PCGBoxRegion)target;
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox($"Region Id: {region.RegionId}", MessageType.None);
        }

        private void OnSceneGUI()
        {
            var region = (PCGBoxRegion)target;

            using (new Handles.DrawingScope(region.transform.localToWorldMatrix))
            {
                boundsHandle.center = region.Center;
                boundsHandle.size = region.Size;

                EditorGUI.BeginChangeCheck();
                boundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(region, "Resize PCG Box Region");
                    region.Center = boundsHandle.center;
                    region.Size = boundsHandle.size;
                    EditorUtility.SetDirty(region);
                }
            }
        }
    }
}
