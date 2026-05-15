namespace PCG.Editor
{
    using UnityEditor;
    using UnityEngine;

    public static class PCGGraphAssetUtility
    {
        public static void AddNodeToGraph(PCGGraphData graphData, PCGNodeData nodeData)
        {
            if (graphData == null || nodeData == null)
            {
                return;
            }

            AssetDatabase.AddObjectToAsset(nodeData, graphData);
            EditorUtility.SetDirty(nodeData);
            EditorUtility.SetDirty(graphData);
        }

        public static void DeleteNodeAsset(PCGNodeData nodeData, bool useUndo = false)
        {
            if (nodeData == null)
            {
                return;
            }

            if (AssetDatabase.Contains(nodeData))
            {
                if (useUndo)
                {
                    Undo.DestroyObjectImmediate(nodeData);
                }
                else
                {
                    Object.DestroyImmediate(nodeData, true);
                }

                return;
            }

            if (useUndo)
            {
                Undo.DestroyObjectImmediate(nodeData);
            }
            else
            {
                Object.DestroyImmediate(nodeData);
            }
        }
    }
}
