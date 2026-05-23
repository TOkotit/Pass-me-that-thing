using System;
using Entity;
using Game.Entity;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

namespace Game.NewMainCharacterPhysics
{
    public class ServerCollider : NetworkBehaviour
    {
        [SerializeField] private Rigidbody bodyPart;
        [SerializeField] private GameObject collider;
        [SerializeField] private MainCharacter player;
        [Inject] private DamagableRegistry _damagableRegistry;

        private void Start()
        {
            var col = Instantiate(collider, transform.position, transform.rotation);
            var fixedJoint = col.GetComponent<FixedJoint>();
            fixedJoint.connectedBody = bodyPart;
            
            _damagableRegistry.Register(gameObject, player);
        }
    }
}