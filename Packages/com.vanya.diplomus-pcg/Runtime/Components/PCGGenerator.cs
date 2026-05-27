namespace PCG
{
    using System.Collections.Generic;
    using UnityEngine;

    public class PCGGenerator : MonoBehaviour
    {
        public PCGGraphData graph;
        public Bounds generationBounds = new Bounds(Vector3.zero, new Vector3(50, 10, 50));
        public int randomSeed = 42;
        public Transform spawnRoot;
        public bool debugDrawPoints = true;

        [System.NonSerialized] private List<PCGPoint> lastGeneratedPoints = new List<PCGPoint>();

        [ContextMenu("Generate")]
        public void Generate()
        {
            if (graph == null)
            {
                Debug.LogError("No PCG Graph assigned!");
                return;
            }

            ClearGeneratedObjects();

            var context = new PCGExecutionContext(randomSeed)
            {
                generationBounds = generationBounds,
                generatorTransform = transform,
                worldRoot = spawnRoot != null ? spawnRoot : transform
            };

            var executor = new PCGGraphExecutor(graph);
            var points = executor.Execute(context);
            lastGeneratedPoints = points ?? new List<PCGPoint>();

            Debug.Log($"Generation complete! Final points: {points.Count}");
            Debug.Log($"Stats - Generated: {context.pointsGenerated}, Filtered: {context.pointsFiltered}");
        }

        private void ClearGeneratedObjects()
        {
            Transform root = spawnRoot != null ? spawnRoot : transform;

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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(generationBounds.center, generationBounds.size);
            Gizmos.matrix = Matrix4x4.identity;

            DrawDebugPointsGizmos();
        }

        private void OnDrawGizmos()
        {
            DrawDebugPointsGizmos();
        }

        private void DrawDebugPointsGizmos()
        {
            if (!debugDrawPoints || lastGeneratedPoints == null || lastGeneratedPoints.Count == 0)
            {
                return;
            }

            float pointRadius = Mathf.Max(0.05f, Mathf.Min(generationBounds.size.x, generationBounds.size.z) * 0.01f);
            Gizmos.matrix = Matrix4x4.identity;

            foreach (var point in lastGeneratedPoints)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(point.position, pointRadius);
            }
        }
    }
}
