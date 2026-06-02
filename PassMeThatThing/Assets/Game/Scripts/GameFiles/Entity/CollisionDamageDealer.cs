using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Entity;
using Enums;
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
        [SerializedDictionary] public SerializedDictionary<DamagableType, float> damageTypes;
        private float _lastDamageTime = -999f;

        private void OnCollisionEnter(Collision other)
        {
            

            if (!isServer) return;               
            if (DamagableRegistry.Instance == null) return;

            if (!DamagableRegistry.Instance.TryGetDamagable(other.gameObject, out var damageable))
                return;

            if (Time.time - _lastDamageTime < cooldown)
                return;

            var finalDamage = damage;
            if (useVelocityDamage)
            {
                var velocity = other.relativeVelocity.magnitude;
                finalDamage += (int)(velocity * velocityDamageMultiplier);
            }

            damageable.ServerTakeDamage(finalDamage);
            _lastDamageTime = Time.time;
        }
    }
}