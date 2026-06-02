
using System;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity
{
    public class HealthPool
    {
        private int _maxHealth;
        private int _currentHealth;

        public int MaxHealth => _maxHealth;
        public int CurrentHealth
        {
            get => _currentHealth;
            private set
            {
                OnHealthChanged.Invoke(value);
                _currentHealth = value;
                if (_currentHealth >= _maxHealth) _currentHealth = _maxHealth;
                if (_currentHealth <= 0)
                {
                    _currentHealth = 0;
                    OnDeath?.Invoke();
                }
            }
        }

        public event Action<int> OnHealthChanged;
        public event Action OnDeath;

        public HealthPool(int maxHealth)
        {
            SetMaxHealth(maxHealth, true);
        }

        public void SetMaxHealth(int newMaxHealth, bool fullHeal)
        {
            _maxHealth = newMaxHealth;
            if (fullHeal) CurrentHealth = _maxHealth;
        }

        public float TakeDamage(int damage)
        {
            if (damage <= 0) return 0;
            CurrentHealth -= damage;
            OnHealthChanged?.Invoke(CurrentHealth);
            Debug.Log("Здоровье после получения урона: " + CurrentHealth);
            return _currentHealth;
        }
    }
}