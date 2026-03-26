using UnityEngine;

namespace PCG
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine.UIElements;

    public abstract class PCGNodeView : Node
    {
        public PCGNodeData nodeData;
        public string GUID => nodeData.GUID;

        // Создание UI элементов из данных ноды
        public virtual void Initialize(PCGNodeData data, Vector2 position)
        {
            nodeData = data;
            SetPosition(new Rect(position, new Vector2(200, 100)));

            title = data.nodeName;

            // Добавляем порты
            AddInputPort();
            AddOutputPort();

            // Добавляем кастомные параметры
            CreateParameterFields();
        }

        protected virtual void AddInputPort()
        {
            var inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(List<PCGPoint>));
            inputPort.portName = "Input";
            inputContainer.Add(inputPort);
        }

        protected virtual void AddOutputPort()
        {
            var outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(List<PCGPoint>));
            outputPort.portName = "Output";
            outputContainer.Add(outputPort);
        }

        protected virtual void CreateParameterFields()
        {
            var parameters = nodeData.GetParameters();
            foreach (var param in parameters)
            {
                var field = CreateFieldForParameter(param);
                if (field != null)
                {
                    extensionContainer.Add(field);
                }
            }

            RefreshExpandedState();
        }

        private VisualElement CreateFieldForParameter(PCGNodeParameter param)
        {
            switch (param.type)
            {
                case PCGParameterType.Float:
                    var floatField = new FloatField(param.name);
                    floatField.value = (float)param.value;
                    floatField.RegisterValueChangedCallback(evt =>
                    {
                        param.value = evt.newValue;
                        UpdateNodeData();
                    });
                    return floatField;

                case PCGParameterType.Int:
                    var intField = new IntegerField(param.name);
                    intField.value = (int)param.value;
                    intField.RegisterValueChangedCallback(evt =>
                    {
                        param.value = evt.newValue;
                        UpdateNodeData();
                    });
                    return intField;

                case PCGParameterType.Bool:
                    var toggle = new Toggle(param.name);
                    toggle.value = (bool)param.value;
                    toggle.RegisterValueChangedCallback(evt =>
                    {
                        param.value = evt.newValue;
                        UpdateNodeData();
                    });
                    return toggle;

                case PCGParameterType.String:
                    var textField = new TextField(param.name);
                    textField.value = (string)param.value;
                    textField.RegisterValueChangedCallback(evt =>
                    {
                        param.value = evt.newValue;
                        UpdateNodeData();
                    });
                    return textField;

                case PCGParameterType.Dropdown:
                    var dropdown = new PopupField<string>(param.name, new List<string>(param.options), (int)param.value);
                    dropdown.RegisterValueChangedCallback(evt =>
                    {
                        param.value = dropdown.index;
                        UpdateNodeData();
                    });
                    return dropdown;

                default:
                    return new Label(param.name);
            }
        }

        protected virtual void UpdateNodeData()
        {
            // Сохраняем изменения в ScriptableObject
            EditorUtility.SetDirty(nodeData);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            // Можно добавить выделение в сцене
        }
    }
}
