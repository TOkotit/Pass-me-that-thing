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
        
        private PhysicalItem heldItem;
        private HandsMovement _handsMovement;
        private Camera _camera;
        
        
        private PhysicalItemRegistry _physicalItemRegistry; 

        [Inject]
        private void Construct(GameInputManager gameInputManager, PhysicalItemRegistry physicalItemRegistry)
        {
            _physicalItemRegistry = physicalItemRegistry;
        }
        public override void OnStartLocalPlayer()
        {
            InjectSelf();
        }

        private void Start()
        {
            _handsMovement = GetComponentInChildren<HandsMovement>();
            _camera = GetComponentInChildren<Camera>();
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
            if (heldItem)
            {
                _handsMovement.ChargeThrow();
            }
        }
        public void Drop()
        {
            if (heldItem)
            {
                Debug.Log("Метод вызывается");
                float force = _handsMovement.GetCurrentThrowForce(); // нужно добавить этот метод
                CmdReleaseAndDrop(heldItem, force);
                heldItem = null;
            }
        }
        public void PhysicalPickUpItem(PhysicalItem item)
        {
            //var ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            //RaycastHit hit;
            //if (Physics.Raycast(ray, out hit, interactionDistance, itemLayer))
            if (item && !heldItem)
            { 
                CmdGrabItem(item);
                heldItem = item;
            }
        }

        /*private void OnLeftMouseReleased(InputAction.CallbackContext context)
        {
            if (!heldItem)
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
        private void CmdReleaseAndDrop(PhysicalItem item, float force)
        {
            Debug.Log("Команда вызывается");
            if (!item) return;
            _handsMovement.ReleaseItem(item, force); 
        }
        private void Update()
        {
            
        }
    }
}