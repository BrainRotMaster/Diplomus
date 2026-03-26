using UnityEngine;

namespace PCG
{
    using UnityEngine;

    public class PCGGenerator : MonoBehaviour
    {
        public PCGGraphData graph;
        public Bounds generationBounds = new Bounds(Vector3.zero, new Vector3(100, 50, 100));
        public int randomSeed = 42;
        public Transform spawnRoot;

        [ContextMenu("Generate")]
        public void Generate()
        {
            if (graph == null)
            {
                Debug.LogError("No PCG Graph assigned!");
                return;
            }

            // Создаем контекст
            var context = new PCGExecutionContext(randomSeed)
            {
                generationBounds = generationBounds,
                worldRoot = spawnRoot != null ? spawnRoot : transform
            };

            // Выполняем граф
            var executor = new PCGGraphExecutor(graph);
            var points = executor.Execute(context);

            Debug.Log($"Generation complete! Generated {points.Count} final points.");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(generationBounds.center, generationBounds.size);
        }
    }
}
