using System;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics
{
    public class ServerCollider : NetworkBehaviour
    {
        [SerializeField] private Rigidbody bodyPart;
        [SerializeField] private GameObject collider;

        private void Start()
        {
            var col = Instantiate(collider, transform.position, transform.rotation);
            var fixedJoint = col.GetComponent<FixedJoint>();
            fixedJoint.connectedBody = bodyPart;
        }
    }
}