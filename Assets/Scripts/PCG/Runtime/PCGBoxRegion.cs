namespace PCG
{
    using UnityEngine;

    [AddComponentMenu("PCG/Regions/Box Region")]
    public class PCGBoxRegion : PCGRegionBase
    {
        [SerializeField] private Vector3 center;
        [SerializeField] private Vector3 size = new Vector3(10f, 10f, 10f);
        [SerializeField] private Color gizmoColor = new Color(0.25f, 0.85f, 0.35f, 0.9f);

        public Vector3 Center
        {
            get => center;
            set => center = value;
        }

        public Vector3 Size
        {
            get => size;
            set => size = new Vector3(
                Mathf.Max(0.01f, value.x),
                Mathf.Max(0.01f, value.y),
                Mathf.Max(0.01f, value.z));
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            Size = size;
        }

        public override bool Contains(Vector3 worldPoint)
        {
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
            var localBounds = new Bounds(center, size);
            return localBounds.Contains(localPoint);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(center, size);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
