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
        public void Drop()
        {
            if (_heldItem)
            {
                Debug.Log("Метод вызывается");
                CmdReleaseAndDrop(_heldItem);
                _heldItem = null;
            }
        }
        [Server]
        public void PhysicalPickUpItem(PhysicalItem item)
        {
            _heldItem = item;
            item.Network.netIdentity.AssignClientAuthority(connectionToClient);
            _handsMovement.GrabItem(item);
        }
        
        
        
        [Command]
        private void CmdReleaseAndDrop(PhysicalItem item)
        {
            Debug.Log("Команда вызывается");
            if (!item) return;
            _handsMovement.ReleaseItem(item);
            ServerClearHeldItem();  
        }

        public void ClearHeldItem()
        {
            _heldItem = null;
        }
        [Server]
        public void ServerClearHeldItem()
        {
            _heldItem = null;
        }
    }
}