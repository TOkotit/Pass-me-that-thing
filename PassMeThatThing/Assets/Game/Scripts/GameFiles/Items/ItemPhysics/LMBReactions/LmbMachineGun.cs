using System;
using System.Collections;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class LmbMachineGun : ShotReaction
    {
        [SerializeField] private TracerController tracerController;
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

        private float _currentStability;
        private bool _isOverloading;

        private void Update()
        {
            if (!isServer) return;
            if (IsReloading || _isOverloading) return;

            if (_currentStability > 0)
            {
                _currentStability -= stabilityDecreaseRate * Time.deltaTime;
                if (_currentStability < 0) _currentStability = 0;
            }
        }

        [Command(requiresAuthority = false)]
        public override void CmdReload()
        {
            if (IsReloading || CurrentAmmo == maxAmmo) return;
            StopCoroutine(ReloadRoutine());
            StartCoroutine(ReloadRoutine());
        }

        protected override IEnumerator ReloadRoutine()
        {
            IsReloading = true;
            RpcReloadStart();
            var elapsed = 0f;
            var duration = 3f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            CurrentAmmo = maxAmmo;
            IsReloading = false;
            RpcReloadFinish();
        }

        public override void Act()
        {
            Debug.Log("должен быть выстрел!");
            if (!isServer) return;

            if (Time.time - LastShotTime <= delay || CurrentAmmo <= 0 || IsReloading)
            {
                if (CurrentAmmo <= 0 && !IsReloading && !_isOverloading)
                    CmdPlayEmptySound();
                return;
            }

            _currentStability += stabilityIncreasePerShot;

            var spread = baseSpread + Mathf.Pow(_currentStability * spreadPerStability, spreadExponent);
            var recoilScale = Mathf.Pow(_currentStability * recoilPerStability, recoilExponent);
            var finalRecoilForce = baseRecoilForce * (1 + recoilScale);
            var finalRecoilTorque = baseRecoilTorque * (1 + recoilScale);

            var shootDir = Quaternion.Euler(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                0
            ) * barrel.forward;

            PhysicsApplyer.ShotRaycast(barrel.position, shootDir, maxDistance, layersToShot,
                force: force, damage: damage, toughDamage: toughnessDamage);
            var hitPoint = tracerController.ActivateEffect(barrel.position, shootDir);
            LastShotTime = Time.time;
            CurrentAmmo -= 1;
            CmdPlayParticle(hitPoint);
            CmdPlayShotSound();

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
                var spread = baseSpread + Mathf.Pow(_currentStability * spreadPerStability, spreadExponent);
                var shootDir = Quaternion.Euler(
                    Random.Range(-spread, spread),
                    Random.Range(-spread, spread),
                    0
                ) * barrel.forward;

                PhysicsApplyer.ShotRaycast(barrel.position, shootDir, maxDistance, layersToShot,
                    force: force, damage: damage, toughDamage: toughnessDamage);
                var hitPoint = tracerController.ActivateEffect(barrel.position, shootDir);
                CurrentAmmo -= 1;
                CmdPlayParticle(hitPoint);
                CmdPlayShotSound();
            }
            _currentStability = 0;
            _isOverloading = false;
        }
    }
}