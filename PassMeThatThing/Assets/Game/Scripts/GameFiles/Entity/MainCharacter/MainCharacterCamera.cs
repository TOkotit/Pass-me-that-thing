using System;
using Systems;
using UnityEngine;
using VContainer;

namespace MainCharacter
{
    [RequireComponent(typeof(Camera))]
    public class MainCharacterCamera : MonoBehaviour
    {
        public Camera _camera;
        private GameInput _gameInput;
        private IControllable _controllable;
        private bool _initialized;
        [SerializeField] private float _sensitivity = 1f; 
        [SerializeField] private float _maxPitch = 80f;  
        private Vector2 _rotation;
        private bool isCameraRotating = false;

        public bool IsCameraRotating
        {
            get => isCameraRotating;
            set => isCameraRotating = value;
        }

        private void Awake()
        { 
            _camera = GetComponent<Camera>();
        }
        void Start()
        {
            isCameraRotating = true;
        }
        
        [Inject]
        public void Construct(GameInputManager gameInputManager, IControllable controllable)
        {
            _gameInput = gameInputManager.GameInput;
            _controllable = controllable;
            _initialized = true;
        }

        private void LateUpdate()
        {
            if (!_initialized) return;

            if (isCameraRotating)
                ReadRotation();
        }

        private void ReadRotation()
        {
            var inputDelta = _gameInput.Gameplay.MouseDrag.ReadValue<Vector2>();
            RotateInternal(inputDelta);
            // var cameraInput = new Vector3(0, inputDelta.x, 0);
            var characterRotation = Quaternion.Euler(0f, _rotation.y, 0f);
            _controllable.Rotate(characterRotation);
        }

        private void RotateInternal(Vector2 inputDelta)
        {
            _rotation.x -= inputDelta.y * _sensitivity * 0.01f; 
            _rotation.y += inputDelta.x * _sensitivity * 0.01f; 

            _rotation.x = Mathf.Clamp(_rotation.x, -_maxPitch, _maxPitch);
            transform.localRotation = Quaternion.Euler(_rotation.x, 0f, 0f);
        }
    }
}