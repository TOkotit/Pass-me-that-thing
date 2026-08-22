using System;
using DI;
using Game.Entity;
using Game.Scripts.GameFiles.Entity.MainCharacterNetwork;
using Mirror;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using VContainer;
using VContainer.Unity;

namespace MainCharacterNetwork
{
    public class MainCharacterMovementController : NetworkBehaviour
    {
        [SerializeField] private UniversalRendererData rendererData;
        private ScriptableRendererFeature _visionFeature;
        private ScriptableRendererFeature _outlineFeature;
        private bool _isVisionEnabled = false;

        private IControllable _controllable;
        private IControllable _defaultControllable;
        private MainCharacterCamera _mainCamera;
        private GameInput _gameInput;
        private MCLocalModel _mcLocalModel;
        private bool _subscribed;
        private Vector3 _lastSentDirection;

        private void Awake()
        {
            var movement = GetComponentInChildren<IControllable>();
            if (movement != null)
            {
                _controllable = movement;
                _defaultControllable = movement;
            }
            else
            {
                Debug.LogError("MainCharacterMovement not found on player");
            }

            _mainCamera = GetComponentInChildren<MainCharacterCamera>(true);
            if (!_mainCamera)
                Debug.LogWarning("MainCharacterCamera not found in children");
        }

        private void Start()
        {
            _visionFeature = rendererData.rendererFeatures.Find(f => f.name == "VisionEffect");
            _outlineFeature = rendererData.rendererFeatures.Find(f => f.name == "GlobalOutlines");
            SetVisionState(false);
        }

        [Inject]
        private void Construct(GameInputManager gameInputManager, MCLocalModel mcLocalModel)
        {
            _gameInput = gameInputManager.GameInput;
            _mcLocalModel = mcLocalModel;
        }

        public override void OnStartClient()
        {
            if (isLocalPlayer) return;

            var allRbs = GetComponentsInChildren<Rigidbody>();
            foreach (var rb in allRbs)
            {
                rb.detectCollisions = true;
                rb.isKinematic = true;
                if (rb.TryGetComponent<NetworkTransformReliable>(out _))
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                else
                    rb.interpolation = RigidbodyInterpolation.None;
                rb.useGravity = false;
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

            SetVisionState(false);
        }

        private void TrySubscribe()
        {
            if (_subscribed) return;
            if (_gameInput == null)
            {
                Debug.LogError($"[{gameObject.name}] GameInput is NULL during TrySubscribe!");
                return;
            }

            _gameInput.Gameplay.Jump.performed += OnJumpPerformed;
            _gameInput.Gameplay.Sprint.started += OnSprintStarted;
            _gameInput.Gameplay.Sprint.canceled += OnSprintCanceled;
            _gameInput.Gameplay.Control.started += OnControlStarted;
            _gameInput.Gameplay.Control.canceled += OnControlCanceled;
            _subscribed = true;
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
                _gameInput.Gameplay.Control.started -= OnControlStarted;
                _gameInput.Gameplay.Control.canceled -= OnControlCanceled;
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
            _mcLocalModel?.ReportPlayerPosition(transform.position);

            if (Input.GetKeyDown(KeyCode.V))
            {
                _isVisionEnabled = !_isVisionEnabled;
                SetVisionState(_isVisionEnabled);
            }
        }

        private void SetVisionState(bool state)
        {
            if (_visionFeature) _visionFeature.SetActive(state);
            if (_outlineFeature) _outlineFeature.SetActive(state);
        }

        private void ReadMovement()
        {
            if (_gameInput == null || !_mainCamera || _controllable == null) return;

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
            _controllable?.Jump();
        }

        private void OnSprintStarted(InputAction.CallbackContext context)
        {
            _controllable?.SetSprinting(true);
        }

        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            _controllable?.SetSprinting(false);
        }

        public void ControllerRotate(Quaternion rotation)
        {
            _controllable?.Rotate(rotation);
        }
        private void OnControlStarted(InputAction.CallbackContext context)
        {
            _controllable?.Control(true);
        }

        private void OnControlCanceled(InputAction.CallbackContext context)
        {
            _controllable?.Control(false);
        }
        public void SetControllable(IControllable newControllable)
        {
            if (newControllable != null)
            {
                _controllable = newControllable;
                _lastSentDirection = Vector3.zero;
            }
        }

        public void ResetToDefaultControllable()
        {
            _controllable = _defaultControllable;
            _lastSentDirection = Vector3.zero;
        }

        private void OnDestroy()
        {
            SetVisionState(false);

            if (isLocalPlayer && _gameInput != null)
            {
                TryUnsubscribe();
                _gameInput.Gameplay.Disable();
            }
        }
    }
}