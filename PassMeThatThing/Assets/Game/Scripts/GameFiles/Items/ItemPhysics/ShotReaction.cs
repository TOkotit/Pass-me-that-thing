using System;
using Ami.BroAudio;
using Game.Scripts.GameFiles.Entity;
using Mirror;
using UnityEngine;
using VContainer;
using System.Collections;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public abstract class ShotReaction : LmbReaction
    {
        [Inject] protected PhysicsApplyer PhysicsApplyer;
        [Inject] protected ParticlePoolManager PoolManager;

        [SerializeField] protected Transform barrel;
        [SerializeField] protected float maxDistance;
        [SerializeField] protected LayerMask layersToShot;
        [SerializeField] protected float damage;
        [SerializeField] protected float force;
        [SerializeField] protected float reloadTime;
        [SerializeField] protected int toughnessDamage;
        [SerializeField] protected SoundSource shotSound;
        [SerializeField] protected SoundSource emptySound;
        [SerializeField] protected float delay = 1f;

        [Header("Ammo")]
        [SerializeField] protected int maxAmmo = 30;
        protected int CurrentAmmo;
        protected bool IsReloading;

        [SerializeField] protected IEffectController EffectController;
        protected float LastShotTime;

        public override void Act()
        {
            if (Time.time - LastShotTime <= delay || CurrentAmmo <= 0)
            {
                if (CurrentAmmo <= 0 && !IsReloading)
                    CmdPlayEmptySound();
                return;
            }

            PhysicsApplyer.ShotRaycast(barrel.position, barrel.forward, maxDistance, layersToShot,
                force: force, damage: damage, toughDamage: toughnessDamage);
            var hitPoint = EffectController.ActivateEffect(barrel.position, barrel.forward);
            LastShotTime = Time.time;
            CurrentAmmo -= 1;
            CmdPlayParticle(hitPoint);
            CmdPlayShotSound();
        }

        [Command(requiresAuthority = false)]
        public virtual void CmdReload()
        {
            if (IsReloading || CurrentAmmo == maxAmmo) return;
            StartCoroutine(ReloadRoutine());
        }

        protected virtual IEnumerator ReloadRoutine()
        {
            IsReloading = true;
            RpcReloadStart();
            yield return new WaitForSeconds(reloadTime);
            CurrentAmmo = maxAmmo;
            IsReloading = false;
            RpcReloadFinish();
        }

        [ClientRpc]
        protected void RpcReloadStart() { }
        [ClientRpc]
        protected void RpcReloadFinish() { }

        [Command(requiresAuthority = false)]
        protected void CmdPlayParticle(Vector3 hitPoint) => RpcPlayParticle(hitPoint);

        [ClientRpc]
        private void RpcPlayParticle(Vector3 hitPoint) => PoolManager.GetAndPlayParticle(Particles.pow, hitPoint);

        [Command(requiresAuthority = false)]
        protected void CmdPlayShotSound() => RpcPlayShotSound();

        [Command(requiresAuthority = false)]
        protected void CmdPlayEmptySound() => RpcPlayEmptySound();

        [ClientRpc]
        private void RpcPlayShotSound() { if (shotSound) shotSound.Play(); }

        [ClientRpc]
        private void RpcPlayEmptySound() { if (emptySound) emptySound.Play(); }

        protected virtual void Awake()
        {
            CurrentAmmo = maxAmmo;
        }
    }
}