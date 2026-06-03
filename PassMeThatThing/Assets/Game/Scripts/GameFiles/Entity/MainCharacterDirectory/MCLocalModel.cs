using System;

namespace Game.Entity
{
    public class MCLocalModel
    {
        private int _health;
        private int _maxHealth;

        private bool _isDead;

        public event Action<int, int> OnHealthChanged;
        public event Action<bool> OnDeathChanged;

        public int Health
        {
            get => _health;
            set
            {
                OnHealthChanged?.Invoke(value, _maxHealth);
                _health = value;
            }
        }

        public int MaxHealth
        {
            get => _maxHealth;
            set => _maxHealth = value;
        }

        public bool IsDead
        {
            get => _isDead;
            set
            {
                if (_isDead != value)
                    OnDeathChanged?.Invoke(value);
                _isDead = value;
            }
        }
    }
}