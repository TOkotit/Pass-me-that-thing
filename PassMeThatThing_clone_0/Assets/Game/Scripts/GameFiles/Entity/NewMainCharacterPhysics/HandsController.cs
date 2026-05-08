using System;
using DI;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using Systems;
using UnityEditor;
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
        
        private PhysicalItem heldItem;
        private HandsMovement _handsMovement;
        private Camera _camera;
        private PhysicalItemRegistry _physicalItemRegistry; 
        [SerializeField] LayerMask itemLayer;
        [SerializeField] float interactionDistance;

        [Inject]
        private void Construct(GameInputManager gameInputManager, PhysicalItemRegistry physicalItemRegistry)
        {
            _gameInput = gameInputManager.GameInput;
            _physicalItemRegistry = physicalItemRegistry;
        }
        public override void OnStartLocalPlayer()
        {
            InjectSelf();
            _gameInput.Gameplay.Enable();
            TrySubscribe();
        }

        private void Start()
        {
            _handsMovement = GetComponentInChildren<HandsMovement>();
            Debug.Log("_handsMovement" +  _handsMovement);
            _camera = GetComponentInChildren<Camera>();
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
                
            }
            else
            {
                _handsMovement.ChargeThrow();
            }
        }

        private void OnRightMouseReleased(InputAction.CallbackContext context)
        {
            if (!heldItem)
            {
                _handsMovement.ResetRightHand();
            }
            else
            {
                if (heldItem)
                {
                    _handsMovement.ReleaseItem(heldItem);
                    heldItem = null;
                }
            }
        }
        private void OnLeftMousePressed(InputAction.CallbackContext context)
        {
            var ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, interactionDistance, itemLayer))
            {
                var itemObject = hit.collider.gameObject;
                Debug.Log(itemObject.name);
                if (itemObject)
                {
                    var physicalItem = _physicalItemRegistry.TryGetItem(itemObject);
                    Debug.Log("предмет найден: "+physicalItem);
                    CmdGrabItem(physicalItem);
                    heldItem = physicalItem;
                }
            }
            if (!heldItem)
            {
                //логика движения руки без предмета, будет на настоящей модели, без нее не целесообразно
            }
        }

        private void OnLeftMouseReleased(InputAction.CallbackContext context)
        {
            if (!heldItem)
            {
                _handsMovement.ResetLeftHand();
            }
        }
        
        [Command]
        private void CmdGrabItem(PhysicalItem physicalItem)
        {
            if (physicalItem)
            {
                _handsMovement.GrabItem(physicalItem);
            }
        }
        
        private void Update()
        {
            
        }
    }
}