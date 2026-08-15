using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Game.Entity;
using Game.Scripts.Enums;                    
using Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.MainCharacterPhysics
{
    public class MeleeAttackController : NetworkBehaviour
    {
        [SerializeField] private MainCharacter mainCharacter;
        [SerializeField] private Animator pivotAnimator;
        private float _swingDuration;
        private PhysicalItemInteractionController _interactionController;
        private int _attackID;
        private MeleeItem _currentMelee;
        public Animator PivotAnimator => pivotAnimator;

        public void ResetId()
        {
            _attackID = 0;
        }

        public void SwitchID()
        {
            _attackID++;
            if (!_currentMelee) return;
            if (_attackID >= _currentMelee.Attacks.Count) { ResetId(); }
        }

        private void Start()
        {
            _interactionController = mainCharacter.PhysicalItemInteractionController;
        }

        public void TriggerSwing()
        {
            _currentMelee = _interactionController.CurrentHeldItem.Melee;
            if (!_currentMelee || _currentMelee.Attacks.Count <= 0) return;
            if (_attackID >= _currentMelee.Attacks.Count) _attackID = 0; 
            var currentAttackID = _currentMelee.Attacks[_attackID];
            var currentAttack = mainCharacter.AttackAnimationId.attackIds[currentAttackID];
            var swingClip = pivotAnimator.runtimeAnimatorController.animationClips
                .FirstOrDefault(clip => clip.name == currentAttack);
            if (swingClip)
                _swingDuration = swingClip.length;
            else
                _swingDuration = 0.5f;
            _interactionController.CurrentHeldItem.Collider.isTrigger = true;
            if (pivotAnimator)
                pivotAnimator.SetTrigger(Animator.StringToHash( currentAttack));
            StartCoroutine(StopHolding());
        }

        private IEnumerator StopHolding()
        {
            yield return new WaitForSeconds(_swingDuration);
            _interactionController.CurrentHeldItem.Collider.isTrigger = false;
        }
    }
}