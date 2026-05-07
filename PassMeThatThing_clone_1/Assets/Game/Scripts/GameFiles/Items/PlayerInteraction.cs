using System;
using System.Collections.Generic;
using Game.Scripts.GameFiles.InteractableObjects;
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
        
        private List<Collider> targetsInRadius;
        
        [Inject]
        private void Construct(GameInputManager gameInputManager,  
            PlayerInventoryModel playerInventoryModel)
        {
            _gameInput = gameInputManager.GameInput;
            _playerInventoryModel = playerInventoryModel;
        }

        public override void OnStartLocalPlayer()
        {
            inventory = GetComponent<PlayerInventory>();
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
            _gameInput.Gameplay.Drop.performed += OnDrop;
            
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
                _gameInput.Gameplay.Drop.performed -= OnDrop;
                
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
            inventory.CmdDropItem(_playerInventoryModel.ActiveSlotIndex, interactionZone.transform.position);
        }

        // public void FixedUpdate()
        // {
        //     if (isLocalPlayer)
        //     {
        //         // var size = Physics.OverlapSphereNonAlloc(interactionZone.transform.position,
        //         //     interactionDistance, targetsInRadius, itemLayer);
        //     
        //         // _playerInventoryModel.IsAbleInteract = size > 0;
        //         
        //         
        //     }
        // }

        private void OnColliderEnter(Collider collider)
        {
            // Debug.Log($"{targetsInRadius.Count}");
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
            if (!_playerInventoryModel.IsAbleInteract) return;
            
            var target = targetsInRadius[0];
            if (target == null) return;
            
            if (target.CompareTag("Item"))
            {
                TryPickUp(target);
            }
            else if (target.CompareTag("Door"))
            {
                TryOpen(target);
            }
            else
            {
                target.TryGetComponent(out IInteractable interactable);
                if (interactable == null) return;
                interactable.Interact();
            }
        }

        private void TryPickUp(Collider target)
        {
            if (!target.TryGetComponent(out NetworkItem item)) return;
            inventory.CmdPickUpItem(item.gameObject);
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
        }
        
        private void Select2(InputAction.CallbackContext context)
        {
            _playerInventoryModel.ActiveSlotIndex = 1;
        }
        private void Select3(InputAction.CallbackContext context)
        {
            _playerInventoryModel.ActiveSlotIndex = 2;
        }
    }
}