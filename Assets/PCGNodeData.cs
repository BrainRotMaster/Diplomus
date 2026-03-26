using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace PCG
{
    [System.Serializable]
    public abstract class PCGNodeData : ScriptableObject
    {
        [HideInInspector]
        public string GUID;

        [HideInInspector]
        public Vector2 position;

        [HideInInspector]
        public string nodeName;

        // Кастомные параметры ноды (будут отображаться в UI)
        public abstract List<PCGNodeParameter> GetParameters();

        // Основной метод обработки
        public abstract List<PCGPoint> Process(List<PCGPoint> inputPoints, PCGExecutionContext context);

        // Визуальное представление (какой класс View использовать)
        public abstract Type GetViewType();
    }

    // Параметр ноды для UI
    [System.Serializable]
    public class PCGNodeParameter
    {
        public string name;
        public PCGParameterType type;
        public object value;

        // Для числовых значений
        public float minValue;
        public float maxValue;

        // Для выпадающих списков
        public string[] options;
    }

    public enum PCGParameterType
    {
        Float,
        Int,
        Bool,
        String,
        Vector3,
        GameObject,
        LayerMask,
        Dropdown
    }
}
