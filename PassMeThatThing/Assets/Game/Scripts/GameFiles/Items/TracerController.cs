using UnityEngine;

namespace Game.Scripts.GameFiles.Items
{
    public class TracerController : MonoBehaviour
    {
        [SerializeField] private GameObject tracerPrefab;
        [SerializeField] private LayerMask hitLayers = ~0;
        [SerializeField] private float maxDistance = 100f;
        [SerializeField] private float duration = 0.5f;

        public Vector3 Shoot(Vector3 origin, Vector3 direction)
        {
            float distance = maxDistance;
            if (Physics.Raycast(origin, direction, out var hit, maxDistance, hitLayers))
                distance = hit.distance;

            var instance = Instantiate(tracerPrefab, origin, Quaternion.LookRotation(direction));
            var lr = instance.GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.useWorldSpace = false;
                lr.positionCount = 2;
                lr.SetPosition(0, Vector3.zero);
                lr.SetPosition(1, Vector3.forward * distance);
            }
            Destroy(instance, duration);
            return hit.point;
        }
    }
}