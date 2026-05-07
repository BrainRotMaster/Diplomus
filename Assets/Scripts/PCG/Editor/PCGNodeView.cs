namespace PCG
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    public abstract class PCGNodeView : Node
    {
        public PCGNodeData nodeData;
        public string GUID => nodeData.GUID;
        public Port InputPort { get; private set; }
        public Port OutputPort { get; private set; }
        public Action OnNodeChanged { get; set; }

        private List<VisualElement> parameterFields = new List<VisualElement>();

        public virtual void Initialize(PCGNodeData data, Vector2 position)
        {
            nodeData = data;
            SetPosition(new Rect(position, new Vector2(250, 120)));

            title = data.nodeName;

            InputPort = CreatePort("Input", Direction.Input, Port.Capacity.Multi);
            OutputPort = CreatePort("Output", Direction.Output, Port.Capacity.Multi);

            inputContainer.Add(InputPort);
            outputContainer.Add(OutputPort);

            CreateParameterFields();
            RefreshExpandedState();
            RefreshPorts();
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            if (nodeData == null || nodeData.position == newPos.position)
            {
                return;
            }

            nodeData.position = newPos.position;
            EditorUtility.SetDirty(nodeData);
            OnNodeChanged?.Invoke();
        }

        protected Port CreatePort(string portName, Direction direction, Port.Capacity capacity)
        {
            var port = InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(List<PCGPoint>));
            port.portName = portName;
            port.userData = this;
            return port;
        }

        protected virtual void CreateParameterFields()
        {
            var parameters = nodeData.GetParameters();
            foreach (var param in parameters)
            {
                var field = CreateFieldForParameter(param);
                if (field != null)
                {
                    parameterFields.Add(field);
                    extensionContainer.Add(field);
                }
            }
        }

        private VisualElement CreateFieldForParameter(PCGNodeParameter param)
        {
            switch (param.type)
            {
                case PCGParameterType.String:
                    var textField = new TextField(param.name);
                    textField.value = (string)param.value;
                    textField.RegisterValueChangedCallback(evt =>
                    {
                        param.value = evt.newValue;
                        nodeData.UpdateParameter(param.name, evt.newValue);
                        UpdateNodeData();
                    });
                    return textField;

                case PCGParameterType.Int:
                    var intField = new IntegerField(param.name);
                    intField.value = (int)param.value;
                    intField.RegisterValueChangedCallback(evt =>
                    {
                        param.value = evt.newValue;
                        nodeData.UpdateParameter(param.name, evt.newValue);
                        UpdateNodeData();
                    });
                    return intField;

                case PCGParameterType.Float:
                    var floatField = new FloatField(param.name);
                    floatField.value = (float)param.value;
                    floatField.RegisterValueChangedCallback(evt =>
                    {
                        param.value = evt.newValue;
                        nodeData.UpdateParameter(param.name, evt.newValue);
                        UpdateNodeData();
                    });
                    return floatField;

                case PCGParameterType.Bool:
                    var toggle = new Toggle(param.name);
                    toggle.value = (bool)param.value;
                    toggle.RegisterValueChangedCallback(evt =>
                    {
                        param.value = evt.newValue;
                        nodeData.UpdateParameter(param.name, evt.newValue);
                        UpdateNodeData();
                    });
                    return toggle;

                case PCGParameterType.Enum:
                    if (param.enumType != null && param.value is Enum enumValue)
                    {
                        var enumField = new EnumField(param.name, enumValue);
                        enumField.Init(enumValue);
                        enumField.RegisterValueChangedCallback(evt =>
                        {
                            param.value = evt.newValue;
                            nodeData.UpdateParameter(param.name, Convert.ToInt32(evt.newValue));
                            UpdateNodeData();
                        });
                        return enumField;
                    }
                    break;

                case PCGParameterType.GameObject:
                    var container = new VisualElement();
                    container.style.flexDirection = FlexDirection.Row;
                    container.style.marginBottom = 4;
                    container.style.marginTop = 4;

                    var label = new Label(param.name);
                    label.style.width = 80;
                    container.Add(label);

                    var objectField = new ObjectField();
                    objectField.objectType = typeof(GameObject);
                    objectField.allowSceneObjects = false;
                    objectField.value = (GameObject)param.value;
                    objectField.style.flexGrow = 1;

                    objectField.RegisterValueChangedCallback(evt =>
                    {
                        param.value = evt.newValue;
                        nodeData.UpdateParameter(param.name, evt.newValue);
                        UpdateNodeData();
                    });

                    container.Add(objectField);
                    return container;

                case PCGParameterType.Dropdown:
                    if (param.options != null && param.options.Length > 0)
                    {
                        var dropdown = new PopupField<string>(param.name, new List<string>(param.options), (int)param.value);
                        dropdown.RegisterValueChangedCallback(evt =>
                        {
                            param.value = dropdown.index;
                            nodeData.UpdateParameter(param.name, dropdown.index);
                            UpdateNodeData();
                        });
                        return dropdown;
                    }
                    break;
            }

            return null;
        }

        protected virtual void UpdateNodeData()
        {
            EditorUtility.SetDirty(nodeData);
            OnNodeChanged?.Invoke();
        }

        public override void OnSelected()
        {
            base.OnSelected();
        }
    }
}
