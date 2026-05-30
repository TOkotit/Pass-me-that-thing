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
        
        
        public override void OnStartLocalPlayer()        {
            base.OnStartLocalPlayer();
            
            if (leftLeg) leftLeg.OnWaterTouched += TrySlip;
            if (rightLeg) rightLeg.OnWaterTouched += TrySlip;
        }

        private void TrySlip()
        {
            if (_isSlipping || Time.time < _nextSlipTime) return;

            StartCoroutine(SlipRoutine());
        }
        
        private IEnumerator SlipRoutine()
        {
            _isSlipping = true;
            _nextSlipTime = Time.time + slipCooldown;

            if (movement) movement.LockUpMovement();
            
            if (verticalAlign)
            {
                verticalAlign.Consciousness = 0f;
                CmdSetConsciousness(0f);
            }

            yield return new WaitForSeconds(slipDuration);
            
            if (verticalAlign) 
            {
                verticalAlign.Consciousness = 1f;
                CmdSetConsciousness(1f);
            }
            
            if (movement) movement.UnlockMovement();
            
            _isSlipping = false;
        }
        
        [Command]
        private void CmdSetConsciousness(float value)
        {
            if (verticalAlign) 
                verticalAlign.Consciousness = value;
        }
        
        private void OnDestroy()
        {
            if (isLocalPlayer)
            {
                if (leftLeg) leftLeg.OnWaterTouched -= TrySlip;
                if (rightLeg) rightLeg.OnWaterTouched -= TrySlip;
            }
        }
    }
}