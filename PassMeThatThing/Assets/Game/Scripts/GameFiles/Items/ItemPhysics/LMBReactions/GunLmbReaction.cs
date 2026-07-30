using Game.Scripts.GameFiles.Entity;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class GunLmbReaction : LMBReaction //в будущем будет корневым классом для
    {                                         //всех пушек, поэтому название не с LMB
        [Inject] private PhysicsApplyer physicsApplyer;
        [SerializeField] private Transform barrel;
        [SerializeField] private float maxDistance;
        [SerializeField] private LayerMask layersToShot;
        [SerializeField] private float damage;
        [SerializeField] private float force;
        [SerializeField] private int toughnessDamage;
        [SerializeField] private TracerController tracerController;
        private int ammo = 7;
        private float lastShotTime;
        private float delay = 1f;
        public override void Act()
        { 
            if (Time.time - lastShotTime <= delay || ammo <= 0) return;
            Debug.Log(tracerController + " " + barrel);
            physicsApplyer.ShotRaycast(barrel.position, barrel.forward, maxDistance, layersToShot,
                force:force, damage:damage, toughDamage: toughnessDamage );
            tracerController.Shoot(barrel.position, barrel.forward);
            lastShotTime = Time.time;
            ammo -= 1;
        }
    }
}