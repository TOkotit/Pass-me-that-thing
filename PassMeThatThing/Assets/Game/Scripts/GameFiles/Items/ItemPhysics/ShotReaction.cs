// ShotReaction.cs
using System;
using System.Collections;
using Ami.BroAudio;
using Game.Scripts.GameFiles.Entity;
using Mirror;
using UnityEngine;
using VContainer;

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

        [SerializeField] protected EffectController EffectController;
        protected float LastShotTime;

        [Header("Automatic Fire")]
        [SerializeField] private bool _automatic = false;

        public override bool IsContinuous => _automatic;

        protected virtual bool CanShoot()
        {
            if (Time.time - LastShotTime <= delay || CurrentAmmo <= 0 || IsReloading)
            {
                if (CurrentAmmo <= 0 && !IsReloading)
                    CmdPlayEmptySound();
                return false;
            }
            return true;
        }

        /// <summary> Выполняет выстрел в указанном направлении. </summary>
        protected virtual void Shoot(Vector3 direction)
        {
            PhysicsApplyer.ShotRaycast(barrel.position, direction, maxDistance, layersToShot,
                force: force, damage: damage, toughDamage: toughnessDamage);
            var hitPoint = EffectController.ActivateEffect(barrel.position, direction);
            LastShotTime = Time.time;
            CurrentAmmo -= 1;
            CmdPlayParticle(hitPoint);
            CmdPlayShotSound();
        }

        protected virtual Vector3 GetAimDirection()
        {
            return barrel.forward;
        }

        public override void Act()
        {
            if (!CanShoot()) return;
            Shoot(GetAimDirection());
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

        [ClientRpc] protected void RpcReloadStart() { }
        [ClientRpc] protected void RpcReloadFinish() { }

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