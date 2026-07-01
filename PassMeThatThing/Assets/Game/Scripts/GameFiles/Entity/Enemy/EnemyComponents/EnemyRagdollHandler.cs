using System.Collections.Generic;
using System.Linq;
using FishNet.Object;

using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemyRagdollHandler : NetworkBehaviour
    {
        [SerializeField] private List<Rigidbody> _rigidbodies;

        private Rigidbody _injuredRigidbody;
        
        public void Hit(Vector3 force, Vector3 hitPosition)
        {
            _injuredRigidbody = _rigidbodies
                .OrderBy(rigidbody => Vector3.Distance(rigidbody.position, hitPosition))
                .First();

            _injuredRigidbody.AddForceAtPosition(force, hitPosition, ForceMode.Impulse);
        }

        public void EnableRagdoll()
        {
            foreach (var r in _rigidbodies)
            {
                r.isKinematic = false;
            }
        }
        
        public void DisableRagdoll()
        {
            foreach (var r in _rigidbodies)
            {
                r.isKinematic = true;
            }
        }
    }
}