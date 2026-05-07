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
        public System.Type enumType;

        public PCGNodeParameter(string name, PCGParameterType type, object value)
        {
            this.name = name;
            this.type = type;
            this.value = value;
        }

        public static PCGNodeParameter CreateEnum<TEnum>(string name, TEnum value) where TEnum : System.Enum
        {
            return new PCGNodeParameter(name, PCGParameterType.Enum, value)
            {
                enumType = typeof(TEnum)
            };
        }
    }

    public enum PCGParameterType
    {
        Float,
        Int,
        Bool,
        String,
        Enum,
        Dropdown,
        GameObject
    }
}
