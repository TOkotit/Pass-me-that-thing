using System;
using UnityEngine;

namespace Entity
{
    public class DamagableModel
    {
        public HealthPool HealthPool { get; set; }

        public event Action<int, int> OnHealthChanged; //currHp, maxHp
        public event Action OnDeath;

        public event Action<int> OnDamage; //delta hp
        public event Action<int> OnHeal; //delta hp

        public void TakeDamage(int damage)
        {
            if (HealthPool == null) return;
            HealthPool.TakeDamage(damage);

            OnDamage?.Invoke(damage);
            OnHealthChanged?.Invoke(HealthPool.CurrentHealth, HealthPool.MaxHealth);
            if (HealthPool.CurrentHealth <= 0) OnDeath?.Invoke();
        }

        public void Heal(int value)
        {
            if (HealthPool == null) return;
            HealthPool.Heal(value);

            OnHeal?.Invoke(value);
            OnHealthChanged?.Invoke(HealthPool.CurrentHealth, HealthPool.MaxHealth);
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