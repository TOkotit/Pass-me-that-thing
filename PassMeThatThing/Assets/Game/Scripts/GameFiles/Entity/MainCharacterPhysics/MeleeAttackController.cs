using System.Collections;
using Game.Entity;
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

        [Header("Solid Restore")]
        [Tooltip("Страховочный таймаут")]
        [SerializeField] private float maxSolidRestoreDelay = 3f;

        private float _swingDuration;
        private PhysicalItemInteractionController _interactionController;
        private int _attackID;
        private MeleeItem _currentMelee;
        private float _lastAttackTime;
        private Coroutine _comboResetRoutine;
        private Coroutine _stopHoldingRoutine;

        public Animator PivotAnimator => pivotAnimator;

        private void Start()
        {
            _interactionController = mainCharacter.PhysicalItemInteractionController;
        }

        public void ResetId()
        {
            _attackID = 0;
        }

        [Server]
        public void TriggerSwing()
        {
            if (!isServer) return;
            if (!_interactionController) return;

            var heldItem = _interactionController.CurrentHeldItem;
            if (!heldItem) return;

            _currentMelee = heldItem.Melee;
            if (!_currentMelee) return;
            if (_currentMelee.AttackClips == null || _currentMelee.AttackClips.Count == 0) return;

            if (_comboResetRoutine != null)
            {
                StopCoroutine(_comboResetRoutine);
                _comboResetRoutine = null;
            }

            bool comboExpired = Time.time - _lastAttackTime > _swingDuration + comboResetTime;
            if (comboExpired)
                ResetId();

            if (_attackID >= _currentMelee.AttackClips.Count)
                _attackID = 0;

            var clip = _currentMelee.AttackClips[_attackID];
            if (!clip) return;

            _swingDuration = clip.length;

            _currentMelee.ClearContacts();

            if (heldItem.Collider != null)
                heldItem.Collider.isTrigger = true;

            _lastAttackTime = Time.time;
            _attackID = (_attackID + 1) % _currentMelee.AttackClips.Count;

            var resetDelay = _swingDuration + comboResetTime;
            _comboResetRoutine = StartCoroutine(ResetComboAfterDelay(resetDelay));

            if (_stopHoldingRoutine != null)
            {
                StopCoroutine(_stopHoldingRoutine);
                _stopHoldingRoutine = null;
            }
            _stopHoldingRoutine = StartCoroutine(StopHolding(heldItem));

            RpcPlayAttackAnimation(clip.name);
        }

        [ClientRpc]
        private void RpcPlayAttackAnimation(string clipName)
        {
            if (!pivotAnimator) return;
            pivotAnimator.Play(clipName, 0, 0f);
        }

        private IEnumerator StopHolding(PhysicalItem item)
        {
            yield return new WaitForSeconds(_swingDuration);

            if (!item || !item.Collider)
            {
                _stopHoldingRoutine = null;
                yield break;
            }

            var col = item.Collider;
            var melee = item.Melee;
            var playerRoot = mainCharacter ? mainCharacter.transform.root : null;

            var startTime = Time.time;
            while (melee && melee.HasForeignContacts(playerRoot))
            {
                if (Time.time - startTime >= maxSolidRestoreDelay) break;
                if (!item || !col) { _stopHoldingRoutine = null; yield break; }
                yield return new WaitForFixedUpdate();
            }

            if (col) col.isTrigger = false;
            if (melee) melee.ClearContacts();

            _stopHoldingRoutine = null;
        }

        private IEnumerator ResetComboAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ResetId();
            _comboResetRoutine = null;
        }
    }
}