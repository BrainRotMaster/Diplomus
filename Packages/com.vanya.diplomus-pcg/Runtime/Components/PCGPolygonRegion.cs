namespace PCG
{
    using System.Collections.Generic;
    using UnityEngine;

    [AddComponentMenu("PCG/Regions/Polygon Region")]
    public class PCGPolygonRegion : PCGRegionBase
    {
        [SerializeField] private List<Vector2> points = new List<Vector2>
        {
            new Vector2(-5f, -5f),
            new Vector2(-5f, 5f),
            new Vector2(5f, 5f),
            new Vector2(5f, -5f)
        };
        [SerializeField] private float centerY;
        [SerializeField] private float height = 10f;
        [SerializeField] private Color gizmoColor = new Color(0.95f, 0.65f, 0.2f, 0.9f);

        public List<Vector2> Points => points;

        public float CenterY
        {
            get => centerY;
            set => centerY = value;
        }

        public float Height
        {
            get => height;
            set => height = Mathf.Max(0.01f, value);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            Height = height;
            if (points == null)
            {
                points = new List<Vector2>();
            }
        }

        public override bool Contains(Vector3 worldPoint)
        {
            if (points == null || points.Count < 3)
            {
                return false;
            }

            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
            if (Mathf.Abs(localPoint.y - centerY) > height * 0.5f)
            {
                return false;
            }

            return IsPointInsidePolygon(new Vector2(localPoint.x, localPoint.z));
        }

        private bool IsPointInsidePolygon(Vector2 point)
        {
            bool inside = false;
            int pointCount = points.Count;

            for (int i = 0, j = pointCount - 1; i < pointCount; j = i++)
            {
                Vector2 a = points[i];
                Vector2 b = points[j];

                bool intersects = ((a.y > point.y) != (b.y > point.y)) &&
                    (point.x < ((b.x - a.x) * (point.y - a.y) / ((b.y - a.y) + Mathf.Epsilon)) + a.x);

                if (intersects)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        private void OnDrawGizmosSelected()
        {
            if (points == null || points.Count < 2)
            {
                return;
            }

            Gizmos.color = gizmoColor;
            Gizmos.matrix = transform.localToWorldMatrix;

            float halfHeight = height * 0.5f;
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 current = points[i];
                Vector2 next = points[(i + 1) % points.Count];
                Vector3 bottomA = new Vector3(current.x, centerY - halfHeight, current.y);
                Vector3 bottomB = new Vector3(next.x, centerY - halfHeight, next.y);
                Vector3 topA = new Vector3(current.x, centerY + halfHeight, current.y);
                Vector3 topB = new Vector3(next.x, centerY + halfHeight, next.y);

                Gizmos.DrawLine(bottomA, bottomB);
                Gizmos.DrawLine(topA, topB);
                Gizmos.DrawLine(bottomA, topA);
            }

            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
