using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemyRagdollHandler : NetworkBehaviour
    {
        [SerializeField] private List<Rigidbody> _rigidbodies;

        public void Init()
        {
            _rigidbodies = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>());
        }
        
        public void Hit(Vector3 force, Vector3 hitPosition)
        {
            var injuredRigidbody = _rigidbodies
                .OrderBy(rigidbody => Vector3.Distance(rigidbody.position, hitPosition))
                .First();

            injuredRigidbody.AddForceAtPosition(force, hitPosition, ForceMode.Impulse);
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