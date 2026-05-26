namespace PCG.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(PCGPolygonRegion))]
    public class PCGPolygonRegionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var region = (PCGPolygonRegion)target;
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox($"Region Id: {region.RegionId}", MessageType.None);

            if (GUILayout.Button("Add Point"))
            {
                Undo.RecordObject(region, "Add Polygon Point");
                region.Points.Add(region.Points.Count > 0 ? region.Points[region.Points.Count - 1] + new Vector2(1f, 1f) : Vector2.zero);
                EditorUtility.SetDirty(region);
            }

            using (new EditorGUI.DisabledScope(region.Points.Count <= 3))
            {
                if (GUILayout.Button("Remove Last Point"))
                {
                    Undo.RecordObject(region, "Remove Polygon Point");
                    region.Points.RemoveAt(region.Points.Count - 1);
                    EditorUtility.SetDirty(region);
                }
            }
        }

        private void OnSceneGUI()
        {
            var region = (PCGPolygonRegion)target;
            Transform regionTransform = region.transform;

            using (new Handles.DrawingScope(regionTransform.localToWorldMatrix))
            {
                for (int i = 0; i < region.Points.Count; i++)
                {
                    Vector2 point = region.Points[i];
                    Vector3 localPosition = new Vector3(point.x, region.CenterY, point.y);

                    EditorGUI.BeginChangeCheck();
                    Vector3 newLocalPosition = Handles.PositionHandle(localPosition, Quaternion.identity);
                    newLocalPosition.y = region.CenterY;

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(region, "Move Polygon Point");
                        region.Points[i] = new Vector2(newLocalPosition.x, newLocalPosition.z);
                        EditorUtility.SetDirty(region);
                    }
                }

                float halfHeight = region.Height * 0.5f;
                Vector3 heightHandlePosition = new Vector3(0f, region.CenterY + halfHeight, 0f);

                EditorGUI.BeginChangeCheck();
                Vector3 newHeightHandlePosition = Handles.Slider(heightHandlePosition, Vector3.up);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(region, "Resize Polygon Region");
                    region.Height = Mathf.Max(0.01f, (newHeightHandlePosition.y - region.CenterY) * 2f);
                    EditorUtility.SetDirty(region);
                }
            }
        }
    }
}
