namespace PCG.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(PCGCylinderRegion))]
    public class PCGCylinderRegionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var region = (PCGCylinderRegion)target;
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox($"Region Id: {region.RegionId}", MessageType.None);
        }

        private void OnSceneGUI()
        {
            var region = (PCGCylinderRegion)target;
            Transform regionTransform = region.transform;

            using (new Handles.DrawingScope(regionTransform.localToWorldMatrix))
            {
                EditorGUI.BeginChangeCheck();

                Vector3 center = region.Center;
                float height = region.Height;
                float radius = region.Radius;
                float halfHeight = height * 0.5f;

                Vector3 topCenter = center + Vector3.up * halfHeight;
                Vector3 bottomCenter = center - Vector3.up * halfHeight;
                Vector3 radiusHandlePosition = center + Vector3.right * radius;

                Vector3 newTopCenter = Handles.Slider(topCenter, Vector3.up);
                Vector3 newBottomCenter = Handles.Slider(bottomCenter, Vector3.down);
                Vector3 newRadiusHandlePosition = Handles.Slider(radiusHandlePosition, Vector3.right);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(region, "Edit PCG Cylinder Region");
                    region.Center = new Vector3(center.x, (newTopCenter.y + newBottomCenter.y) * 0.5f, center.z);
                    region.Height = Mathf.Abs(newTopCenter.y - newBottomCenter.y);
                    region.Radius = Mathf.Abs(newRadiusHandlePosition.x - region.Center.x);
                    EditorUtility.SetDirty(region);
                }
            }
        }
    }
}
