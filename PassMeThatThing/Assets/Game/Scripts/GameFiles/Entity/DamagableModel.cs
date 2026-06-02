using System;
using UnityEngine;

namespace Entity
{
    public abstract class DamagableModel
    {
        protected HealthPool _healthPool;
        public HealthPool HealthPool
        {
            get => _healthPool;
            set => _healthPool = value;
        }

        public event Action<int> OnHealthChanged;
        public event Action OnDeath;

        public void TakeDamage(int damage)
        {
            if (_healthPool == null) return;
            int newHealth = _healthPool.TakeDamage(damage);
            OnHealthChanged?.Invoke(newHealth);
            if (newHealth <= 0) OnDeath?.Invoke();
        }

        public void SetHealth(int newHealth)
        {
            if (_healthPool == null) return;
            _healthPool.SetCurrentHealth(newHealth);
            OnHealthChanged?.Invoke(newHealth);
            if (newHealth <= 0) OnDeath?.Invoke();
        }
    }
}