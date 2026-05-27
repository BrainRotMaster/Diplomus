namespace PCG
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using UnityEngine.UIElements;

    public abstract class PCGNodeView : Node
    {
        public PCGNodeData nodeData;
        public string GUID => nodeData.GUID;
        public Action OnNodeChanged { get; set; }

        private readonly Dictionary<string, Port> inputPortsByName = new Dictionary<string, Port>();
        private readonly Dictionary<string, Port> outputPortsByName = new Dictionary<string, Port>();

        public virtual void Initialize(PCGNodeData data, Vector2 position)
        {
            nodeData = data;
            SetPosition(new Rect(position, new Vector2(250, 120)));

            title = data.nodeName;

            CreatePorts();

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

            Undo.RecordObject(nodeData, "Move PCG Node");
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

        public Port GetInputPort(string portName)
        {
            if (string.IsNullOrEmpty(portName))
            {
                portName = PCGNodeData.DefaultInputPortName;
            }

            inputPortsByName.TryGetValue(portName, out var port);
            return port;
        }

        public Port GetOutputPort(string portName)
        {
            if (string.IsNullOrEmpty(portName))
            {
                portName = PCGNodeData.DefaultOutputPortName;
            }

            outputPortsByName.TryGetValue(portName, out var port);
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
                        RecordNodeUndo();
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
                        RecordNodeUndo();
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
                        RecordNodeUndo();
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
                        RecordNodeUndo();
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
                            RecordNodeUndo();
                            param.value = evt.newValue;
                            nodeData.UpdateParameter(param.name, Convert.ToInt32(evt.newValue));
                            UpdateNodeData();
                        });
                        return enumField;
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

        protected void RecordNodeUndo(string actionName = "Edit PCG Node")
        {
            if (nodeData != null)
            {
                Undo.RecordObject(nodeData, actionName);
            }
        }

        private void CreatePorts()
        {
            inputPortsByName.Clear();
            outputPortsByName.Clear();

            foreach (var portName in nodeData.GetInputPortNames())
            {
                var port = CreatePort(portName, Direction.Input, Port.Capacity.Multi);
                inputPortsByName[portName] = port;
                inputContainer.Add(port);
            }

            foreach (var portName in nodeData.GetOutputPortNames())
            {
                var port = CreatePort(portName, Direction.Output, Port.Capacity.Multi);
                outputPortsByName[portName] = port;
                outputContainer.Add(port);
            }
        }
    }
}
