namespace PCG
{
    using UnityEngine;

    public class PCGGenerator : MonoBehaviour
    {
        public PCGGraphData graph;
        public Bounds generationBounds = new Bounds(Vector3.zero, new Vector3(50, 10, 50));
        public int randomSeed = 42;
        public Transform spawnRoot;
        public bool debugDrawPoints = true;

        [ContextMenu("Generate")]
        public void Generate()
        {
            if (graph == null)
            {
                Debug.LogError("No PCG Graph assigned!");
                return;
            }

            // Очищаем предыдущие сгенерированные объекты
            ClearGeneratedObjects();

            var context = new PCGExecutionContext(randomSeed)
            {
                generationBounds = generationBounds,
                worldRoot = spawnRoot != null ? spawnRoot : transform
            };

            var executor = new PCGGraphExecutor(graph);
            var points = executor.Execute(context);

            Debug.Log($"Generation complete! Final points: {points.Count}");
            Debug.Log($"Stats - Generated: {context.pointsGenerated}, Filtered: {context.pointsFiltered}");

            // Визуализация точек для отладки
            if (debugDrawPoints)
            {
                StartCoroutine(DrawDebugPoints(points));
            }
        }

        private void ClearGeneratedObjects()
        {
            Transform root = spawnRoot != null ? spawnRoot : transform;

            // Удаляем все дочерние объекты, которые были сгенерированы
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            Debug.Log("Cleared previous generated objects");
        }

        private System.Collections.IEnumerator DrawDebugPoints(System.Collections.Generic.List<PCGPoint> points)
        {
            // Рисуем точки в течение 5 секунд
            float duration = 5f;
            float startTime = Time.time;

            while (Time.time - startTime < duration)
            {
                foreach (var point in points)
                {
                    Debug.DrawRay(point.position, Vector3.up * 0.5f, Color.green, 0.1f);
                }
                yield return null;
            }

            Debug.Log($"Debug visualization ended. {points.Count} points were shown");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(generationBounds.center, generationBounds.size);
        }

        private void OnDrawGizmos()
        {
            if (debugDrawPoints && Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                // Здесь можно добавить отрисовку точек если нужно
            }
        }
    }
}
