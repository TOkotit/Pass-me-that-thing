using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Entity
{
    //OLD
    public class Health_old
    { 
        private int _maxHealth = 100 * BC.Health;
        private int _currentHealth;
        private bool _damageImmunity;
        public int CurrentHealth
        {
            get { return _currentHealth; }
            private set
            {
                _currentHealth = value;
                if (_currentHealth >= _maxHealth) _currentHealth = _maxHealth;
                if (_currentHealth <= 0) {
                    _currentHealth = 0;
                    OnDeath?.Invoke();
                }

            }
        }
       
        public bool DamageImmunity
        {
            get => _damageImmunity;
            set => _damageImmunity = value;
        }

        public event Action<int> OnHealthChanged;
        public event Action OnDeath;
        
        

        public int Heal(int heal)
        {
            if (heal <= 0) return 0;
            CurrentHealth += heal;
            OnHealthChanged?.Invoke(CurrentHealth);
            //анналогично
            return heal;
        }
        public Health_old()
        {
            _currentHealth = _maxHealth;
        }
        

        public void SetMaxHealth(int newMaxHealth, bool isFullHealNeeded)
        {
            _maxHealth = newMaxHealth;
            if (isFullHealNeeded) CurrentHealth = _maxHealth;
        }
        
    }
}