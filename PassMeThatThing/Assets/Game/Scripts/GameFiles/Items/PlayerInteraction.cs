using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DI;
using Entity;
using Game.Entity;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics;
using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items.Highlight;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Game.Scripts.GameFiles.Items.Refill;
using Mirror;
using Systems;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.Items
{
    public class PlayerInteraction : NetworkBehaviour
    {
        private PlayerInventory inventory;
        private GameInput _gameInput;
        private PlayerInventoryModel _playerInventoryModel;
        private PhysicalItemRegistry _physicalItemRegistry;
        private OutlineRegistry _outlineRegistry;
        private DamagableRegistry _damagableRegistry;
        private bool _inTimeOut;
        private Coroutine _currentAction;
        private float _lastInteractionTime;
        private float _lastDropTime;

        [SerializeField] private PhysicalItemInteractionController _physicalItemInteractionController;
        [SerializeField] private MainCharacter mainCharacter;
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask interactionLayer;
        [SerializeField] private float interactionDistance;
        [SerializeField] private float interactionTimeOut = 1f;

        [Header("Swing Attack")]
        [SerializeField] private float swingCooldown = 0.8f;

        private float _lastSwingTime = -999f;
        private bool _leftMouseHeld;

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
            TrySubscribe();
        }

        private void Awake()
        {
            var gameplayScope = LifetimeScope.Find<GameplayScope>();
            if (gameplayScope) gameplayScope.Container.Inject(this);
            inventory = GetComponent<PlayerInventory>();
        }

        public override void OnStopLocalPlayer()
        {
            TryUnsubscribe();
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;
            if (_leftMouseHeld)
            {
                var currentItem = _physicalItemInteractionController.CurrentHeldItem;
                if (currentItem && currentItem.LmbReaction && currentItem.LmbReaction.IsContinuous)
                {
                    currentItem.LmbReaction.Act();
                }
            }
            if (_outlineRegistry.EnabledOutlines.Count > 1)
            {
                for (var i = _outlineRegistry.EnabledOutlines.Count - 2; i >= 0; i--)
                {
                    _outlineRegistry.DisableOutline(_outlineRegistry.EnabledOutlines[i]);
                }
            }

            var ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayer))
            {
                if (_outlineRegistry.TryGetOutline(hit.collider.gameObject, out var outline))
                {
                    _outlineRegistry.EnableOutline(outline);
                }
            }
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
            _gameInput.Gameplay.Reload.performed += OnReload;
            _gameInput.Gameplay.Interact.performed += OnInteract;
            _gameInput.Gameplay.Drop.canceled += OnDrop;
            _gameInput.Gameplay.Drop.performed += OnDropCharge;
            _gameInput.Gameplay.LeftMouse.performed += OnActPerformed;
            _gameInput.Gameplay.LeftMouse.canceled += OnActCanceled;
            _gameInput.Gameplay.Item1.performed += Select1;
            _gameInput.Gameplay.Item2.performed += Select2;
            _gameInput.Gameplay.Item3.performed += Select3;
            _gameInput.Gameplay.SkipWave.performed += SkipWave;
        }

        private void SkipWave(InputAction.CallbackContext context)
        {
            CmdRequestSkipPreparation();
        }

        [Command]
        private void CmdRequestSkipPreparation()
        {
            GlobalStageManager.GlobalStageManager.Instance?.CmdSkipPreparation(netIdentity);
        }

        private void TryUnsubscribe()
        {
            if (_gameInput == null) return;
            try
            {
                _gameInput.Gameplay.Reload.performed -= OnReload;
                _gameInput.Gameplay.Interact.performed -= OnInteract;
                _gameInput.Gameplay.Drop.canceled -= OnDrop;
                _gameInput.Gameplay.Drop.performed -= OnDropCharge;
                _gameInput.Gameplay.LeftMouse.performed -= OnActPerformed;
                _gameInput.Gameplay.LeftMouse.canceled -= OnActCanceled;
                _gameInput.Gameplay.Item1.performed -= Select1;
                _gameInput.Gameplay.Item2.performed -= Select2;
                _gameInput.Gameplay.Item3.performed -= Select3;
                _gameInput.Gameplay.SkipWave.performed -= SkipWave;
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
            InterruptCurrentAction();
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
            if (Time.time - _lastInteractionTime > interactionTimeOut)
            {
                _lastInteractionTime = Time.time;
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
            _currentAction = null;
            if (Time.time - _lastInteractionTime > interactionTimeOut)
            {
                _lastInteractionTime = Time.time;
                var ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
                if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayer))
                {
                    var worldPoint = hit.point;
                    var hitTransform = hit.collider.transform;
                    var localPoint = hitTransform.InverseTransformPoint(worldPoint);

                    if (hit.collider.gameObject.CompareTag("Item"))
                    {
                        TryPickUp(hit.collider, localPoint);
                    }
                    else if (hit.collider.gameObject.CompareTag("InteractableItem"))
                    {
                        var item = _physicalItemInteractionController.CurrentHeldItem;
                        if (item)
                        {
                            if (InteractableRegistry.Instance.TryGetInteractable(hit.collider.gameObject, out var interactable))
                                CmdInteractWithItem(hit.collider.gameObject, item);
                        }
                        else
                        {
                            TryPickUp(hit.collider, localPoint);
                        }
                    }
                    else if (hit.collider.gameObject.CompareTag("Player"))
                    {
                        _damagableRegistry.TryGetDamagable(hit.collider.gameObject, out var damagable);
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
            InterruptCurrentAction();
            _physicalItemInteractionController.ChargeDrop();
        }

        public void TryPickUp(Collider target, Vector3 localPoint)
        {
            InterruptCurrentAction();
            var item = _physicalItemRegistry.GetItem(target.gameObject);
            if (item == _physicalItemInteractionController.CurrentHeldItem) return;
            inventory.CmdPickUpItem(item, _playerInventoryModel.ActiveSlotIndex, localPoint);
        }

        [Server]
        public void TryPickUp(PhysicalItem target, Vector3 localPoint)
        {
            if (target == _physicalItemInteractionController.CurrentHeldItem) return;
            inventory.ServerPickUpItem(target, _playerInventoryModel.ActiveSlotIndex, localPoint);
            if (_outlineRegistry.TryGetOutline(target.gameObject, out var outline))
            {
                _outlineRegistry.DisableOutline(outline);
            }
        }

        private void TryOpen(Collider target)
        {
            var interactable = FindInteractable(target.gameObject);
            if (interactable == null) return;
            interactable.Interact();
        }

        private void OnActPerformed(InputAction.CallbackContext context)
        {
            InterruptCurrentAction();
            var currentItem = _physicalItemInteractionController.CurrentHeldItem;
            if (!currentItem) return;

            if (currentItem.LmbReaction)
            {
                if (currentItem.LmbReaction.IsContinuous)
                {
                    _leftMouseHeld = true;
                }
                else
                {
                    currentItem.LmbReaction.Act();
                    if (currentItem.CanBeOwned && currentItem.DoActAndSwing)
                        CmdSwing();
                }
            }
            else
            {
                if (currentItem.CanBeOwned)
                    CmdSwing();
            }
        }

        private void OnActCanceled(InputAction.CallbackContext context)
        {
            _leftMouseHeld = false;
        }

        private void OnReload(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer) return;
            var currentItem = _physicalItemInteractionController.CurrentHeldItem;
            if (!currentItem || !currentItem.ReloadReaction) return;

            if (currentItem.ReloadReaction is IRefillable)
                CmdTryReload();
            else
                currentItem.ReloadReaction.Act();
        }

        [Command]
        private void CmdTryReload()
        {
            var currentItem = _physicalItemInteractionController.CurrentHeldItem;
            if (!currentItem || !currentItem.ReloadReaction) return;

            if (currentItem.ReloadReaction is not IRefillable refillable) return;
            var refiller = FindRefillerFor(refillable.RefillType);
            if (refiller == null) return;

            InterruptCurrentAction();
            _currentAction = StartCoroutine(ReloadCoroutine(refiller, refillable));
        }

        private IEnumerator ReloadCoroutine(IRefiller refiller, IRefillable refillable)
        {
            yield return refiller.Refill(refillable, inventory);
            _currentAction = null;
        }

        private IRefiller FindRefillerFor(RefillType type)
        {
            foreach (var kvp in inventory.ServerInventory)
            {
                var slot = kvp.Value;
                var item = _physicalItemRegistry.GetItems()
                    .FirstOrDefault(i => i.Network && i.Network.instanceId == slot.instanceId);
                if (!item || !item.ReloadReaction) continue;

                if (item.ReloadReaction is IRefiller refiller && refiller.RefillType == type)
                    return refiller;
            }
            return null;
        }

        [Command]
        private void CmdSwing()
        {
            InterruptCurrentAction();
            if (!mainCharacter.MeleeAttackController) return;
            if (Time.time - _lastSwingTime < swingCooldown) return;
            _lastSwingTime = Time.time;
            mainCharacter.MeleeAttackController.TriggerSwing();
        }

        private void SelectSlot(int index)
        {
            InterruptCurrentAction();
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
                inventory.CmdDrawItem(index, _physicalItemInteractionController.AnimatorTransform.position);
            }
        }

        [Command]
        private void CmdInteractWithItem(GameObject interactableObject, PhysicalItem item)
        {
            if (InteractableRegistry.Instance.TryGetInteractable(interactableObject, out var interactable))
            {
                interactable.InteractWithItem(item);
            }
        }

        private void InterruptCurrentAction()
        {
            if (_currentAction != null)
            {
                StopCoroutine(_currentAction);
                _currentAction = null;
            }
        }
    }
}