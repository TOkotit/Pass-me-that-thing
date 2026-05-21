using System;
using DI;
using Mirror;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace MainCharacter
{
    public class MainCharacterMovementController : NetworkBehaviour
    {
        
        [SerializeField] private LocalVisionShaderApplier visionApplier;
        private MainCharacterMovement _controllable;
        private MainCharacterCamera _mainCamera;
        private GameInput _gameInput;
        
        private bool _subscribed; 
        private Vector3 _lastSentDirection;
        
        
        private void Awake()
        {
            _controllable = GetComponentInChildren<MainCharacterMovement>();

            if (!_controllable)
                Debug.LogError("IControllable not found on player");

            _mainCamera = GetComponentInChildren<MainCharacterCamera>(true);

            if (!_mainCamera)
                Debug.LogWarning("MainCharacterCamera not found in children");
        }
        
        [Inject]
        private void Construct(GameInputManager gameInputManager)
        {
            _gameInput = gameInputManager.GameInput;
        }
        
        public override void OnStartClient()
        {
            if (isLocalPlayer) return; 

            var allRbs = GetComponentsInChildren<Rigidbody>();
            foreach (var rb in allRbs)
            {
                rb.detectCollisions = false;
                var hasNetTransform = rb.TryGetComponent<NetworkTransformReliable>(out var netTransform);
                
                if (hasNetTransform)
                {
                    rb.isKinematic = true; 
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                }
                else
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                    rb.WakeUp();
                }
            }
        }
        
        public override void OnStartLocalPlayer()
        {
            _gameInput.Gameplay.Enable();

            if (_mainCamera)
            {
                _mainCamera.gameObject.SetActive(true);
                _mainCamera.SetupInput(_gameInput); 
            }

            TrySubscribe();
        }

        private void OnEnable()
        {
            if (isLocalPlayer) 
                TrySubscribe();
        }

        private void OnDisable()
        {
            if (isLocalPlayer)
                TryUnsubscribe();
        }

        private void TrySubscribe()
        {
            if (_subscribed) return;
            if (_gameInput == null) {
                Debug.LogError($"[{gameObject.name}] GameInput is NULL during TrySubscribe!");
                return;
            }

            _gameInput.Gameplay.Jump.performed += OnJumpPerformed;
            _gameInput.Gameplay.Sprint.started += OnSprintStarted;
            _gameInput.Gameplay.Sprint.canceled += OnSprintCanceled;
            _subscribed = true;
            Debug.Log($"<color=green>[{gameObject.name}] Subscribed to Input successfully");
        }
        
        private void TryUnsubscribe()
        {
            if (!_subscribed) return;
            if (_gameInput == null) return;

            try
            {
                _gameInput.Gameplay.Jump.performed -= OnJumpPerformed;
                _gameInput.Gameplay.Sprint.started -= OnSprintStarted;
                _gameInput.Gameplay.Sprint.canceled -= OnSprintCanceled;
                
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to unsubscribe safely: {ex}");
            }
            _subscribed = false;
        }
        
        private void Update()
        {
            if (!isLocalPlayer) return;

            ReadMovement();
            
            if (Input.GetKeyDown(KeyCode.V))
            {
                Debug.Log("<color=green>V</color>");
                visionApplier.EnableVision();
            }
    
            if (Input.GetKeyUp(KeyCode.V))
            {
                Debug.Log("<color=red>V</color>");
                visionApplier.DisableVision();
            }
            
            
        }
        
        private void ReadMovement()
        {
            if (_gameInput == null || !_mainCamera) return;

            var input = _gameInput.Gameplay.Movement.ReadValue<Vector2>();

            var cam = _mainCamera.transform;

            var forward = cam.forward;
            var right = cam.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            var moveDirection = forward * input.y + right * input.x;

            if (moveDirection == _lastSentDirection && moveDirection == Vector3.zero)
                return;
            _controllable.Move(moveDirection);
            _lastSentDirection = moveDirection;
        }
        
        
        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            _controllable.Jump();
        }

        private void OnSprintStarted(InputAction.CallbackContext context)
        {
            _controllable.SetSprinting(true);
        }

        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            _controllable.SetSprinting(false);
        }
        
        public void ControllerRotate(Quaternion rotation)
        {
            _controllable.Rotate(rotation);
        }
        
        // ================== COMMANDS ==================
        
        // [Command]
        // private void CmdJump()
        // {
        //     _controllable.Jump();
        // }
        //
        // [Command]
        // private void CmdSprintStarted()
        // {
        //     _controllable.SetSprinting(true); 
        // }
        //
        // [Command]
        // private void CmdSprintCanceled()
        // {
        //     _controllable.SetSprinting(false);
        // }
        //
        // [Command]
        // private void CmdMove(Vector3 direction)
        // {
        //     // Debug.Log($"<color=aliceblue>[{gameObject.name}] Moving to {direction}...");
        //     _controllable.Move(direction);
        // }
        //
        // [Command]
        // public void CmdRotate(Quaternion rotation)
        // {
        //     _controllable.Rotate(rotation);
        // }
        
        // ================== DI ==================
        
        private void OnDestroy()
        {
            if (isLocalPlayer && _gameInput != null)
            {
                TryUnsubscribe();
                _gameInput.Gameplay.Disable();
                Debug.Log("Local GameInput disposed");
            }
        }
    }
}