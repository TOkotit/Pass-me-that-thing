using System;

namespace Game.Entity
{
    public class MCLocalModel
    {
        private int health;
        private int maxHealth;

        public event Action<int, int> OnHealthChanged;

        public int Health
        {
            get => health;
            set
            {
                OnHealthChanged?.Invoke(value, maxHealth);
                health = value;
            }
        }

        public int MaxHealth
        {
            get => maxHealth;
            set => maxHealth = value;
        }
    }
}