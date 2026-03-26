using UnityEngine;
using Unity.Collections;

namespace PCG
{


    [System.Serializable]
    public struct PCGPoint
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public float density;        // Плотность/вес точки
        public Color color;          // Цвет для отладки

        // Расширенные атрибуты (можно хранить в NativeHashMap для Jobs)
        public int seed;
        public int tag;              // Битовые флаги для фильтрации

        public PCGPoint(Vector3 pos)
        {
            position = pos;
            rotation = Quaternion.identity;
            scale = Vector3.one;
            density = 1f;
            color = Color.white;
            seed = 0;
            tag = 0;
        }

        public PCGPoint(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
            scale = Vector3.one;
            density = 1f;
            color = Color.white;
            seed = 0;
            tag = 0;
        }
    }
}
