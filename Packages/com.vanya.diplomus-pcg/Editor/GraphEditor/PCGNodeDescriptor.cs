namespace PCG.Editor
{
    using System;

    public class PCGNodeDescriptor
    {
        public string DisplayName { get; }
        public string CategoryPath { get; }
        public Type NodeType { get; }

        public PCGNodeDescriptor(string displayName, string categoryPath, Type nodeType)
        {
            DisplayName = displayName;
            CategoryPath = categoryPath;
            NodeType = nodeType;
        }
    }
}
