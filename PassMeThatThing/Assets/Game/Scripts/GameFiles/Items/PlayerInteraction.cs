using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Entity;
using Game.Entity;
using Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics;
using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items.Highlight;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.Items
{

    public class PlayerInteraction : NetworkBehaviour
    {
        public InteractionZone interactionZone;

        private PlayerInventory inventory;
        private GameInput _gameInput;
        private PlayerInventoryModel _playerInventoryModel;
        private Camera _camera;
        private PhysicalItemRegistry _physicalItemRegistry;
        private OutlineRegistry _outlineRegistry;
        private DamagableRegistry _damagableRegistry;
        private bool _inTimeOut;
        private float lastInteractionTime;
        private float lastDropTime;

        [SerializeField] private PhysicalItemInteractionController _physicalItemInteractionController;
        [SerializeField] private MainCharacter mainCharacter;
        [SerializeField] private LayerMask interactionLayer;
        [SerializeField] private float interactionDistance;
        [SerializeField] private float interactionTimeOut = 1f;
        [Header("Swing Attack")]
        [SerializeField] private float swingForce = 5f;
        [SerializeField] private float swingTorque = 10f;
        [SerializeField] private float swingDuration = 0.2f;
        [SerializeField] private float swingCooldown = 0.8f;

        private float lastSwingTime = -999f;

        public float InteractionDistance => interactionDistance;
        public PhysicalItemInteractionController PhysicalItemInteractionController => _physicalItemInteractionController;

        [Inject]
        private void Construct(GameInputManager gameInputManager,
            PlayerInventoryModel playerInventoryModel,
            PhysicalItemRegistry physicalItemRegistry,
            OutlineRegistry outlineRegistry,
            DamagableRegistry damagableRegistry)
        {
            _gameInput = gameInputManager.GameInput;
            _playerInventoryModel = playerInventoryModel;
            _physicalItemRegistry = physicalItemRegistry;
            _outlineRegistry = outlineRegistry;
            _damagableRegistry = damagableRegistry;
        }

        #region Unity / Mirror methods
        public override void OnStartLocalPlayer()
        {
            _camera = GetComponentInChildren<Camera>();
            TrySubscribe();
        }

        private void Awake()
        {
            inventory = GetComponent<PlayerInventory>();
        }

        public override void OnStopLocalPlayer()
        {
            TryUnsubscribe();
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;

            if (_outlineRegistry.EnabledOutlines.Count > 1)
            {
                for (var i = _outlineRegistry.EnabledOutlines.Count - 1 - 1; i >= 0; i--)
                {
                    _outlineRegistry.DisableOutline(_outlineRegistry.EnabledOutlines[i]);
                }
            }

            var ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
            {
                if (_outlineRegistry.TryGetOutline(hit.collider.gameObject, out var outline))
                {
                    _outlineRegistry.EnableOutline(outline);
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            var identity = GetComponent<NetworkIdentity>();
            if (!identity || !identity.isLocalPlayer) return;
            
            if (!interactionZone) return;
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(interactionZone.transform.position, 1f);
        }
        #endregion

        #region Subscribes
        private void TrySubscribe()
        {
            if (_gameInput == null)
            {
                Debug.LogError($"[{gameObject.name}] GameInput is NULL during TrySubscribe!");
                return;
            }

            _gameInput.Gameplay.Interact.performed += OnInteract;
            _gameInput.Gameplay.RightMouse.canceled += OnDrop;
            _gameInput.Gameplay.RightMouse.performed += OnDropCharge;

            _gameInput.Gameplay.LeftMouse.performed += onActPerformed;
            _gameInput.Gameplay.LeftMouse.canceled += onActCanceled;

            _gameInput.Gameplay.Item1.performed += Select1;
            _gameInput.Gameplay.Item2.performed += Select2;
            _gameInput.Gameplay.Item3.performed += Select3;
        }

        private void TryUnsubscribe()
        {
            if (_gameInput == null) return;

            try
            {
                _gameInput.Gameplay.Interact.performed -= OnInteract;
                _gameInput.Gameplay.RightMouse.canceled -= OnDrop;
                _gameInput.Gameplay.RightMouse.performed -= OnDropCharge;

                _gameInput.Gameplay.LeftMouse.performed -= onActPerformed;
                _gameInput.Gameplay.LeftMouse.canceled -= onActCanceled;

                _gameInput.Gameplay.Item1.performed -= Select1;
                _gameInput.Gameplay.Item2.performed -= Select2;
                _gameInput.Gameplay.Item3.performed -= Select3;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to unsubscribe safely: {ex}");
            }
        }
        #endregion

        #region Callbacks / Handlers
        private void OnInteract(InputAction.CallbackContext context)
        {
            TryInteract();
        }

        private void OnDrop(InputAction.CallbackContext context)
        {
            Drop();
        }

        private void Select1(InputAction.CallbackContext context)
        {
            SelectSlot(0);
        }

        private void Select2(InputAction.CallbackContext context)
        {
            SelectSlot(1);
        }

        private void Select3(InputAction.CallbackContext context)
        {
            SelectSlot(2);
        }
        #endregion

        public void Drop()
        {
            if (Time.time - lastInteractionTime > interactionTimeOut)
            {
                lastInteractionTime = Time.time;
                var hands = _physicalItemInteractionController.HandsMovement;
                var throwForce = hands.CurrentThrowForce;
                var canThrow = hands.CanThrow;

                inventory.CmdDropItem(_playerInventoryModel.ActiveSlotIndex, throwForce, canThrow);
                hands.ResetCharge();
            }
        }

        private Interactable FindInteractable(GameObject obj)
        {
            Transform t = obj.transform;
            while (t)
            {
                if (InteractableRegistry.Instance.TryGetInteractable(t.gameObject, out var interactable))
                    return interactable;
                t = t.parent;
            }
            return null;
        }

        private void TryInteract()
        {
            Debug.LogWarning("Trying interaction");
            if (Time.time - lastInteractionTime > interactionTimeOut)
            {
                lastInteractionTime = Time.time;
                var ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, interactionDistance, interactionLayer))
                {
                    Debug.Log(hit.collider);
                    if (hit.collider.gameObject.CompareTag("Item"))
                    {
                        TryPickUp(hit.collider);
                    }
                    else if (hit.collider.gameObject.CompareTag("Player"))
                    {
                        _damagableRegistry.TryGetDamagable(hit.collider.gameObject, out var damagable);
                        Debug.Log(damagable);
                        if (damagable && damagable != mainCharacter)
                        {
                            if (_physicalItemInteractionController.CurrentHeldItem && damagable is MainCharacter player)
                            {
                                inventory.CmdGiveItemToPlayer(player);
                            }
                        }
                    }
                    else if (hit.collider.gameObject.CompareTag("Door"))
                    {
                        TryOpen(hit.collider);
                    }
                    else
                    {
                        if (InteractableRegistry.Instance.TryGetInteractable(hit.collider.gameObject, out var interactable))
                            interactable.Interact();
                    }
                }
            }
        }

        private void OnDropCharge(InputAction.CallbackContext context)
        {
            _physicalItemInteractionController.ChargeDrop();
        }

        public void TryPickUp(Collider target)
        {
            var item = _physicalItemRegistry.GetItem(target.gameObject);
            Debug.Log("Trying Pick Up" + target.gameObject);
            if (item == _physicalItemInteractionController.CurrentHeldItem) return;
            inventory.CmdPickUpItem(item, _playerInventoryModel.ActiveSlotIndex);
        }

        [Server]
        public void TryPickUp(PhysicalItem target)
        {
            Debug.Log("Trying Pick Up" + target.gameObject);
            if (target == _physicalItemInteractionController.CurrentHeldItem) return;
            Debug.Log(inventory);
            inventory.ServerPickUpItem(target, _playerInventoryModel.ActiveSlotIndex);
            if (_outlineRegistry.TryGetOutline(target.gameObject, out var outline))
            {
                _outlineRegistry.DisableOutline(outline);
            }
        }

        private void TryOpen(Collider target)
        {
            var interactable = FindInteractable(target.gameObject);
            if (!interactable) return;
            interactable.Interact();
        }

        private void onActPerformed(InputAction.CallbackContext context)
        {
            if (!PhysicalItemInteractionController.CurrentHeldItem) return;
            if (PhysicalItemInteractionController.CurrentHeldItem.Reaction != null)
            {
                PhysicalItemInteractionController.CurrentHeldItem.Reaction.Act();
                if (PhysicalItemInteractionController.CurrentHeldItem.CanBeOwned && 
                    PhysicalItemInteractionController.CurrentHeldItem.DoActAndSwing)
                {
                    StartCoroutine(SwingAttackCoroutine());
                }
            }
            else
            {
                if (PhysicalItemInteractionController.CurrentHeldItem.CanBeOwned)
                {
                    StartCoroutine(SwingAttackCoroutine());
                }
            }
            
        }

        private void onActCanceled(InputAction.CallbackContext context)
        {
        }

        private IEnumerator SwingAttackCoroutine()
        {
            if (Time.time - lastSwingTime < swingCooldown)
                yield break;

            lastSwingTime = Time.time;

            var controller = _physicalItemInteractionController;
            var item = controller.CurrentHeldItem;
            if (!item) yield break;

            controller.DisableAlignment();

            var forward = _camera.transform.forward;
            var right = _camera.transform.right;
            var force = forward * swingForce;
            var torque = right * swingTorque;

            controller.ApplySwingImpulse(force, torque);

            yield return new WaitForSeconds(swingDuration);
            controller.EnableAlignment();
        }

        private void SelectSlot(int index)
        {
            if (_physicalItemInteractionController.CurrentHeldItem && !_physicalItemInteractionController.CurrentHeldItem.CanBeOwned)
            {
                inventory.CmdDropItem(_playerInventoryModel.ActiveSlotIndex, 0, true);
            }

            if (index == _playerInventoryModel.ActiveSlotIndex)
            {
                _playerInventoryModel.ActiveSlotIndex = -1;
                inventory.CmdHideItem();
            }
            else
            {
                _playerInventoryModel.ActiveSlotIndex = index;
                inventory.CmdDrawItem(index, _physicalItemInteractionController.Pivot.position);
            }
        }
    }
}