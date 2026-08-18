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

        public IReadOnlyList<AnimationClip> AttackClips => attackClips;

        private void OnTriggerEnter(Collider other)
        {
            if (!isServer) return;
            if (!item || !item.Rigidbody || physicsApplyer == null) return;

            physicsApplyer.ApplyForceAndDamageToTarget(
                other.gameObject,
                item.Rigidbody.linearVelocity,
                damage,
                toughnessDamage,
                transform.position);
        }
    }
}