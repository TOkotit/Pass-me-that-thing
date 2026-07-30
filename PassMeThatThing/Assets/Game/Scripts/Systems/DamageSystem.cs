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
            Damagable damageable, 
            SerializedDictionary<DamagableType, float> damageTypes = null,
            int toughnessDamage=0,
            Action callback=null
            )
        {
            
            
            if (!damageable) return false;

            if (damageTypes != null)
            {
                if (!damageTypes.ContainsKey(damageable.Type)) return false;
            }
            
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