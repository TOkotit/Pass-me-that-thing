using System;
using Systems;
using UnityEngine.InputSystem;
using VContainer;

namespace Game.Scripts.GameFiles.Items
{
    using UnityEngine;
    using Mirror;

    public class PlayerInteraction : NetworkBehaviour
    {
        public float interactionDistance = 3f;
        public LayerMask itemLayer;
    
        private PlayerInventory inventory;
        private GameInput _gameInput;
        private PlayerInventoryModel _playerInventoryModel;
        
        [Inject]
        private void Construct(GameInputManager gameInputManager,  
            PlayerInventoryModel playerInventoryModel)
        {
            _gameInput = gameInputManager.GameInput;
            _playerInventoryModel = playerInventoryModel;
        }
        
        void Start()
        {
            inventory = GetComponent<PlayerInventory>();
            
            
            if (isLocalPlayer) 
                TrySubscribe();
        }

        private void OnDestroy()
        {
            if (isLocalPlayer) 
                TryUnsubscribe();
        }

        private void TrySubscribe()
        {
            if (_gameInput == null) {
                Debug.LogError($"[{gameObject.name}] GameInput is NULL during TrySubscribe!");
                return;
            }

            _gameInput.Gameplay.Interact.performed += OnPickUp;
            _gameInput.Gameplay.Drop.performed += OnDrop;
            
            _gameInput.Gameplay.Item1.performed += Select1;
            _gameInput.Gameplay.Item2.performed += Select2;
            _gameInput.Gameplay.Item3.performed += Select3;
        }

        private void TryUnsubscribe()
        {
            if (_gameInput == null) return;

            try
            {
                _gameInput.Gameplay.Interact.performed -= OnPickUp;
                _gameInput.Gameplay.Drop.performed -= OnDrop;
                
                _gameInput.Gameplay.Item1.performed -= Select1;
                _gameInput.Gameplay.Item2.performed -= Select2;
                _gameInput.Gameplay.Item3.performed -= Select3;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to unsubscribe safely: {ex}");
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, 2);
        }


        private void OnPickUp(InputAction.CallbackContext context)
        {
            TryPickUp();
        }

        private void OnDrop(InputAction.CallbackContext context)
        {
            inventory.CmdDropItem(_playerInventoryModel.ActiveSlotIndex);
        }
        
        void TryPickUp()
        {
            var targetsInRadius = new Collider[10];
            var size = Physics.OverlapSphereNonAlloc(transform.position,
                interactionDistance, targetsInRadius, itemLayer);
            
            if (size >0)
            {
                if (targetsInRadius[0].TryGetComponent(out NetworkItem item))
                {
                    Debug.Log("Trying to pick up an item");
                    inventory.CmdPickUpItem(item.gameObject);
                }
            }
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