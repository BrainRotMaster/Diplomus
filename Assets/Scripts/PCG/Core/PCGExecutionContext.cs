namespace PCG
{

    using UnityEngine;

    public class PCGExecutionContext
    {
        public int randomSeed;
        public System.Random random;
        public Bounds generationBounds;
        public Transform worldRoot;
        public float globalDensityScale = 1f;
        public int pointsGenerated;
        public int pointsFiltered;

        public PCGExecutionContext(int seed)
        {
            randomSeed = seed;
            random = new System.Random(seed);
            generationBounds = new Bounds(Vector3.zero, new Vector3(100, 50, 100));
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
