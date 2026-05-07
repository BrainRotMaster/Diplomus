using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PCG
{
    public class PCGSpawnerNodeView : PCGNodeView
    {
        private VisualElement prefabListContainer;

        protected override void CreateParameterFields()
        {
            var spawnerNode = nodeData as PCGSpawnerNodeData;
            if (spawnerNode == null)
            {
                base.CreateParameterFields();
                return;
            }

            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.marginBottom = 4;

            var headerLabel = new Label("Prefabs");
            headerLabel.style.flexGrow = 1;
            headerRow.Add(headerLabel);

            var addButton = new Button(() =>
            {
                spawnerNode.AddPrefabEntry();
                RebuildPrefabList(spawnerNode);
                UpdateNodeData();
            })
            {
                text = "+"
            };
            addButton.style.width = 26;
            headerRow.Add(addButton);

            extensionContainer.Add(headerRow);

            prefabListContainer = new VisualElement();
            extensionContainer.Add(prefabListContainer);

            RebuildPrefabList(spawnerNode);
        }

        private void RebuildPrefabList(PCGSpawnerNodeData spawnerNode)
        {
            prefabListContainer.Clear();

            for (int i = 0; i < spawnerNode.PrefabEntries.Count; i++)
            {
                int index = i;
                var entry = spawnerNode.PrefabEntries[index];

                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 4;

                var objectField = new ObjectField
                {
                    objectType = typeof(GameObject),
                    allowSceneObjects = false,
                    value = entry.prefab
                };
                objectField.style.flexGrow = 1;
                objectField.RegisterValueChangedCallback(evt =>
                {
                    entry.prefab = evt.newValue as GameObject;
                    UpdateNodeData();
                });
                row.Add(objectField);

                var weightField = new FloatField
                {
                    value = entry.weight
                };
                weightField.style.width = 60;
                weightField.style.marginLeft = 4;
                weightField.RegisterValueChangedCallback(evt =>
                {
                    entry.weight = Mathf.Max(0f, evt.newValue);
                    if (!Mathf.Approximately(weightField.value, entry.weight))
                    {
                        weightField.SetValueWithoutNotify(entry.weight);
                    }
                    UpdateNodeData();
                });
                row.Add(weightField);

                var removeButton = new Button(() =>
                {
                    spawnerNode.RemovePrefabEntryAt(index);
                    RebuildPrefabList(spawnerNode);
                    UpdateNodeData();
                })
                {
                    text = "x"
                };
                removeButton.style.width = 26;
                removeButton.style.marginLeft = 4;
                row.Add(removeButton);

                prefabListContainer.Add(row);
            }

            RefreshExpandedState();
        }

        protected override void UpdateNodeData()
        {
            EditorUtility.SetDirty(nodeData);
            OnNodeChanged?.Invoke();
        }
    }
}
