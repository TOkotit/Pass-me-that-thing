using System;
using FishNet.Object;
using Game.NewMainCharacterPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class ItemCatcher : NetworkBehaviour
    {
        private PhysicalItemRegistry _physicalItemRegistry;
        [SerializeField] private Transform dir;
        private PlayerInteraction _playerInteraction;

        public void SetPlayerInteraction(PlayerInteraction playerInteraction)
        {
            _playerInteraction = playerInteraction;
        }
        public void SetRegistry(PhysicalItemRegistry registry) 
        {
            _physicalItemRegistry = registry;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (_physicalItemRegistry != null && 
                !_playerInteraction.PhysicalItemInteractionController.CurrentHeldItem)
            {
                _physicalItemRegistry.TryGetItem(other.gameObject, out var item);
                if ((item && item.Rigidbody.linearVelocity.magnitude < 0.1f) || !item) return;
                Debug.Log("Предмет брошен:"+item.IsThrown);
                if (item && item.IsThrown)
                {
                    if (item.Rigidbody && (item.Rigidbody.linearVelocity.normalized
                        + dir.forward).magnitude < 0.7f)
                    {
                        _playerInteraction.TryPickUp(item);
                    }
                }
            }
        }
    }
}