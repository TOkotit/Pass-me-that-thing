using System;
using UnityEngine;

namespace Entity
{
    public abstract class DamagableModel
    {
        public HealthPool HealthPool { get; set; }

        public event Action<int, int> OnHealthChanged; //currHp, maxHp
        public event Action OnDeath;

        public void TakeDamage(int damage)
        {
            if (HealthPool == null) return;
            int newHealth = HealthPool.TakeDamage(damage);
            
            OnHealthChanged?.Invoke(HealthPool.CurrentHealth, HealthPool.MaxHealth);
            if (HealthPool.CurrentHealth <= 0) OnDeath?.Invoke();
        }

        public void SetHealth(int newHealth)
        {
            if (HealthPool == null) return;
            HealthPool.SetCurrentHealth(newHealth);
            
            OnHealthChanged?.Invoke(HealthPool.CurrentHealth, HealthPool.MaxHealth);
            if (HealthPool.CurrentHealth <= 0) OnDeath?.Invoke();
        }

        public void SetMaxHealth(int newMaxHealth, bool fullHeal)
        {
            if (HealthPool == null) return;
            HealthPool.SetMaxHealth(newMaxHealth);
            if (newMaxHealth < HealthPool.CurrentHealth || fullHeal ) 
                SetHealth(newMaxHealth);
        }
    }
}