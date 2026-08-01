 using System;
using DI;
using Game.Scripts.Enums;
using Game.Scripts.Systems;
using Mirror;
using Systems;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MainCharacterNetwork
{
    [RequireComponent(typeof(Camera))]
    public class MainCharacterCamera : MonoBehaviour
    {
        private const float standardSensitivity = 30f;

        [SerializeField] private Camera mCamera;
        [SerializeField] private float maxPitch = 80f;
        [SerializeField] private bool lockCursor = true;
        [SerializeField] private float tiltMultiplier = 0.2f;
        [SerializeField] private float zoomDistance = 5f;
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private Camera ragdollCamera;
        private GameInput _gameInput;
        private OptionsManager _optionsManager;
        private MainCharacterMovementController _movementController;
        private NetworkIdentity _ownerIdentity;
        private CameraState _cameraState;

        private float _sensitivity;
        private Vector2 _rotation;
        private bool _initialized;
        private bool _isLocalPlayer;
        private bool _isCameraRotating = true;
        private bool _isFirstPerson = true;
        private Quaternion _originalSpineLocalRotation;
        private Coroutine _zoomRoutine;

        private void SwitchToRagdollCamera()
        {
            if (mCamera) mCamera.enabled = false;
            if (ragdollCamera) ragdollCamera.enabled = true;
        }

        
        public bool IsCameraRotating
        {
            get => _isCameraRotating;
            set  => _isCameraRotating = value;
        }
        
        private void Awake()
        {
            if (!mCamera)
                mCamera = GetComponent<Camera>();

            _ownerIdentity = GetComponentInParent<NetworkIdentity>();
            _movementController = GetComponentInParent<MainCharacterMovementController>();
        }
        
        private void Start()
        {
            _isLocalPlayer = _ownerIdentity && _ownerIdentity.isLocalPlayer;

            if (!_isLocalPlayer)
            {
                if (mCamera)
                    mCamera.enabled = false;

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
        public void Construct(GameInputManager gameInputManager, OptionsManager optionsManager)
        {
            _gameInput = gameInputManager.GameInput;
            _optionsManager = optionsManager;

            SetSensitivity(optionsManager.OptionsData.mouseSensitivity);
            _optionsManager.OnSensitivityChanged += SetSensitivity;
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

            _rotation.x -= inputDelta.y * _sensitivity * 0.01f;
            _rotation.y += inputDelta.x * _sensitivity * 0.01f;

            _rotation.x = Mathf.Clamp(_rotation.x, -maxPitch, maxPitch);

            cameraRoot.localRotation = Quaternion.Euler(_rotation.x, 0f, 0f);

            var characterRotation = Quaternion.Euler(0f, _rotation.y, 0f);
            _movementController.ControllerRotate(characterRotation);

            var targetTilt = new Vector3(Mathf.Clamp(-_rotation.x * tiltMultiplier, -10f, 10f), 0f, 0f);
            
            /*if (_isLocalPlayer)
            {
                spinePivot.localRotation = Quaternion.Euler(targetTilt);
            }*/
        }

        public void SetupInput(GameInput input)
        {
            _gameInput = input;
            _initialized = true;
        }

        public void SetSensitivity(float sensitivityPercent)
        {
            _sensitivity = standardSensitivity * sensitivityPercent / 100;
        }

        private void OnDestroy()
        {
            _optionsManager.OnSensitivityChanged -= SetSensitivity;
        }
    }
}