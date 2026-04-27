using System;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace MainCharacter
{
    public class MainCharacterMovementController : MonoBehaviour
    {
        [Inject] private IControllable _controllable;
        private GameInputManager _gameInputManager;
        private GameInput _gameInput;
        [Inject] private MainCharacterCamera _mainCamera;



        private bool isAbleMove = true;
        private bool _subscribed = false;
        
        
        [Inject] 
        private void Construct(GameInputManager gameInputManager)
        {
            Debug.Log("MainCharacterInputController.Construct called");
            _gameInputManager = gameInputManager;
            _gameInput = gameInputManager.GameInput;
            Debug.Log($"_gameInput is null? {_gameInput == null}");
            TrySubscribe();
        }
        private void Awake()
        {
            _controllable = GetComponent<IControllable>();
            if (_controllable == null)
            {
                Debug.Log("Main character controllable is not found");
            }
        }
        private void OnEnable()
        {
            TrySubscribe();
        }
        
        private void OnDisable()
        {
            TryUnsubscribe();
        }
        
        private void TrySubscribe()
        {
            if (_subscribed) return;
            if (_gameInput == null) return;
            if (_gameInput.Gameplay.Jump == null) return;

            _gameInput.Gameplay.Jump.performed += OnJumpPerformed;
            _gameInput.Gameplay.Dash.performed += OnDashPerformed;
            _subscribed = true;
        }
        
        private void TryUnsubscribe()
        {
            if (!_subscribed) return;
            if (_gameInput == null) return;

            try
            {
                _gameInput.Gameplay.Jump.performed -= OnJumpPerformed;
                _gameInput.Gameplay.Dash.performed -= OnDashPerformed;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to unsubscribe safely: {ex}");
            }

            _subscribed = false;
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            _controllable.Jump();
        }

        private void OnDashPerformed(InputAction.CallbackContext context)
        {
            _controllable.Dash();
        }

        private void Update()
        {
            ReadMovement();
        }

        private void ReadMovement()
        {
            if (_gameInput == null)
            {
                return;
            }
            
            
            var inputDirection = _gameInput.Gameplay.Movement.ReadValue<Vector2>();
            
            if (!_mainCamera)
            {
                Debug.LogWarning("MainCamera is null in MainCharacterInputController.ReadMovement");
                return;
            }
            
            var cameraTransform = _mainCamera.transform;
            var forward = cameraTransform.forward;
            var right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            var moveDirection = forward * inputDirection.y + right * inputDirection.x;

            _controllable.Move(moveDirection);
        }
 
    }
}