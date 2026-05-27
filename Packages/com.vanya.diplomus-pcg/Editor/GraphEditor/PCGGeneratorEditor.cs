namespace PCG.Editor
{
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;

    [CustomEditor(typeof(PCGGenerator))]
    public class PCGGeneratorEditor : UnityEditor.Editor
    {
        private readonly BoxBoundsHandle boundsHandle = new BoxBoundsHandle();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            var generator = (PCGGenerator)target;
            using (new EditorGUI.DisabledScope(generator.graph == null))
            {
                if (GUILayout.Button("Generate"))
                {
                    generator.Generate();
                }
            }

            if (generator.graph == null)
            {
                EditorGUILayout.HelpBox("Assign a PCG Graph before generating.", MessageType.Info);
            }
        }

        private void OnSceneGUI()
        {
            var generator = (PCGGenerator)target;

            using (new Handles.DrawingScope(generator.transform.localToWorldMatrix))
            {
                boundsHandle.center = generator.generationBounds.center;
                boundsHandle.size = generator.generationBounds.size;

                EditorGUI.BeginChangeCheck();
                boundsHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(generator, "Resize PCG Generation Bounds");
                    generator.generationBounds = new Bounds(boundsHandle.center, boundsHandle.size);
                    EditorUtility.SetDirty(generator);
                }
            }
        }
    }
}
