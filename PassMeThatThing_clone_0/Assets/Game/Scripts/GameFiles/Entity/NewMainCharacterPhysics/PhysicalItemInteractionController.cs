using System;
using DI;
using Game.Scripts.Enums;
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
        
        private bool _subscribed;
        private Action<Vector3> OnPositionChanged;
        
        [SerializeField] private PhysicalItem _heldItem;
        private HandsMovement _handsMovement;
        public Rigidbody Pivot => _handsMovement.Pivot;
        public PhysicalItem CurrentHeldItem => _heldItem;

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
            if (!item) return;
            _heldItem = item;
            _handsMovement.GrabItem(item);
        }

        /*private void OnLeftMouseReleased(InputAction.CallbackContext context)
        {
            if (!_heldItem)
            {
                _handsMovement.ResetLeftHand();
            }
        }*/
        
        [Command]
        private void CmdGrabItem(PhysicalItem physicalItem)
        {
            if (physicalItem)
            {
                _handsMovement.GrabItem(physicalItem);
            }
        }
        
        
        [Command]
        private void CmdReleaseAndDrop(PhysicalItem item)
        {
            Debug.Log("Команда вызывается");
            if (!item) return;
            _handsMovement.ReleaseItem(item); 
        }

        public void SetHeldItem(PhysicalItem heldItem)
        {
            Debug.Log("Попытка перехватить предмет" + heldItem);
            _heldItem = heldItem;
            CmdGrabItem(heldItem);
        }

        public void ClearHeldItem()
        {
            _heldItem = null;
        }
    }
}