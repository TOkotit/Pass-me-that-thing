using System;
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
            Debug.Log($"[MeleeAttack] Start: interactionController={_interactionController != null}, mainCharacter={mainCharacter != null}");
        }

        public void ResetId()
        {
            _attackID = 0;
            Debug.Log("[MeleeAttack] ResetId called, attackID=0");
        }

        [Server]
        public void TriggerSwing()
        {
            Debug.Log($"[MeleeAttack] TriggerSwing called. isServer={isServer}, isClient={isClient}");
            if (!isServer) return;

            if (_interactionController == null)
            {
                Debug.LogError("[MeleeAttack] _interactionController is null!");
                return;
            }

            var heldItem = _interactionController.CurrentHeldItem;
            Debug.Log($"[MeleeAttack] CurrentHeldItem={heldItem?.name ?? "null"}");
            if (heldItem == null)
            {
                Debug.LogError("[MeleeAttack] No held item!");
                return;
            }

            _currentMelee = heldItem.Melee;
            Debug.Log($"[MeleeAttack] Melee component={_currentMelee?.name ?? "null"}");
            if (_currentMelee == null)
            {
                Debug.LogError("[MeleeAttack] Melee is null on held item!");
                return;
            }

            if (_currentMelee.AttackClips == null || _currentMelee.AttackClips.Count == 0)
            {
                Debug.LogError("[MeleeAttack] AttackClips list is empty or null!");
                return;
            }
            Debug.Log($"[MeleeAttack] AttackClips count={_currentMelee.AttackClips.Count}");

            // Отменяем предыдущий сброс комбо
            if (_comboResetRoutine != null)
            {
                StopCoroutine(_comboResetRoutine);
                _comboResetRoutine = null;
                Debug.Log("[MeleeAttack] Previous combo reset coroutine stopped");
            }

            // Проверка окна комбо
            bool comboExpired = Time.time - _lastAttackTime > _swingDuration + comboResetTime;
            Debug.Log($"[MeleeAttack] Combo expired? {comboExpired}. TimeSinceLast={Time.time - _lastAttackTime:F3}, swingDuration={_swingDuration:F3}");
            if (comboExpired)
            {
                ResetId();
            }

            if (_attackID >= _currentMelee.AttackClips.Count)
            {
                _attackID = 0;
                Debug.Log("[MeleeAttack] attackID was out of range, reset to 0");
            }

            var clip = _currentMelee.AttackClips[_attackID];
            if (clip == null)
            {
                Debug.LogError($"[MeleeAttack] AttackClips[{_attackID}] is null!");
                return;
            }

            _swingDuration = clip.length;
            Debug.Log($"[MeleeAttack] Selected attackID={_attackID}, clip={clip.name}, length={_swingDuration:F3}");

            // Включаем триггер коллайдера
            heldItem.Collider.isTrigger = true;
            Debug.Log("[MeleeAttack] Collider.isTrigger = true");

            // Запоминаем время атаки и переключаем ID
            _lastAttackTime = Time.time;
            _attackID = (_attackID + 1) % _currentMelee.AttackClips.Count;
            Debug.Log($"[MeleeAttack] Next attackID will be {_attackID}");

            // Запускаем корутину сброса комбо
            var resetDelay = _swingDuration + comboResetTime;
            _comboResetRoutine = StartCoroutine(ResetComboAfterDelay(resetDelay));
            Debug.Log($"[MeleeAttack] Combo reset coroutine started with delay {resetDelay:F3}");

            // Запускаем корутину выключения триггера
            if (_stopHoldingRoutine != null)
            {
                StopCoroutine(_stopHoldingRoutine);
                Debug.Log("[MeleeAttack] Previous StopHolding coroutine stopped");
            }
            _stopHoldingRoutine = StartCoroutine(StopHolding(heldItem));
            Debug.Log("[MeleeAttack] StopHolding coroutine started");

            // Отправляем RPC на клиенты
            RpcPlayAttackAnimation(clip.name);
            Debug.Log($"[MeleeAttack] RPC PlayAttackAnimation sent for clip '{clip.name}'");
        }

        [ClientRpc]
        private void RpcPlayAttackAnimation(string clipName)
        {
            Debug.Log($"[MeleeAttack] RpcPlayAttackAnimation received on client. clipName={clipName}, pivotAnimator={pivotAnimator != null}");
            if (pivotAnimator == null)
            {
                Debug.LogError("[MeleeAttack] pivotAnimator is null on client!");
                return;
            }

            // Попробуем проиграть по имени состояния
            // Если имена состояний совпадают с именами клипов, Play сработает
            pivotAnimator.Play(clipName, 0, 0f);
            Debug.Log($"[MeleeAttack] pivotAnimator.Play('{clipName}') called");
        }

        private IEnumerator StopHolding(PhysicalItem item)
        {
            yield return new WaitForSeconds(_swingDuration);
            if (item != null)
            {
                item.Collider.isTrigger = false;
                Debug.Log("[MeleeAttack] Collider.isTrigger = false after swing duration");
            }
            _stopHoldingRoutine = null;
        }

        private IEnumerator ResetComboAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ResetId();
            _comboResetRoutine = null;
            Debug.Log("[MeleeAttack] Combo reset after delay");
        }
    }
}