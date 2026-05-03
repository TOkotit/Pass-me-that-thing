using System;
using DI;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics
{
    public class HandsController : NetworkBehaviour
    {
        
        private GameInput _gameInput;
        private bool _subscribed;
        private Action<Vector3> OnPositionChanged;
        private Vector3 position;
        public Vector3 Position
        {
            get => position;
            set
            {
                position = value;
                OnPositionChanged?.Invoke(position);
            }
        }
        
        private PhysicalItem heldItem;
        private HandsMovement _handsMovement;

        [Inject]
        private void Construct(GameInputManager gameInputManager)
        {
            _gameInput = gameInputManager.GameInput;
        }
        public override void OnStartLocalPlayer()
        {
            InjectSelf();
            _gameInput.Gameplay.Enable();
            TrySubscribe();
        }

        private void Awake()
        {
            _handsMovement = GetComponentInChildren<HandsMovement>();
        }
        
        private void OnDisable()
        {
            if (isLocalPlayer)
                TryUnsubscribe();
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
        
        private void OnEnable()
        {
            if (isLocalPlayer) 
                TrySubscribe();
        }
        private void TrySubscribe()
        {
            if (_subscribed) return;
            if (_gameInput == null) {
                Debug.LogError($"[{gameObject.name}] GameInput is NULL during TrySubscribe!");
                return;
            }

            _gameInput.Gameplay.RightMouse.performed += OnRightMousePressed;
            _gameInput.Gameplay.RightMouse.canceled += OnRightMouseReleased;
            _gameInput.Gameplay.LeftMouse.performed += OnLeftMousePressed;
            _gameInput.Gameplay.LeftMouse.canceled += OnLeftMouseReleased;
            
                _subscribed = true;
            Debug.Log($"<color=green>[{gameObject.name}] Subscribed to Input successfully");
        }

        private void TryUnsubscribe()
        {
            _gameInput.Gameplay.RightMouse.performed -= OnRightMousePressed;
            _gameInput.Gameplay.RightMouse.canceled -= OnRightMouseReleased;
            _gameInput.Gameplay.LeftMouse.performed -= OnLeftMousePressed;
            _gameInput.Gameplay.LeftMouse.canceled -= OnLeftMouseReleased;
        }
        
        private void OnRightMousePressed(InputAction.CallbackContext context)
        {
            if (!heldItem)
            {
                _handsMovement.Move(Hand.Right);
            }
        }

        private void OnRightMouseReleased(InputAction.CallbackContext context)
        {
            if (!heldItem)
            {
                _handsMovement.ResetRightHand();
            }
        }
        private void OnLeftMousePressed(InputAction.CallbackContext context)
        {
            if (!heldItem)
            {
                
                _handsMovement.Move(Hand.Left);
            }
        }

        private void OnLeftMouseReleased(InputAction.CallbackContext context)
        {
            if (!heldItem)
            {
                _handsMovement.ResetLeftHand();
            }
        }
        private void Update()
        {
            
        }
    }
}