using System;
using DI;
using Entity;
using Game.Entity;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using Systems;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics
{
    public class PhysicalItemInteractionController : NetworkBehaviour
    {
        public PhysicalItem CurrentHeldItem => _heldItem;

        [SerializeField] private PhysicalItem _heldItem;
        [SerializeField] private MainCharacter mainCharacter;
        private HandsMovement _handsMovement;
        private DamagableRegistry _damagableRegistry; 
        //private int _originalHeldItemLayer;   

        public Rigidbody Pivot => _handsMovement.Pivot;
        public HandsMovement HandsMovement => _handsMovement;

        public override void OnStartLocalPlayer()
        {
            InjectSelf();
        }

        [Inject]
        private void Construct(DamagableRegistry damagableRegistry)
        {
            _damagableRegistry = damagableRegistry;
        }

        private void Start()
        {
            _handsMovement = GetComponentInChildren<HandsMovement>();
        }

        private void InjectSelf()
        {
            var scope = FindObjectOfType<GameplayScope>();
            if (scope)
                scope.Container.Inject(this);
            else
                Debug.LogError("GameplayScope not found!");
        }

        private void SetOwnerAndLayer(PhysicalItem item)
        {
            if (item.CanBeOwned)
            {
                item.Owner = mainCharacter;
                item.gameObject.layer = LayerMask.NameToLayer("HeldItem");
            }
        }
        
        private void RestoreLayerAndClear(PhysicalItem item)
        {
            if (item)
            {
                item.gameObject.layer = LayerMask.NameToLayer("Interactable");
                item.Owner = null;
            }
        }

        public void ChargeDrop()
        {
            if (_heldItem)
                _handsMovement.ChargeThrow();
        }

        [Server]
        public void PhysicalPickUpItem(PhysicalItem item)
        {
            _heldItem = item;
            SetOwnerAndLayer(item);
            TargetPickUpItem(item);
            _handsMovement.GrabItem(item);
        }

        [TargetRpc]
        private void TargetPickUpItem(PhysicalItem item)
        {
            _heldItem = item;
            SetOwnerAndLayer(item);
            if (_heldItem)
                _handsMovement.GrabItem(_heldItem);
        }

        [Server]
        public void ReleaseCurrentItem(float throwForce, bool canThrow)
        {
            if (_heldItem)
            {
                RestoreLayerAndClear(_heldItem);
                _handsMovement.ReleaseItem(_heldItem, throwForce, canThrow);
                _heldItem = null;
                TargetClearHeldItem();
            }
        }

        [Server]
        public void ServerClearHeldItem()
        {
            if (_heldItem)
            {
                RestoreLayerAndClear(_heldItem);
                _heldItem = null;
            }
            TargetClearHeldItem();
        }

        [TargetRpc]
        public void TargetClearHeldItem()
        {
            if (_heldItem)
            {
                RestoreLayerAndClear(_heldItem);
                _handsMovement.ReleaseItem(_heldItem, 0f, false);
                _heldItem = null;
            }
        }

        [TargetRpc]
        public void TargetSyncPositionForDrop(NetworkConnection target, Vector3 position, Quaternion rotation)
        {
            if (_heldItem)
            {
                _heldItem.Rigidbody.MovePosition(position);
                _heldItem.Rigidbody.MoveRotation(rotation);
            }
        }
    }
}