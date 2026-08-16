using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Entity;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.MainCharacterPhysics
{
    public class MeleeAttackController : NetworkBehaviour
    {
        [SerializeField] private MainCharacter mainCharacter;
        [SerializeField] private Animator pivotAnimator;

        [Header("Combo Settings")]
        [SerializeField] private float comboResetTime = 1.2f;

        private float _swingDuration;
        private PhysicalItemInteractionController _interactionController;
        private int _attackID;
        private MeleeItem _currentMelee;
        private float _lastAttackTime;
        private Coroutine _comboResetRoutine;
        private Coroutine _stopHoldingRoutine;

        public Animator PivotAnimator => pivotAnimator;

        public void ResetId()
        {
            _attackID = 0;
        }

        public void SwitchID()
        {
            _attackID++;
            if (!_currentMelee) return;
            if (_attackID >= _currentMelee.Attacks.Count)
            {
                ResetId();
            }
        }

        private void Start()
        {
            _interactionController = mainCharacter.PhysicalItemInteractionController;
        }

        public void TriggerSwing()
        {
            if (!isServer) return; 

            _currentMelee = _interactionController.CurrentHeldItem?.Melee;
            if (!_currentMelee || _currentMelee.Attacks.Count <= 0) return;

            if (_comboResetRoutine != null)
            {
                StopCoroutine(_comboResetRoutine);
                _comboResetRoutine = null;
            }

            if (Time.time - _lastAttackTime > _swingDuration + comboResetTime)
            {
                ResetId();
            }

            if (_attackID >= _currentMelee.Attacks.Count)
            {
                _attackID = 0;
            }

            var currentAttackType = _currentMelee.Attacks[_attackID];
            var currentAttackName = mainCharacter.AttackAnimationId.attackIds[currentAttackType];

            var swingClip = pivotAnimator.runtimeAnimatorController.animationClips
                .FirstOrDefault(clip => clip.name == currentAttackName);
            _swingDuration = swingClip ? swingClip.length : 1f;

            var heldItem = _interactionController.CurrentHeldItem;
            if (heldItem)
            {
                heldItem.Collider.isTrigger = true;
            }

           _lastAttackTime = Time.time;
            SwitchID();

            var resetDelay = _swingDuration + comboResetTime;
            _comboResetRoutine = StartCoroutine(ResetComboAfterDelay(resetDelay));

            if (_stopHoldingRoutine != null)
            {
                StopCoroutine(_stopHoldingRoutine);
            }
            _stopHoldingRoutine = StartCoroutine(StopHolding(heldItem));
            RpcPlayAttackAnimation(currentAttackName);
        }

        [ClientRpc]
        private void RpcPlayAttackAnimation(string attackName)
        {
            if (pivotAnimator)
            {
                pivotAnimator.SetTrigger(Animator.StringToHash(attackName));
            }
        }

        private IEnumerator StopHolding(PhysicalItem item)
        {
            yield return new WaitForSeconds(_swingDuration);
            if (item)
            {
                item.Collider.isTrigger = false;
            }
        }

        private IEnumerator ResetComboAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ResetId();
            _comboResetRoutine = null;
        }
    }
}