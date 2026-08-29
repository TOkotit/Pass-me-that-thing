using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Entity;
using Enums;
using Game.Scripts.Systems;
using Mirror;
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

        [SyncVar]
        public Vector3 HitPosition;

        public event Action OnTakeDamage;

        private void OnCollisionEnter(Collision other)
        {

            if (!isServer) return;
            if (DamagableRegistry.Instance == null) return;
            if (Time.time - _lastDamageTime < cooldown) return;
            
            var finalDamage = damage;
            if (useVelocityDamage)
            {
                var velocity = other.relativeVelocity.magnitude;
                finalDamage += (int)(velocity * velocityDamageMultiplier);
            }
            
            if (DamagableRegistry.Instance.TryGetDamagable(other.gameObject, out var dam))
            {
                _damageSystem.TakeDamage(finalDamage, dam, damageTypes, toughnessDamage, OnTakeDamage);
                if (other.contactCount > 0)
                {
                    HitPosition = other.GetContact(0).point;
                }
            }

            
            Debug.Log($"{other.gameObject.name} collided with {finalDamage} damage");
            
            _lastDamageTime = Time.time;
        }
    }
}