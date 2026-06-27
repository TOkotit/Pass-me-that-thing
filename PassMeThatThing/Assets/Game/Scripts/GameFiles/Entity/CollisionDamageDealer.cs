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
        
        public event Action OnServerTakeDamage;

        private void OnCollisionEnter(Collision other)
        {

            if (!isServer) return;
            if (DamagableRegistry.Instance == null) return;

            var damageable = FindDamagableInHierarchy(other.gameObject);
            if (!damageable) return;

            if (!damageTypes.ContainsKey(damageable.Type)) return;
            if (Time.time - _lastDamageTime < cooldown) return;

            var finalDamage = (int)(damage * damageTypes[damageable.Type]);
            if (useVelocityDamage)
            {
                var velocity = other.relativeVelocity.magnitude;
                finalDamage += (int)(velocity * velocityDamageMultiplier);
            }

            damageable.ServerTakeDamage(finalDamage);
            OnServerTakeDamage?.Invoke();
            _lastDamageTime = Time.time;
        }
        private Damagable FindDamagableInHierarchy(GameObject obj)
        {
            var t = obj.transform;
            while (t)
            {
                if (DamagableRegistry.Instance.TryGetDamagable(t.gameObject, out var damagable))
                    return damagable;
                t = t.parent;
            }
            return null;
        }
    }
}