namespace PCG
{
    using System;
    using UnityEngine;

    [ExecuteAlways]
    public abstract class PCGRegionBase : MonoBehaviour
    {
        [SerializeField, HideInInspector] private string regionId;

        public string RegionId => regionId;

        protected virtual void Reset()
        {
            EnsureRegionId();
        }

        protected virtual void OnEnable()
        {
            EnsureUniqueRegionId();
        }

        protected virtual void OnValidate()
        {
            EnsureUniqueRegionId();
        }

        public abstract bool Contains(Vector3 worldPoint);

        public static PCGRegionBase FindById(string targetRegionId)
        {
            if (string.IsNullOrEmpty(targetRegionId))
            {
                return null;
            }

            var regions = FindObjectsByType<PCGRegionBase>(FindObjectsSortMode.None);
            foreach (var region in regions)
            {
                if (region != null && region.regionId == targetRegionId)
                {
                    return region;
                }
            }

            return null;
        }

        protected void EnsureRegionId()
        {
            if (string.IsNullOrEmpty(regionId))
            {
                regionId = Guid.NewGuid().ToString();
            }
        }

        protected void EnsureUniqueRegionId()
        {
            EnsureRegionId();

            var regions = FindObjectsByType<PCGRegionBase>(FindObjectsSortMode.None);
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
