using System;
using DI;
using Mirror;
using Systems;
using UnityEngine;
using VContainer;

namespace MainCharacter
{
    [RequireComponent(typeof(Camera))]
    public class MainCharacterCamera : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _sensitivity = 1f;
        [SerializeField] private float _maxPitch = 80f;
        [SerializeField] private bool _lockCursor = true;
        
        private GameInput _gameInput;
        private MainCharacterMovementController _movementController;
        private NetworkIdentity _ownerIdentity;

        private Vector2 _rotation;
        private bool _initialized;
        private bool _isLocalPlayer;
        private bool _isCameraRotating = true;

        public bool IsCameraRotating
        {
            get => _isCameraRotating;
            set => _isCameraRotating = value;
        }

        private void Awake()
        {
            if (!_camera)
                _camera = GetComponent<Camera>();

            _ownerIdentity = GetComponentInParent<NetworkIdentity>();
            _movementController = GetComponentInParent<MainCharacterMovementController>();
        }
        
        private void Start()
        {
            _isLocalPlayer = _ownerIdentity != null && _ownerIdentity.isLocalPlayer;

            if (!_isLocalPlayer)
            {
                if (_camera != null)
                    _camera.enabled = false;

                enabled = false;
                return;
            }

            if (_lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            _initialized = true;
        }
        
        
        [Inject]
        public void Construct(GameInputManager gameInputManager)
        {
            _gameInput = gameInputManager.GameInput;
        }

        private void LateUpdate()
        {
            if (!_isLocalPlayer || !_initialized)
                return;

            if (!_isCameraRotating)
                return;

            ReadRotation();
        }

        private void ReadRotation()
        {
            if (_gameInput == null || _movementController == null)
                return;

            var inputDelta = _gameInput.Gameplay.MouseDrag.ReadValue<Vector2>();

            _rotation.x -= inputDelta.y * _sensitivity * 0.01f;
            _rotation.y += inputDelta.x * _sensitivity * 0.01f;

            _rotation.x = Mathf.Clamp(_rotation.x, -_maxPitch, _maxPitch);

            transform.localRotation = Quaternion.Euler(_rotation.x, 0f, 0f);

            var characterRotation = Quaternion.Euler(0f, _rotation.y, 0f);
            _movementController.transform.rotation = characterRotation; 

            _movementController.CmdRotate(characterRotation);
        }
        
        public void SetupInput(GameInput input)
        {
            _gameInput = input;
            _initialized = true;
        }
    }
}