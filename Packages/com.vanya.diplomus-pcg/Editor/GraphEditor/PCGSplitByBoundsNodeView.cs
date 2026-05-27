using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace PCG
{
    public class PCGSplitByBoundsNodeView : PCGNodeView
    {
        protected override void CreateParameterFields()
        {
            var splitNode = nodeData as PCGSplitByBoundsNodeData;
            if (splitNode == null)
            {
                base.CreateParameterFields();
                return;
            }

            var regionField = new ObjectField("Region")
            {
                objectType = typeof(PCGRegionBase),
                allowSceneObjects = true,
                value = PCGRegionBase.FindById(splitNode.RegionId)
            };
            regionField.RegisterValueChangedCallback(evt =>
            {
                RecordNodeUndo();

                var region = evt.newValue as PCGRegionBase;
                splitNode.RegionId = region != null ? region.RegionId : string.Empty;
                splitNode.RegionName = region != null ? region.name : string.Empty;
                UpdateNodeData();
            });
            extensionContainer.Add(regionField);
        }

        protected override void UpdateNodeData()
        {
            EditorUtility.SetDirty(nodeData);
            OnNodeChanged?.Invoke();
        }
    }
}
