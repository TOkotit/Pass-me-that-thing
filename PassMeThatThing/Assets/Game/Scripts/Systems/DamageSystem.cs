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
            GameObject gameObject, 
            SerializedDictionary<DamagableType, float> damageTypes,
            GameObject source,
            int toughnessDamage=0,
            Action callback=null
            )
        {
            var damageable = DamagableRegistry.Instance.TryGetDamagable(gameObject, out var item);
            
            if (!damageable) return false;
            
            
            if (!damageTypes.ContainsKey(damageable.Type)) return false;
            
            var finalDamage = (int)(damage * damageTypes[damageable.Type]);
            
            damageable.ServerTakeDamage(finalDamage);

            if (toughnessDamage > 0)
            {
                if (damageable is ToughnessDamagable toughnessDamagable)
                {
                    toughnessDamagable.ServerReduceToughness(toughnessDamage);
                }
            }
            
            callback?.Invoke();
            return true;
        }
    }
}