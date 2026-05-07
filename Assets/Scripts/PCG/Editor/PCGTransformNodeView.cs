using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PCG
{
    public class PCGTransformNodeView : PCGNodeView
    {
        protected override void CreateParameterFields()
        {
            var transformNode = nodeData as PCGTransformNodeData;
            if (transformNode == null)
            {
                base.CreateParameterFields();
                return;
            }

            extensionContainer.Add(CreateVectorRow(
                "Offset",
                transformNode.OffsetX,
                transformNode.OffsetY,
                transformNode.OffsetZ,
                value => transformNode.OffsetX = value,
                value => transformNode.OffsetY = value,
                value => transformNode.OffsetZ = value));

            extensionContainer.Add(CreateVectorRow(
                "Rotation",
                transformNode.RotationX,
                transformNode.RotationY,
                transformNode.RotationZ,
                value => transformNode.RotationX = value,
                value => transformNode.RotationY = value,
                value => transformNode.RotationZ = value));

            extensionContainer.Add(CreateFloatField("Scale Multiplier", transformNode.ScaleMultiplier, value => transformNode.ScaleMultiplier = value));
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
                onChanged(evt.newValue);
                UpdateNodeData();
            });
            container.Add(field);

            return container;
        }

        private FloatField CreateFloatField(string label, float initialValue, System.Action<float> onChanged)
        {
            var field = new FloatField(label)
            {
                value = initialValue
            };

            field.RegisterValueChangedCallback(evt =>
            {
                onChanged(evt.newValue);
                UpdateNodeData();
            });

            return field;
        }

        protected override void UpdateNodeData()
        {
            EditorUtility.SetDirty(nodeData);
            OnNodeChanged?.Invoke();
        }
    }
}
