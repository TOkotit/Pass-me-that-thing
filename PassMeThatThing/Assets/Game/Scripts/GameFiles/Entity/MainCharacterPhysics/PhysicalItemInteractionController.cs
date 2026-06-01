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
        [SerializeField] private float strength;
        private HandsMovement _handsMovement;

        public Rigidbody Pivot => _handsMovement.Pivot;
        public HandsMovement HandsMovement => _handsMovement;
        
        private bool _alignment = true; 
        public void DisableAlignment() => _alignment = false;
        public void EnableAlignment() => _alignment = true;

        public override void OnStartLocalPlayer()
        {
            InjectSelf();
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
                if (isLocalPlayer)
                {
                    item.gameObject.layer = LayerMask.NameToLayer("HeldItem");
                }
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
        
        private void FixedUpdate()
        {
            if (_heldItem && _heldItem.HasToBeAligned && _alignment)
            {
                var rb = _heldItem.Rigidbody;

                var targetRotation = Pivot.rotation;
                var desiredRotation = targetRotation;
                rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, desiredRotation, 360f * Time.fixedDeltaTime));
            }
        }
        public void ApplySwingImpulse(Vector3 force, Vector3 torque)
        {
            if (_heldItem)
            {
                
                var rb = _heldItem.Rigidbody;
                rb.AddForce(force, ForceMode.Impulse);
                rb.AddTorque(torque, ForceMode.Impulse);
            }
        }

    }
}