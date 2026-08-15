// LmbMachineGun.cs
using System;
using System.Collections;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LmbMachineGun : ShotReaction
    {
        [Header("Spread")]
        [SerializeField] private float baseSpread = 0.5f;
        [SerializeField] private float spreadPerStability = 0.2f;
        [SerializeField] private float spreadExponent = 1.5f;

        [Header("Recoil")]
        [SerializeField] private Vector3 baseRecoilForce = new Vector3(0, 0, -5f);
        [SerializeField] private Vector3 baseRecoilTorque = new Vector3(-2f, 0, 0);
        [SerializeField] private float recoilPerStability = 0.3f;
        [SerializeField] private float recoilExponent = 1.5f;

        [Header("Stability")]
        [SerializeField] private float stabilityIncreasePerShot = 1f;
        [SerializeField] private float stabilityDecreaseRate = 2f;
        [SerializeField] private int overloadBurstCount = 3;
        [SerializeField] private float overloadBurstDelay = 0.1f;
        [SerializeField] private float stabilityRecoveryDelay = 0.5f;
        
        [Header("Aiming")]
        [SerializeField] private float maxAimAngle = 10f;
        private float _currentStability;
        private bool _isOverloading;

        private void Update()
        {
            if (!isServer) return;
            if (IsReloading || _isOverloading) return;

            if (_currentStability > 0 && Time.time - LastShotTime > stabilityRecoveryDelay)
            {
                _currentStability -= stabilityDecreaseRate * Time.deltaTime;
                if (_currentStability < 0) _currentStability = 0;
            }
        }

        public override void Act()
        {
            if (!CanShoot()) return;

            _currentStability += stabilityIncreasePerShot;

            var baseDir = GetAimDirection();          
            var shootDir = GetSpreadDirection(baseDir); 
            Shoot(shootDir);

            var recoilScale = Mathf.Pow(_currentStability * recoilPerStability, recoilExponent);
            var finalRecoilForce = baseRecoilForce * (1 + recoilScale);
            var finalRecoilTorque = baseRecoilTorque * (1 + recoilScale);

            if (Item && Item.Rigidbody)
            {
                Item.Rigidbody.AddRelativeForce(finalRecoilForce, ForceMode.Impulse);
                Item.Rigidbody.AddRelativeTorque(finalRecoilTorque, ForceMode.Impulse);
            }

            if (!_isOverloading && Item && Item.Owner && _currentStability >= Item.Owner.Strength)
            {
                StartCoroutine(StabilityOverload());
            }
        }

        private IEnumerator StabilityOverload()
        {
            _isOverloading = true;
            if (Item.Owner)
                Item.Owner.Fall(5f, -barrel.forward);

            for (int i = 0; i < overloadBurstCount; i++)
            {
                if (CurrentAmmo <= 0) break;
                yield return new WaitForSeconds(overloadBurstDelay);
                Shoot(GetSpreadDirection(GetAimDirection()));
            }
            _currentStability = 0;
            _isOverloading = false;
        }

        private Vector3 GetSpreadDirection(Vector3 baseDir)
        {
            var spread = baseSpread + Mathf.Pow(_currentStability * spreadPerStability, spreadExponent);
            return Quaternion.Euler(Random.Range(-spread, spread), Random.Range(-spread, spread), 0) * baseDir;
        }

        protected override Vector3 GetAimDirection()
        {
            if (maxAimAngle <= 0f || !Item || !Item.Owner)
                return barrel.forward;

            var cam = Item.Owner.MCamera?.Camera;
            if (!cam)
                return barrel.forward;

            var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 targetPoint;
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layersToShot))
                targetPoint = hit.point;
            else
                targetPoint = ray.GetPoint(maxDistance);

            var desiredDirection = (targetPoint - barrel.position).normalized;
            var angle = Vector3.Angle(barrel.forward, desiredDirection);
            if (angle <= maxAimAngle)
                return desiredDirection;

            var axis = Vector3.Cross(barrel.forward, desiredDirection).normalized;
            return Quaternion.AngleAxis(maxAimAngle, axis) * barrel.forward;
        }
    }
}