using UnityEngine;

namespace PCG
{
    using UnityEngine;

    public class PCGSpawnDebugMarker : MonoBehaviour
    {
        public PCGPoint point;
        public float debugDuration = 5f;

        void Start()
        {
            Debug.Log($"Object spawned at {transform.position} with rotation {transform.rotation.eulerAngles}");

            if (Application.isPlaying)
            {
                Destroy(this, debugDuration);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.3f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.up * 0.5f);
        }
    }
}
