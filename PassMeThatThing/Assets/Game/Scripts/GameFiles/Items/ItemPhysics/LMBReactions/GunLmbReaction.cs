using Ami.BroAudio;
using Game.Scripts.GameFiles.Entity;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class GunLmbReaction : LMBReaction //в будущем будет корневым классом для
    {                                         //всех пушек, поэтому название не с LMB
        [Inject] private PhysicsApplyer physicsApplyer;
        [Inject] private ParticlePoolManager _poolManager;

        [SerializeField] private Transform barrel;
        [SerializeField] private float maxDistance;
        [SerializeField] private LayerMask layersToShot;
        [SerializeField] private float damage;
        [SerializeField] private float force;
        [SerializeField] private int toughnessDamage;
        [SerializeField] private TracerController tracerController;
        [SerializeField] private SoundSource shotSound;
        [SerializeField] private SoundSource emptySound;

        private int ammo = 7;
        private float lastShotTime;
        private float delay = 1f;
        public override void Act()
        { 
            if (Time.time - lastShotTime <= delay || ammo <= 0)
            {
                if (ammo <= 0)
                    RpcPlayEmptySound();
                return;
            }
            Debug.Log(tracerController + " " + barrel);
            physicsApplyer.ShotRaycast(barrel.position, barrel.forward, maxDistance, layersToShot,
                force:force, damage:damage, toughDamage: toughnessDamage );
            var hitPoint = tracerController.Shoot(barrel.position, barrel.forward);
            lastShotTime = Time.time;
            ammo -= 1;
            RpcPlayParticle(hitPoint);
            RpcPlayShotSound();
        }

        [ClientRpc]
        private void RpcPlayParticle(Vector3 hitPoint)
        {
            _poolManager.GetAndPlayParticle(Particles.pow, hitPoint);
        }

        
        [ClientRpc]
        private void RpcPlayShotSound()
        {
            if (shotSound ) 
            {
                shotSound.Play();
            }
        }
        
        [ClientRpc]
        private void RpcPlayEmptySound()
        {
            if (emptySound ) 
            {
                emptySound.Play();
            }
        }
    }
}