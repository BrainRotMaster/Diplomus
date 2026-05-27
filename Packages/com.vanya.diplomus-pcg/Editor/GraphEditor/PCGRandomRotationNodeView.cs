using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PCG
{
    public class PCGRandomRotationNodeView : PCGNodeView
    {
        protected override void CreateParameterFields()
        {
            var rotationNode = nodeData as PCGRandomRotationNodeData;
            if (rotationNode == null)
            {
                base.CreateParameterFields();
                return;
            }

            extensionContainer.Add(CreateRangeRow(
                "Rotation X",
                rotationNode.MinRotationX,
                rotationNode.MaxRotationX,
                value => rotationNode.MinRotationX = value,
                value => rotationNode.MaxRotationX = value));
            extensionContainer.Add(CreateRangeRow(
                "Rotation Y",
                rotationNode.MinRotationY,
                rotationNode.MaxRotationY,
                value => rotationNode.MinRotationY = value,
                value => rotationNode.MaxRotationY = value));
            extensionContainer.Add(CreateRangeRow(
                "Rotation Z",
                rotationNode.MinRotationZ,
                rotationNode.MaxRotationZ,
                value => rotationNode.MinRotationZ = value,
                value => rotationNode.MaxRotationZ = value));
        }

        private VisualElement CreateRangeRow(
            string labelText,
            float minValue,
            float maxValue,
            System.Action<float> onMinChanged,
            System.Action<float> onMaxChanged)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 4;

            var label = new Label(labelText);
            label.style.width = 70;
            row.Add(label);

            var minField = CreateFloatField(minValue, onMinChanged);
            minField.style.flexGrow = 1;
            row.Add(minField);

            var toLabel = new Label("to");
            toLabel.style.marginLeft = 4;
            toLabel.style.marginRight = 4;
            row.Add(toLabel);

            var maxField = CreateFloatField(maxValue, onMaxChanged);
            maxField.style.flexGrow = 1;
            row.Add(maxField);

            return row;
        }

        private FloatField CreateFloatField(float initialValue, System.Action<float> onChanged)
        {
            var field = new FloatField
            {
                value = initialValue
            };

            field.RegisterValueChangedCallback(evt =>
            {
                RecordNodeUndo();
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
