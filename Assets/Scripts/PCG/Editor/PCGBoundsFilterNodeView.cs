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

            extensionContainer.Add(CreateVectorRow(
                "Center",
                boundsNode.CenterX,
                boundsNode.CenterY,
                boundsNode.CenterZ,
                value => boundsNode.CenterX = value,
                value => boundsNode.CenterY = value,
                value => boundsNode.CenterZ = value));

            extensionContainer.Add(CreateVectorRow(
                "Size",
                boundsNode.SizeX,
                boundsNode.SizeY,
                boundsNode.SizeZ,
                value => boundsNode.SizeX = value,
                value => boundsNode.SizeY = value,
                value => boundsNode.SizeZ = value));

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
        }

        private VisualElement CreateVectorRow(
            string labelText,
            float xValue,
            float yValue,
            float zValue,
            System.Action<float> onXChanged,
            System.Action<float> onYChanged,
            System.Action<float> onZChanged)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 4;

            var label = new Label(labelText);
            label.style.width = 55;
            row.Add(label);

            row.Add(CreateAxisField("X", xValue, onXChanged));
            row.Add(CreateAxisField("Y", yValue, onYChanged));
            row.Add(CreateAxisField("Z", zValue, onZChanged));

            return row;
        }

        private VisualElement CreateAxisField(string axisLabel, float initialValue, System.Action<float> onChanged)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;
            container.style.flexGrow = 1;

            var label = new Label(axisLabel);
            label.style.width = 12;
            label.style.marginLeft = 4;
            container.Add(label);

            var field = new FloatField
            {
                value = initialValue
            };
            field.style.flexGrow = 1;
            field.RegisterValueChangedCallback(evt =>
            {
                RecordNodeUndo();
                onChanged(evt.newValue);
                UpdateNodeData();
            });
            container.Add(field);

            return container;
        }

        protected override void UpdateNodeData()
        {
            EditorUtility.SetDirty(nodeData);
            OnNodeChanged?.Invoke();
        }
    }
}
