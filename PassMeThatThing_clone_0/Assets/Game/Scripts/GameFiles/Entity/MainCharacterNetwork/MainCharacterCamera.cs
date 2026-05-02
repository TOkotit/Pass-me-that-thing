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
        [SerializeField] private float sensitivity = 1f;
        [SerializeField] private float maxPitch = 80f;
        [SerializeField] private bool lockCursor = true;
        [SerializeField] private float tiltMultiplier = 0.2f;
        [SerializeField] BodyVerticalAlign bodyVerticalAlign;
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
            _isLocalPlayer = _ownerIdentity && _ownerIdentity.isLocalPlayer;

            if (!_isLocalPlayer)
            {
                if (_camera)
                    _camera.enabled = false;

                enabled = false;
                return;
            }

            if (lockCursor)
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
            if (_gameInput == null || !_movementController)
                return;

            var inputDelta = _gameInput.Gameplay.MouseDrag.ReadValue<Vector2>();

            _rotation.x -= inputDelta.y * sensitivity * 0.01f;
            _rotation.y += inputDelta.x * sensitivity * 0.01f;

            _rotation.x = Mathf.Clamp(_rotation.x, -maxPitch, maxPitch);

            transform.localRotation = Quaternion.Euler(_rotation.x, _rotation.y, 0f);

            var characterRotation = Quaternion.Euler(0f, _rotation.y, 0f);
                
            _movementController.CmdRotate(characterRotation);
            bodyVerticalAlign.SetTilt(new Vector3(Math.Clamp(-10 ,-_rotation.x * tiltMultiplier,10) , 0f, 0f));
        }
        
        public void SetupInput(GameInput input)
        {
            _gameInput = input;
            _initialized = true;
        }

    }
}