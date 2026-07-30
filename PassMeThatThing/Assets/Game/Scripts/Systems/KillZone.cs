using Mirror;
using UnityEngine;

namespace Game.Scripts.Systems
{
    public class KillZone : NetworkBehaviour
    {
        [SerializeField] private Transform pointOfTeleporting;
        
        
        [ServerCallback]
        public void OnTriggerEnter(Collider other)
        {
            Debug.Log("<color=red>OnTriggerEnter</color>");
            var networkIdentity = other.GetComponent<NetworkIdentity>();
            if (networkIdentity == null) return;
            
            other.transform.position = pointOfTeleporting.position;
            
            var rb = other.GetComponent<Rigidbody>();
            if (rb == null) return;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}