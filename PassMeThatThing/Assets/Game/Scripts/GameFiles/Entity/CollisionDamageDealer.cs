using System;
using Entity;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity
{
    public class CollisionDamageDealer : NetworkBehaviour
    {
        [SerializeField] private int damage = 10;
        [SerializeField] private bool useVelocityDamage = false;
        [SerializeField] private float velocityDamageMultiplier = 1f;
        [SerializeField] private float cooldown = 0.5f;

        private float _lastDamageTime = -999f;

        private void OnCollisionEnter(Collision other)
        {
            if (!isServer) return;               
            if (DamagableRegistry.Instance == null) return;

            if (!DamagableRegistry.Instance.TryGetDamagable(other.gameObject, out var damagable))
                return;

            if (Time.time - _lastDamageTime < cooldown)
                return;

            var finalDamage = damage;
            if (useVelocityDamage)
            {
                var velocity = other.relativeVelocity.magnitude;
                finalDamage += (int)(velocity * velocityDamageMultiplier);
            }

            damagable.DamagableModel.HealthPool.TakeDamage(finalDamage);
            _lastDamageTime = Time.time;
        }
    }
}