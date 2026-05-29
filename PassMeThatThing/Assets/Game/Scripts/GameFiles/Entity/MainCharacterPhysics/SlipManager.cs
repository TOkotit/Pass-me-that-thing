using System.Collections;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.NewMainCharacterPhysics
{
    public class SlipManager : NetworkBehaviour
    {
        
        [SerializeField] private BodyVerticalAlign verticalAlign;
        [SerializeField] private GroundStateManager groundState;
        [SerializeField] private GroundCheck leftLeg;
        [SerializeField] private GroundCheck rightLeg;
        [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private MainCharacterMovement movement;

        [Header("Настройки")]
        [SerializeField] private float slipDuration = 2f;
        [SerializeField] private float slipCooldown = 5f;

        private float _nextSlipTime;
        private bool _isSlipping;
        
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            // Подписываемся на события ног
            leftLeg.OnWaterTouched += TrySlip;
            rightLeg.OnWaterTouched += TrySlip;
        }

        [Server]
        private void TrySlip()
        {
            if (_isSlipping || Time.time < _nextSlipTime) return;

            StartCoroutine(SlipRoutine());
        }
        
        private IEnumerator SlipRoutine()
        {
            _nextSlipTime = Time.time + slipCooldown;

            SetMovementLocked(true);
            
            if (verticalAlign)
            {
                //verticalAlign.EmergencyRelax(); 
                //verticalAlign.enabled = false;  
            }

            if (rigidbody)
            {
                var direction = Random.value > 0.5f ? 1f : -1f;
        
                rigidbody.AddForce(transform.forward * (10f * direction) + Vector3.up * 2f, ForceMode.Impulse);


                var torqueForce = 50f * direction; 
                rigidbody.AddTorque(transform.right * torqueForce, ForceMode.Impulse);
        
                rigidbody.AddTorque(transform.up * Random.Range(-10f, 10f), ForceMode.Impulse);
            }
            
            yield return new WaitForSeconds(slipDuration);

            
            if (verticalAlign) 
            {
                verticalAlign.enabled = true; 
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
            if (leftLeg) leftLeg.OnWaterTouched -= TrySlip;
            if (rightLeg) rightLeg.OnWaterTouched -= TrySlip;
        }
    }
}