using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PCG
{
    public class PCGBoundsFilterNodeView : PCGNodeView
    {
        protected override void CreateParameterFields()
        {
            var boundsNode = nodeData as PCGBoundsFilterNodeData;
            if (boundsNode == null)
            {
                base.CreateParameterFields();
                return;
            }

            var regionField = new ObjectField("Region")
            {
                objectType = typeof(PCGBoxRegion),
                allowSceneObjects = true,
                value = PCGBoxRegion.FindById(boundsNode.RegionId)
            };
            regionField.RegisterValueChangedCallback(evt =>
            {
                RecordNodeUndo();

                var region = evt.newValue as PCGBoxRegion;
                boundsNode.RegionId = region != null ? region.RegionId : string.Empty;
                boundsNode.RegionName = region != null ? region.name : string.Empty;
                UpdateNodeData();
            });
            extensionContainer.Add(regionField);

            var invertField = new Toggle("Invert")
            {
                value = boundsNode.Invert
            };
            invertField.RegisterValueChangedCallback(evt =>
            {
                RecordNodeUndo();
                boundsNode.Invert = evt.newValue;
                UpdateNodeData();
            });
            extensionContainer.Add(invertField);

            var infoLabel = new Label("Edit the selected region directly in Scene View.");
            infoLabel.style.whiteSpace = WhiteSpace.Normal;
            infoLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            infoLabel.style.marginTop = 4;
            extensionContainer.Add(infoLabel);
        }

        protected override void UpdateNodeData()
        {
            EditorUtility.SetDirty(nodeData);
            OnNodeChanged?.Invoke();
        }
    }
}
