namespace PCG
{
    using UnityEngine;

    [System.Serializable]
    public class PCGNodeParameter
    {
        public string name;
        public PCGParameterType type;
        public object value;
        public float minValue;
        public float maxValue;
        public string[] options;

        public PCGNodeParameter(string name, PCGParameterType type, object value)
        {
            this.name = name;
            this.type = type;
            this.value = value;
        }
    }

    public enum PCGParameterType
    {
        Float,
        Int,
        Bool,
        String,
        Dropdown,
        GameObject  // Добавляем новый тип
    }
}
