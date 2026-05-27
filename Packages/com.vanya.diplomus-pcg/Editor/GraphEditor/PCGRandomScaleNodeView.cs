using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PCG
{
    public class PCGRandomScaleNodeView : PCGNodeView
    {
        private VisualElement modeFieldsContainer;

        protected override void CreateParameterFields()
        {
            var scaleNode = nodeData as PCGRandomScaleNodeData;
            if (scaleNode == null)
            {
                base.CreateParameterFields();
                return;
            }

            var modeField = new EnumField("Scale Mode", scaleNode.Mode);
            modeField.Init(scaleNode.Mode);
            modeField.RegisterValueChangedCallback(evt =>
            {
                RecordNodeUndo();
                scaleNode.Mode = (PCGRandomScaleNodeData.ScaleMode)evt.newValue;
                RebuildModeFields(scaleNode);
                UpdateNodeData();
            });
            extensionContainer.Add(modeField);

            modeFieldsContainer = new VisualElement();
            extensionContainer.Add(modeFieldsContainer);

            RebuildModeFields(scaleNode);
        }

        private void RebuildModeFields(PCGRandomScaleNodeData scaleNode)
        {
            modeFieldsContainer.Clear();

            if (scaleNode.Mode == PCGRandomScaleNodeData.ScaleMode.Uniform)
            {
                modeFieldsContainer.Add(CreateRangeRow(
                    "Scale",
                    scaleNode.UniformMin,
                    scaleNode.UniformMax,
                    value => scaleNode.UniformMin = value,
                    value => scaleNode.UniformMax = value));
            }
            else
            {
                modeFieldsContainer.Add(CreateRangeRow(
                    "Scale X",
                    scaleNode.MinScaleX,
                    scaleNode.MaxScaleX,
                    value => scaleNode.MinScaleX = value,
                    value => scaleNode.MaxScaleX = value));
                modeFieldsContainer.Add(CreateRangeRow(
                    "Scale Y",
                    scaleNode.MinScaleY,
                    scaleNode.MaxScaleY,
                    value => scaleNode.MinScaleY = value,
                    value => scaleNode.MaxScaleY = value));
                modeFieldsContainer.Add(CreateRangeRow(
                    "Scale Z",
                    scaleNode.MinScaleZ,
                    scaleNode.MaxScaleZ,
                    value => scaleNode.MinScaleZ = value,
                    value => scaleNode.MaxScaleZ = value));
            }

            RefreshExpandedState();
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
            label.style.width = 55;
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
