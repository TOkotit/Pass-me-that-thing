using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics;
using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Systems;
using UnityEngine.InputSystem;
using VContainer;

namespace Game.Scripts.GameFiles.Items
{
    using UnityEngine;
    using Mirror;

    public class PlayerInteraction : NetworkBehaviour
    {
        
        public InteractionZone interactionZone;
        
    
        private PlayerInventory inventory;
        private GameInput _gameInput;
        private PlayerInventoryModel _playerInventoryModel;
        private Camera _camera;
        private PhysicalItemRegistry _physicalItemRegistry;
        private bool _inTimeOut;
        private float lastInteractionTime;
        private float lastDropTime;
        
        [SerializeField] private PhysicalItemInteractionController _physicalItemInteractionController;
        [SerializeField] private LayerMask interactionLayer;
        [SerializeField] private float interactionDistance;
        [SerializeField] private float interactionTimeOut = 1;
       
        
        private List<Collider> targetsInRadius;
        
        [Inject]
        private void Construct(GameInputManager gameInputManager,  
            PlayerInventoryModel playerInventoryModel,
            PhysicalItemRegistry    physicalItemRegistry)
        {
            _gameInput = gameInputManager.GameInput;
            _playerInventoryModel = playerInventoryModel;
            _physicalItemRegistry = physicalItemRegistry;
        }

        public override void OnStartLocalPlayer()
        {
            inventory = GetComponent<PlayerInventory>();
            _camera = GetComponentInChildren<Camera>();
            targetsInRadius =  new List<Collider>();
            TrySubscribe();
        }
        
        public override void OnStopLocalPlayer()
        {
            TryUnsubscribe();
        }
        

        private void TrySubscribe()
        {
            if (_gameInput == null) {
                Debug.LogError($"[{gameObject.name}] GameInput is NULL during TrySubscribe!");
                return;
            }

            _gameInput.Gameplay.Interact.performed += OnInteract;
            _gameInput.Gameplay.RightMouse.canceled += OnDrop;
            _gameInput.Gameplay.RightMouse.performed += OnDropCharge;
            
            _gameInput.Gameplay.Item1.performed += Select1;
            _gameInput.Gameplay.Item2.performed += Select2;
            _gameInput.Gameplay.Item3.performed += Select3;

            interactionZone.OnInteractionZoneEnter += OnColliderEnter;
            interactionZone.OnInteractionZoneExit += OnColliderExit;
        }

        private void TryUnsubscribe()
        {
            if (_gameInput == null) return;

            try
            {
                _gameInput.Gameplay.Interact.performed -= OnInteract;
                _gameInput.Gameplay.RightMouse.canceled -= OnDrop;
                _gameInput.Gameplay.RightMouse.performed -= OnDropCharge;
                
                _gameInput.Gameplay.Item1.performed -= Select1;
                _gameInput.Gameplay.Item2.performed -= Select2;
                _gameInput.Gameplay.Item3.performed -= Select3;
                
                interactionZone.OnInteractionZoneEnter -= OnColliderEnter;
                interactionZone.OnInteractionZoneExit -= OnColliderExit;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to unsubscribe safely: {ex}");
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            var identity = GetComponent<NetworkIdentity>();
            if (!identity || !identity.isLocalPlayer) return;
            
            if (!interactionZone) return;
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(interactionZone.transform.position, 1f);
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            TryInteract();
        }
        
        private void OnDrop(InputAction.CallbackContext context)
        {
            if (Time.time - lastInteractionTime > interactionTimeOut)
            {
                lastInteractionTime = Time.time;
                float throwForce = _physicalItemInteractionController.HandsMovement.CurrentThrowForce;
                inventory.CmdDropItem(_playerInventoryModel.ActiveSlotIndex, throwForce);
            }
        }


        private void OnColliderEnter(Collider collider)
        {
            if (!targetsInRadius.Contains(collider))
                targetsInRadius.Add(collider);
            
            _playerInventoryModel.IsAbleInteract = targetsInRadius.Count > 0;
            if (targetsInRadius.Contains(collider) && targetsInRadius[0].TryGetComponent(out Outline outline))
                outline.enabled = true;
        }
        
        private void OnColliderExit(Collider collider)
        {
            // Debug.Log($"[{gameObject.name}] OnColliderExit: {collider.name}");
            targetsInRadius.Clear();
            
            _playerInventoryModel.IsAbleInteract = targetsInRadius.Count > 0;
            if (collider.TryGetComponent(out Outline outline))
                outline.enabled = false;
        }

        private void TryInteract()
        {
            Debug.LogWarning("Trying interaction");
            if (Time.time - lastInteractionTime > interactionTimeOut)
            { 
                lastInteractionTime = Time.time;
                var ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
                {
                    if (hit.collider.gameObject.CompareTag("Item"))
                    {
                        Debug.Log("Trying Pick Up");
                        TryPickUp(hit.collider);
                    }
                    else if (hit.collider.gameObject.CompareTag("Door"))
                    {
                        TryOpen(hit.collider);
                    }
                    else
                    {
                        hit.collider.gameObject.TryGetComponent(out IInteractable interactable);
                        if (interactable == null) return;
                        interactable.Interact();
                    }
                }

                if (!_playerInventoryModel.IsAbleInteract) return;
            }
        }

        private void OnDropCharge(InputAction.CallbackContext context)
        {
            _physicalItemInteractionController.ChargeDrop();
        }
        
        private void TryPickUp(Collider target)
        {
            var item = _physicalItemRegistry.TryGetItem(target.gameObject);
            Debug.Log("Trying Pick Up" + target.gameObject);
            if (item == _physicalItemInteractionController.CurrentHeldItem) return;
            inventory.CmdPickUpItem(item, _playerInventoryModel.ActiveSlotIndex);
            OnColliderExit(target);
        }

        private void TryOpen(Collider target)
        {
            var interactable = target.GetComponentInParent<IInteractable>();
            interactable?.Interact();
        }

        private void Select1(InputAction.CallbackContext context)
        {
            _playerInventoryModel.ActiveSlotIndex = 0;
            inventory.CmdDrawItem(0, _physicalItemInteractionController.Pivot.position);
        }
        
        private void Select2(InputAction.CallbackContext context)
        {
            _playerInventoryModel.ActiveSlotIndex = 1;
            inventory.CmdDrawItem(1, _physicalItemInteractionController.Pivot.position);
        }
        private void Select3(InputAction.CallbackContext context)
        {
            _playerInventoryModel.ActiveSlotIndex = 2;
            inventory.CmdDrawItem(2, _physicalItemInteractionController.Pivot.position);
        }
    }
}