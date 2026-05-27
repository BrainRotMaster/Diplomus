namespace PCG
{
    using UnityEngine;

    [AddComponentMenu("PCG/Regions/Cylinder Region")]
    public class PCGCylinderRegion : PCGRegionBase
    {
        [SerializeField] private Vector3 center;
        [SerializeField] private float radius = 5f;
        [SerializeField] private float height = 10f;
        [SerializeField] private Color gizmoColor = new Color(0.2f, 0.7f, 0.95f, 0.9f);

        public Vector3 Center
        {
            get => center;
            set => center = value;
        }

        public float Radius
        {
            get => radius;
            set => radius = Mathf.Max(0.01f, value);
        }

        public float Height
        {
            get => height;
            set => height = Mathf.Max(0.01f, value);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            Radius = radius;
            Height = height;
        }

        public override bool Contains(Vector3 worldPoint)
        {
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint) - center;
            if (Mathf.Abs(localPoint.y) > height * 0.5f)
            {
                return false;
            }

            Vector2 radialPoint = new Vector2(localPoint.x, localPoint.z);
            return radialPoint.sqrMagnitude <= radius * radius;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;
            Gizmos.matrix = transform.localToWorldMatrix;

            const int segments = 32;
            float halfHeight = height * 0.5f;
            Vector3 topCenter = center + Vector3.up * halfHeight;
            Vector3 bottomCenter = center - Vector3.up * halfHeight;
            Vector3 previousTop = topCenter + new Vector3(radius, 0f, 0f);
            Vector3 previousBottom = bottomCenter + new Vector3(radius, 0f, 0f);

            for (int i = 1; i <= segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Vector3 currentTop = topCenter + offset;
                Vector3 currentBottom = bottomCenter + offset;

                Gizmos.DrawLine(previousTop, currentTop);
                Gizmos.DrawLine(previousBottom, currentBottom);

                if (i % (segments / 4) == 0)
                {
                    Gizmos.DrawLine(previousTop, previousBottom);
                }

                previousTop = currentTop;
                previousBottom = currentBottom;
            }

            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
