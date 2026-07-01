using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Entity;
using Enums;
using FishNet.Object;
using Game.Scripts.Systems;

using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity
{
    public class CollisionDamageDealer : NetworkBehaviour
    {
        [SerializeField] private int damage = 10;
        [SerializeField] private int toughnessDamage = 1;
        [SerializeField] private bool useVelocityDamage = false;
        [SerializeField] private float velocityDamageMultiplier = 1f;
        [SerializeField] private float cooldown = 0.5f;
        [SerializedDictionary] public SerializedDictionary<DamagableType, float> damageTypes;
        
        [Inject] private DamageSystem _damageSystem;
        
        private float _lastDamageTime = -999f;
        
        public event Action OnServerTakeDamage;

        private void OnCollisionEnter(Collision other)
        {

            if (!IsServerStarted) return;
            if (DamagableRegistry.Instance == null) return;
            if (Time.time - _lastDamageTime < cooldown) return;
            
            var finalDamage = damage;
            if (useVelocityDamage)
            {
                var velocity = other.relativeVelocity.magnitude;
                finalDamage += (int)(velocity * velocityDamageMultiplier);
            }
            
            _damageSystem.TakeDamage(finalDamage, other.gameObject, damageTypes, toughnessDamage, OnServerTakeDamage);
            
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