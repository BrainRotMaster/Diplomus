using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PCG
{
    public class PCGFilterNodeView : PCGNodeView
    {
        private VisualElement modeFieldsContainer;

        protected override void CreateParameterFields()
        {
            var filterNode = nodeData as PCGFilterNodeData;
            if (filterNode == null)
            {
                base.CreateParameterFields();
                return;
            }

            var filterTypeField = new EnumField("Filter Type", filterNode.FilterTypeValue);
            filterTypeField.Init(filterNode.FilterTypeValue);
            filterTypeField.RegisterValueChangedCallback(evt =>
            {
                RecordNodeUndo();
                filterNode.FilterTypeValue = (PCGFilterNodeData.FilterType)evt.newValue;
                RebuildModeFields(filterNode);
                UpdateNodeData();
            });
            extensionContainer.Add(filterTypeField);

            modeFieldsContainer = new VisualElement();
            extensionContainer.Add(modeFieldsContainer);

            RebuildModeFields(filterNode);
        }

        private void RebuildModeFields(PCGFilterNodeData filterNode)
        {
            modeFieldsContainer.Clear();

            if (filterNode.FilterTypeValue == PCGFilterNodeData.FilterType.RandomChance)
            {
                modeFieldsContainer.Add(CreateFloatField("Random Chance", filterNode.RandomChance, value => filterNode.RandomChance = value));
            }
            else if (filterNode.FilterTypeValue == PCGFilterNodeData.FilterType.DensityThreshold)
            {
                modeFieldsContainer.Add(CreateFloatField("Min Density", filterNode.MinDensity, value => filterNode.MinDensity = value));
            }
            else
            {
                modeFieldsContainer.Add(CreateIntField("Tag", filterNode.RequiredTag, value => filterNode.RequiredTag = value));
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
