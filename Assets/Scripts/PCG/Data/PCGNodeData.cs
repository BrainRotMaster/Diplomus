using PCG;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class PCGNodeData : ScriptableObject
{
    [HideInInspector] public string GUID;
    [HideInInspector] public Vector2 position;
    [HideInInspector] public string nodeName = "Node";

    public abstract List<PCGNodeParameter> GetParameters();
    public abstract void UpdateParameter(string name, object value);
    public abstract List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context);
    public abstract string GetViewTypeName();
}