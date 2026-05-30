using System.Collections;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics
{
    public class SlipManager : NetworkBehaviour
    {
        
        [SerializeField] private BodyVerticalAlign verticalAlign;
        [SerializeField] private GroundCheck leftLeg;
        [SerializeField] private GroundCheck rightLeg;
        [SerializeField] private MainCharacterMovement movement;

        [Header("Настройки")]
        [SerializeField] private float slipDuration = 2f;
        [SerializeField] private float slipCooldown = 5f;

        private float _nextSlipTime;
        private bool _isSlipping;
        
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            
            if (leftLeg) leftLeg.OnWaterTouched += TrySlip;
            if (rightLeg) rightLeg.OnWaterTouched += TrySlip;
        }

        [Server]
        private void TrySlip()
        {
            if (_isSlipping || Time.time < _nextSlipTime) return;

            StartCoroutine(SlipRoutine());
        }
        
        private IEnumerator SlipRoutine()
        {
            _isSlipping = true;
            _nextSlipTime = Time.time + slipCooldown;

            SetMovementLocked(true);
            
            if (verticalAlign)
            {
                verticalAlign.SetConsciousness(0f);
            }

            yield return new WaitForSeconds(slipDuration);
            
            if (verticalAlign) 
            {
                verticalAlign.SetConsciousness(1f);
            }
            
            SetMovementLocked(false);
            
            _isSlipping = false;
        }
        
        
        [Server]
        private void SetMovementLocked(bool locked)
        {
            if (connectionToClient != null)
                TargetSetMovementLocked(connectionToClient, locked);
        }

        [TargetRpc]
        private void TargetSetMovementLocked(NetworkConnection target, bool locked)
        {
            if (!movement) return;

            if (locked)
                movement.LockUpMovement();
            else
                movement.UnlockMovement();
        }
        
        private void OnDestroy()
        {
            if (isServer)
            {
                if (leftLeg) leftLeg.OnWaterTouched -= TrySlip;
                if (rightLeg) rightLeg.OnWaterTouched -= TrySlip;
            }
        }
    }
}