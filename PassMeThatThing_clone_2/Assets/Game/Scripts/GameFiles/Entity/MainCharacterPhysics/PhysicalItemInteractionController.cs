using System;
using DI;
using Entity;
using Game.Entity;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using Systems;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics
{
    public class PhysicalItemInteractionController : NetworkBehaviour
    {
        /*[SyncVar]
        private uint heldItemNetId;*/
        public PhysicalItem CurrentHeldItem => _heldItem;

        private bool _subscribed;
        private Action<Vector3> OnPositionChanged;
        
        [SerializeField] private PhysicalItem _heldItem;
        [SerializeField] private MainCharacter mainCharacter;
        private HandsMovement _handsMovement;
        private ItemPoolManager _itemPoolManager;
        private DamagableRegistry _damagableRegistry;
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
            {
                scope.Container.Inject(this);
            }
            else
            {
                Debug.LogError("GameplayScope not found!");
            }
        }
      
        public void ChargeDrop()
        {
            if (_heldItem)
            {
                _handsMovement.ChargeThrow();
            }
        }

        [ClientRpc]
        private void TargetDrop()
        {
            if (_heldItem)
            {
                if (_heldItem.CanBeOwned)
                {
                    _heldItem.Owner = null;
                }

                _heldItem = null;
            }
        }
        
        
        [Server]
        public void PhysicalPickUpItem(PhysicalItem item)
        {
            _heldItem = item;
            if (_heldItem.CanBeOwned) {
                var damagable = mainCharacter;
                _heldItem.Owner = damagable;
            }
            TargetPickUpItem(item);
            _handsMovement.GrabItem(item);
        }

        [TargetRpc]
        private void TargetPickUpItem(PhysicalItem item)
        {
            _heldItem = item;
            if (_heldItem && _heldItem.CanBeOwned) {
                var damagable = mainCharacter;
                _heldItem.Owner = damagable;
            }
        }
        
        [TargetRpc]
        public void TargetClearHeldItem()
        {
            if (_heldItem)
            {
                _heldItem.Owner = null;
                _heldItem = null;
            }
        }
        [Server]
        public void ServerClearHeldItem()
        {
            if (_heldItem)
            {
                _heldItem.Owner = null;
                _heldItem = null;
            }

            TargetClearHeldItem();
        }
        
        [Server]
        public void ReleaseCurrentItem(float throwForce, bool canThrow)
        {
            if (_heldItem)
            {
                _handsMovement.ReleaseItem(_heldItem, throwForce, canThrow); 
                _heldItem.Owner = null;
                _heldItem = null;
                TargetClearHeldItem();
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