namespace PCG.Editor
{
    using System;
    using System.Collections.Generic;

    public static class PCGNodeRegistry
    {
        private static readonly List<PCGNodeDescriptor> descriptors = new List<PCGNodeDescriptor>
        {
            new PCGNodeDescriptor("Source Node", "Source", typeof(PCGSourceNodeData)),
            new PCGNodeDescriptor("Filter Node", "Filter", typeof(PCGFilterNodeData)),
            new PCGNodeDescriptor("Distance Filter Node", "Filter", typeof(PCGDistanceFilterNodeData)),
            new PCGNodeDescriptor("Transform Node", "Transform", typeof(PCGTransformNodeData)),
            new PCGNodeDescriptor("Project To Surface Node", "Transform", typeof(PCGProjectToSurfaceNodeData)),
            new PCGNodeDescriptor("Random Rotation Node", "Transform", typeof(PCGRandomRotationNodeData)),
            new PCGNodeDescriptor("Random Scale Node", "Transform", typeof(PCGRandomScaleNodeData)),
            new PCGNodeDescriptor("Random Offset Node", "Transform", typeof(PCGRandomOffsetNodeData)),
            new PCGNodeDescriptor("Density Noise Node", "Attributes", typeof(PCGDensityNoiseNodeData)),
            new PCGNodeDescriptor("Attribute Set Node", "Attributes", typeof(PCGAttributeSetNodeData)),
            new PCGNodeDescriptor("Merge Node", "Utility", typeof(PCGMergeNodeData)),
            new PCGNodeDescriptor("Spawner Node", "Spawn", typeof(PCGSpawnerNodeData))
        };

        public static IReadOnlyList<PCGNodeDescriptor> Descriptors => descriptors;

        public static PCGNodeDescriptor GetByType(Type nodeType)
        {
            foreach (var descriptor in descriptors)
            {
                if (descriptor.NodeType == nodeType)
                {
                    return descriptor;
                }
            }

            return null;
        }
    }
}
