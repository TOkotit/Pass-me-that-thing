using System;
using System.Collections;
using System.Linq;
using DI;
using Entity;
using Game.Entity;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using Systems;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics
{
    public class PhysicalItemInteractionController : NetworkBehaviour
    {
        public PhysicalItem CurrentHeldItem => _heldItem;

        [SerializeField] private PhysicalItem _heldItem;
        [SerializeField] private MainCharacter mainCharacter;
        [SerializeField] private float strength;
        [SerializeField] private MainCharacterMovement movement;
        [SerializeField] private Animator pivotAnimator;
        private HandsMovement _handsMovement;
        private float _swingDuration;
        public Transform AnimatorTransform => _handsMovement.AnimatorTransform;
        public HandsMovement HandsMovement => _handsMovement;
        public Animator PivotAnimator => pivotAnimator;
        

        public override void OnStartLocalPlayer()
        {
            InjectSelf();
        }

        private void Start()
        {
            _handsMovement = GetComponentInChildren<HandsMovement>();
            var swingClip = pivotAnimator.runtimeAnimatorController.animationClips
                .FirstOrDefault(clip => clip.name == "Swing");
            if (swingClip)
                _swingDuration = swingClip.length;
        }

        private void InjectSelf()
        {
            var scope = FindObjectOfType<GameplayScope>();
            if (scope)
                scope.Container.Inject(this);
            else
                Debug.LogError("GameplayScope not found!");
        }

        private void SetOwnerAndLayer(PhysicalItem item)
        {
            if (item.CanBeOwned)
            {
                item.Owner = mainCharacter;
                if (isLocalPlayer)
                {
                    item.gameObject.layer = LayerMask.NameToLayer("HeldItem");
                }
            }
        }
        
        private void RestoreLayerAndClear(PhysicalItem item)
        {
            if (item)
            {
                item.gameObject.layer = LayerMask.NameToLayer("Interactable");
                item.Owner = null;
            }
        }

        public void ChargeDrop()
        {
            if (_heldItem)
                _handsMovement.ChargeThrow();
        }

        [Server]
        public void PhysicalPickUpItem(PhysicalItem item)
        {
            _heldItem = item;
            movement.SetMovementMultiplier(item.Rigidbody.mass);
            SetOwnerAndLayer(item);
            TargetPickUpItem(item);
            _handsMovement.GrabItem(item);
        }

        [TargetRpc]
        private void TargetPickUpItem(PhysicalItem item)
        {
            Debug.Log($"[PC] TargetPickUpItem {item}");
            _heldItem = item;
            movement.SetMovementMultiplier(item.Rigidbody.mass);
            SetOwnerAndLayer(item);
            if (_heldItem)
                _handsMovement.GrabItem(_heldItem);
        }
        
        [Server]
        public void ReleaseCurrentItem(float throwForce, bool canThrow)
        {
            if (_heldItem)
            {
                RestoreLayerAndClear(_heldItem);
                _handsMovement.ReleaseItem(_heldItem, throwForce, canThrow);
                _heldItem = null;
                
                movement.ResetMovementMultiplier();
                TargetClearHeldItem();
            }
        }

        [Server]
        public void ServerClearHeldItem()
        {
            if (_heldItem)
            {
                RestoreLayerAndClear(_heldItem);
                _heldItem = null;
                movement.ResetMovementMultiplier();
            }
            TargetClearHeldItem();
        }

        [TargetRpc]
        public void TargetClearHeldItem()
        {
            if (_heldItem)
            {
                RestoreLayerAndClear(_heldItem);
                _handsMovement.ReleaseItem(_heldItem, 0f, false);
                _heldItem = null;
                movement.ResetMovementMultiplier();
            }
        }

        [TargetRpc]
        public void TargetSyncPositionForDrop(NetworkConnection target, Vector3 position, Quaternion rotation)
        {
            if (_heldItem)
            {
                _heldItem.Rigidbody.MovePosition(position);
                _heldItem.Rigidbody.MoveRotation(rotation);
            }
        }
        
        public void TriggerSwing()
        {
            //pivotAnimator.enabled = true;
            if (pivotAnimator)
                pivotAnimator.SetTrigger("Swing");
            Debug.Log($"TriggerSwing {HandsMovement == null} {CurrentHeldItem == null}");
            HandsMovement.FixGrab(CurrentHeldItem.Rigidbody);
            StartCoroutine(StopHolding());
        }
        
        private IEnumerator StopHolding()
        {
            yield return new WaitForSeconds(1);
            //pivotAnimator.enabled = false;
            HandsMovement.ReleaseGrab();
        }
    }
}