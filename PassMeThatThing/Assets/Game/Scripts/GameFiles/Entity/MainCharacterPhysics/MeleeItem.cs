using System;
using System.Collections.Generic;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.MainCharacterPhysics
{
    public class MeleeItem : NetworkBehaviour
    {
        [Inject] PhysicsApplyer physicsApplyer;
        [SerializeField] private List<AttackAnimationType> attacks;
        [SerializeField] private PhysicalItem item;
        [SerializeField] private float damage;
        [SerializeField] private int toughnessDamage;
        public List<AttackAnimationType> Attacks => attacks;

        private void OnTriggerEnter(Collider other)
        {
            physicsApplyer.ApplyForceAndDamageToTarget(other.gameObject,
                item.Rigidbody.linearVelocity,damage,toughnessDamage,
                transform.position);
        }
    }
}