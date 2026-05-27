using PCG;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class PCGNodeData : ScriptableObject
{
    public const string DefaultInputPortName = "Input";
    public const string DefaultOutputPortName = "Output";

    [HideInInspector] public string GUID;
    [HideInInspector] public Vector2 position;
    [HideInInspector] public string nodeName = "Node";

    public abstract List<PCGNodeParameter> GetParameters();
    public abstract void UpdateParameter(string name, object value);
    public abstract List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context);
    public abstract string GetViewTypeName();

    public virtual IEnumerable<string> GetInputPortNames()
    {
        yield return DefaultInputPortName;
    }

    public virtual IEnumerable<string> GetOutputPortNames()
    {
        yield return DefaultOutputPortName;
    }

    public virtual PCGNodeOutput ProcessMulti(List<PCGPoint> inputPoints, PCGExecutionContext context)
    {
        var output = new PCGNodeOutput();
        output.SetStream(DefaultOutputPortName, Process(inputPoints, context) ?? new List<PCGPoint>());
        return output;
    }
}
