using System;
using DI;
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
        private HandsMovement _handsMovement;
        private ItemPoolManager _itemPoolManager;
        public Rigidbody Pivot => _handsMovement.Pivot;

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
        [Server]
        public void Drop()
        {
            if (_heldItem)
            {
                _heldItem.Network.netIdentity.RemoveClientAuthority();
                CmdReleaseAndDrop(_heldItem);
                _heldItem = null;
                TargetDrop();
            }
        }

        [ClientRpc]
        private void TargetDrop()
        {
            _heldItem = null;
        }
        
        
        [Server]
        public void PhysicalPickUpItem(PhysicalItem item)
        {
            _heldItem = item;
            TargetPickUpItem(item);
            item.Network.netIdentity.AssignClientAuthority(connectionToClient);
            _handsMovement.GrabItem(item);
        }

        [TargetRpc]
        private void TargetPickUpItem(PhysicalItem item)
        {
            _heldItem = item;
        }
        
        
        [Command]
        private void CmdReleaseAndDrop(PhysicalItem item)
        {
            Debug.Log("Команда вызывается");
            if (!item) return;
            _handsMovement.ReleaseItem(item);
            if (item.Network.netIdentity.connectionToClient != null)
            {
                item.Network.netIdentity.RemoveClientAuthority();
            }
            ServerClearHeldItem();  
        }
        [TargetRpc]
        public void TargetClearHeldItem()
        {
            _heldItem = null;
        }
        [Server]
        public void ServerClearHeldItem()
        {
            _heldItem = null;
            TargetClearHeldItem();
        }
        
        [Server]
        public void ReleaseCurrentItem()
        {
            if (_heldItem)
            {
                _handsMovement.ReleaseItem(_heldItem);
                _heldItem = null; 
                TargetClearHeldItem(); 
            }
        }
        
        [TargetRpc]
        public void TargetSyncPositionForDrop(NetworkConnection target, Vector3 position, Quaternion rotation)
        {
            if (_heldItem)
            {
                _heldItem.transform.SetPositionAndRotation(position, rotation);
            }
        }
    }
}