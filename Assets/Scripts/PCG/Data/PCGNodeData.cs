namespace PCG
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public abstract class PCGNodeData : ScriptableObject
    {
        [HideInInspector]
        public string GUID;

        [HideInInspector]
        public Vector2 position;

        [HideInInspector]
        public string nodeName = "Node";

        public abstract List<PCGNodeParameter> GetParameters();
        public abstract List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context);

        // Возвращаем имя типа View вместо прямого типа
        public abstract string GetViewTypeName();
    }
}
