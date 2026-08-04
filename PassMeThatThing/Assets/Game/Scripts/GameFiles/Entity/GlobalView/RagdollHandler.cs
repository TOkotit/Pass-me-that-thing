using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.GlobalView
{
    public class RagdollHandler : NetworkBehaviour
    {
        [SerializeField] private List<Rigidbody> rigidbodies;
        public List<Rigidbody> GetRigidbodies() => rigidbodies;

        private Rigidbody _injuredRigidbody;
        
        public void Hit(Vector3 force, Vector3 hitPosition)
        {
            _injuredRigidbody = rigidbodies.Where(rigidbody => !rigidbody.isKinematic)
                .OrderBy(rigidbody => Vector3.Distance(rigidbody.position, hitPosition))
                .FirstOrDefault();
            Debug.LogWarning(_injuredRigidbody);
            _injuredRigidbody?.AddForceAtPosition(force, hitPosition, ForceMode.Impulse);
        }

        

        public virtual void EnableRagdoll()
        {
            foreach (var r in rigidbodies)
            {
                r.isKinematic = false;
            }
        }
        
        public virtual void DisableRagdoll()
        {
            foreach (var r in rigidbodies)
            {
                r.isKinematic = true;
            }
        }
    }
}