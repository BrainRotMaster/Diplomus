namespace PCG
{
    using System;
    using UnityEngine;

    [ExecuteAlways]
    public class PCGBoxRegion : MonoBehaviour
    {
        [SerializeField, HideInInspector] private string regionId;
        [SerializeField] private Vector3 center;
        [SerializeField] private Vector3 size = new Vector3(10f, 10f, 10f);
        [SerializeField] private Color gizmoColor = new Color(0.25f, 0.85f, 0.35f, 0.9f);

        public string RegionId => regionId;

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

        private void Reset()
        {
            EnsureRegionId();
        }

        private void OnEnable()
        {
            EnsureUniqueRegionId();
        }

        private void OnValidate()
        {
            EnsureUniqueRegionId();
            Size = size;
        }

        public bool Contains(Vector3 worldPoint)
        {
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
            var localBounds = new Bounds(center, size);
            return localBounds.Contains(localPoint);
        }

        public static PCGBoxRegion FindById(string targetRegionId)
        {
            if (string.IsNullOrEmpty(targetRegionId))
            {
                return null;
            }

            var regions = FindObjectsByType<PCGBoxRegion>(FindObjectsSortMode.None);
            foreach (var region in regions)
            {
                if (region != null && region.regionId == targetRegionId)
                {
                    return region;
                }
            }

            return null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = gizmoColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(center, size);
            Gizmos.matrix = Matrix4x4.identity;
        }

        private void EnsureRegionId()
        {
            if (string.IsNullOrEmpty(regionId))
            {
                regionId = Guid.NewGuid().ToString();
            }
        }

        private void EnsureUniqueRegionId()
        {
            EnsureRegionId();

            var regions = FindObjectsByType<PCGBoxRegion>(FindObjectsSortMode.None);
            foreach (var region in regions)
            {
                if (region == null || region == this)
                {
                    continue;
                }

                if (region.regionId == regionId)
                {
                    regionId = Guid.NewGuid().ToString();
                    return;
                }
            }
        }
    }
}
