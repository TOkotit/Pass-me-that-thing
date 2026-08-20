using System.Collections.Generic;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.MainCharacterPhysics
{
    public class MeleeItem : NetworkBehaviour
    {
        [Inject] PhysicsApplyer physicsApplyer;

        [SerializeField] private PhysicalItem item;
        [SerializeField] private List<AnimationClip> attackClips = new();
        [SerializeField] private float damage;
        [SerializeField] private int toughnessDamage;
        [SerializeField] private float impactMultiplier = 1f; 
        
        public IReadOnlyList<AnimationClip> AttackClips => attackClips;

        private void OnTriggerEnter(Collider other)
        {
            if (!isServer) return;
            if (!item || !item.Rigidbody || physicsApplyer == null) return;
            var impulse = item.Rigidbody.mass * item.Rigidbody.linearVelocity * impactMultiplier;

            physicsApplyer.ApplyForceAndDamageToTarget(
                other.gameObject,
                impulse,
                damage,
                toughnessDamage,
                transform.position,
                forceMode: ForceMode.Impulse);
        }
    }
}