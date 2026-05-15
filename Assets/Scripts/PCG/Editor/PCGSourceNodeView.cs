using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PCG
{
    public class PCGSourceNodeView : PCGNodeView
    {
        private VisualElement modeFieldsContainer;

        protected override void CreateParameterFields()
        {
            var sourceNode = nodeData as PCGSourceNodeData;
            if (sourceNode == null)
            {
                base.CreateParameterFields();
                return;
            }

            var sourceTypeField = new EnumField("Source Type", sourceNode.SourceTypeValue);
            sourceTypeField.Init(sourceNode.SourceTypeValue);
            sourceTypeField.RegisterValueChangedCallback(evt =>
            {
                RecordNodeUndo();
                sourceNode.SourceTypeValue = (PCGSourceNodeData.SourceType)evt.newValue;
                RebuildModeFields(sourceNode);
                UpdateNodeData();
            });
            extensionContainer.Add(sourceTypeField);

            modeFieldsContainer = new VisualElement();
            extensionContainer.Add(modeFieldsContainer);

            RebuildModeFields(sourceNode);
        }

        private void RebuildModeFields(PCGSourceNodeData sourceNode)
        {
            modeFieldsContainer.Clear();

            if (sourceNode.SourceTypeValue == PCGSourceNodeData.SourceType.Grid)
            {
                modeFieldsContainer.Add(CreateFloatField("Spacing", sourceNode.Spacing, value => sourceNode.Spacing = value));
            }
            else
            {
                modeFieldsContainer.Add(CreateIntField("Point Count", sourceNode.RandomPointCount, value => sourceNode.RandomPointCount = value));
            }

            RefreshExpandedState();
        }

        private FloatField CreateFloatField(string label, float initialValue, System.Action<float> onChanged)
        {
            var field = new FloatField(label)
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

        private IntegerField CreateIntField(string label, int initialValue, System.Action<int> onChanged)
        {
            var field = new IntegerField(label)
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
