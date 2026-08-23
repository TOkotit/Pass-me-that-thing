using System;
using AYellowpaper.SerializedCollections;
using Entity;
using Enums;
using Game.Scripts.GameFiles.Entity;
using Game.Scripts.GameFiles.Entity.Enemy;
using UnityEngine;

namespace Game.Scripts.Systems
{
    public class DamageSystem
    {
        public bool TakeDamage(float damage, 
            Damageable damageable, 
            SerializedDictionary<DamagableType, float> damageTypes = null,
            int toughnessDamage = 0,
            Action callback = null)
        {
            if (!damageable) return false;

            float multiplier = 1f;
            if (damageTypes != null)
            {
                if (!damageTypes.ContainsKey(damageable.Type)) return false;
                multiplier = damageTypes[damageable.Type];
            }
    
            int finalDamage = (int)(damage * multiplier);
            damageable.ServerTakeDamage(finalDamage);

            if (toughnessDamage > 0 && damageable is ToughnessDamageable tough)
            {
                tough.ServerReduceToughness(toughnessDamage);
            }

            callback?.Invoke();
            return true;
        }
    }
}