using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace PCG
{
    public class PCGProjectToSurfaceNodeView : PCGNodeView
    {
        protected override void CreateParameterFields()
        {
            var projectNode = nodeData as PCGProjectToSurfaceNodeData;
            if (projectNode == null)
            {
                base.CreateParameterFields();
                return;
            }

            extensionContainer.Add(CreateFloatField("Ray Start Offset", projectNode.RayStartOffset, value => projectNode.RayStartOffset = value));
            extensionContainer.Add(CreateFloatField("Max Distance", projectNode.MaxDistance, value => projectNode.MaxDistance = value));
            extensionContainer.Add(CreateFloatField("Max Surface Angle", projectNode.MaxSurfaceAngle, value => projectNode.MaxSurfaceAngle = value));
            extensionContainer.Add(CreateToggle("Align To Normal", projectNode.AlignToSurfaceNormal, value => projectNode.AlignToSurfaceNormal = value));
            extensionContainer.Add(CreateToggle("Discard Misses", projectNode.DiscardMisses, value => projectNode.DiscardMisses = value));
            extensionContainer.Add(CreateLayerMaskField("Layer Mask", projectNode.LayerMaskValue, value => projectNode.LayerMaskValue = value));
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

        private Toggle CreateToggle(string label, bool initialValue, System.Action<bool> onChanged)
        {
            var field = new Toggle(label)
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

        private VisualElement CreateLayerMaskField(string label, int initialMask, System.Action<int> onChanged)
        {
            string[] layerNames = InternalEditorUtility.layers;
            var layerNameList = new System.Collections.Generic.List<string>(layerNames);
            int displayedMask = ToConcatenatedLayerMask(initialMask, layerNames);

            var field = new MaskField(label, layerNameList, displayedMask);
            field.RegisterValueChangedCallback(evt =>
            {
                RecordNodeUndo();
                int unityMask = FromConcatenatedLayerMask(evt.newValue, layerNames);
                onChanged(unityMask);
                UpdateNodeData();
            });

            return field;
        }

        private static int ToConcatenatedLayerMask(int unityMask, string[] layerNames)
        {
            if (unityMask == ~0)
            {
                return ~0;
            }

            int displayedMask = 0;
            for (int i = 0; i < layerNames.Length; i++)
            {
                int layer = LayerMask.NameToLayer(layerNames[i]);
                if (layer >= 0 && (unityMask & (1 << layer)) != 0)
                {
                    displayedMask |= 1 << i;
                }
            }

            return displayedMask;
        }

        private static int FromConcatenatedLayerMask(int displayedMask, string[] layerNames)
        {
            if (displayedMask == ~0)
            {
                return ~0;
            }

            int unityMask = 0;
            for (int i = 0; i < layerNames.Length; i++)
            {
                if ((displayedMask & (1 << i)) == 0)
                {
                    continue;
                }

                int layer = LayerMask.NameToLayer(layerNames[i]);
                if (layer >= 0)
                {
                    unityMask |= 1 << layer;
                }
            }

            return unityMask;
        }

        protected override void UpdateNodeData()
        {
            EditorUtility.SetDirty(nodeData);
            OnNodeChanged?.Invoke();
        }
    }
}
