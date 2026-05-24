using System;
using Entity;
using Game.Entity;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
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
        [Inject] private PhysicalItemRegistry _physicalItemRegistry; 

        private void Start()
        {
            var col = Instantiate(collider, transform.position, transform.rotation);
            var fixedJoint = col.GetComponent<FixedJoint>();
            var itemCatcher = col.GetComponentInChildren<ItemCatcher>();
            fixedJoint.connectedBody = bodyPart;
            itemCatcher.SetPlayerInteraction(player.MainCharacterModel.PlayerInteraction);
            itemCatcher.SetRegistry(_physicalItemRegistry); 
            
            _damagableRegistry.Register(col.gameObject, player);
        }
    }
}