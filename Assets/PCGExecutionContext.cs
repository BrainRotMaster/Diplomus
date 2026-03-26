using UnityEngine;

namespace PCG
{

    public class PCGExecutionContext
    {
        public int randomSeed;
        public System.Random random;
        public Bounds generationBounds;
        public Transform worldRoot;           // Куда спавнить объекты
        public float globalDensityScale = 1f;

        // Статистика для отладки
        public int pointsGenerated;
        public int pointsFiltered;

        public PCGExecutionContext(int seed)
        {
            randomSeed = seed;
            random = new System.Random(seed);
        }

        public float GetRandomFloat(float min, float max)
        {
            return (float)random.NextDouble() * (max - min) + min;
        }

        public int GetRandomInt(int min, int max)
        {
            return random.Next(min, max);
        }
    }
}
