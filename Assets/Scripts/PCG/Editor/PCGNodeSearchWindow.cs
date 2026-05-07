namespace PCG.Editor
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;

    public class PCGNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private PCGGraphView graphView;
        private Texture2D indentationIcon;
        private Vector2 graphCreatePosition;

        public void Initialize(PCGGraphView targetGraphView)
        {
            graphView = targetGraphView;

            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            indentationIcon.Apply();
            indentationIcon.hideFlags = HideFlags.HideAndDontSave;
        }

        public void SetCreatePosition(Vector2 createPosition)
        {
            graphCreatePosition = createPosition;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var searchTree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0)
            };

            var addedGroups = new HashSet<string>();
            foreach (var descriptor in PCGNodeRegistry.Descriptors)
            {
                AddGroupEntries(searchTree, addedGroups, descriptor.CategoryPath);

                int level = string.IsNullOrEmpty(descriptor.CategoryPath)
                    ? 1
                    : descriptor.CategoryPath.Split('/').Length + 1;

                searchTree.Add(new SearchTreeEntry(new GUIContent(descriptor.DisplayName, indentationIcon))
                {
                    level = level,
                    userData = descriptor
                });
            }

            return searchTree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (graphView == null || !(searchTreeEntry.userData is PCGNodeDescriptor descriptor))
            {
                return false;
            }

            graphView.CreateNodeFromDescriptor(descriptor, graphCreatePosition);
            return true;
        }

        private static void AddGroupEntries(List<SearchTreeEntry> searchTree, HashSet<string> addedGroups, string categoryPath)
        {
            if (string.IsNullOrEmpty(categoryPath))
            {
                return;
            }

            var parts = categoryPath.Split('/');
            string currentPath = string.Empty;

            for (int i = 0; i < parts.Length; i++)
            {
                currentPath = string.IsNullOrEmpty(currentPath) ? parts[i] : $"{currentPath}/{parts[i]}";
                if (addedGroups.Add(currentPath))
                {
                    searchTree.Add(new SearchTreeGroupEntry(new GUIContent(parts[i]), i + 1));
                }
            }
        }
    }
}
