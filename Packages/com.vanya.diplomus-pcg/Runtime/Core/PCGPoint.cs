
namespace PCG
{
    using UnityEngine;

    [System.Serializable]
    public struct PCGPoint
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public float density;
        public int tag;
        public int priority;

        public PCGPoint(Vector3 pos)
        {
            position = pos;
            rotation = Quaternion.identity;
            scale = Vector3.one;
            density = 1f;
            tag = 0;
            priority = 0;
        }

        public PCGPoint(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
            scale = Vector3.one;
            density = 1f;
            tag = 0;
            priority = 0;
        }
    }
}
