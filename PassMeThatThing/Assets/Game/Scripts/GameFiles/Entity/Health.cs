using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Entity
{
    public class Health
    { 
        private int _maxHealth = 100 * BC.Health;
        private int _currentHealth;
        private Dictionary<Enums.DamageTypes, float> _vulnerabilities = new Dictionary<Enums.DamageTypes, float>();
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
        public IReadOnlyDictionary<Enums.DamageTypes, float> Vulnerabilities => _vulnerabilities;

        public bool DamageImmunity
        {
            get => _damageImmunity;
            set => _damageImmunity = value;
        }

        public event Action<int> OnHealthChanged;
        public event Action OnDeath;
        
        public float TakeDamage(float damage, Enums.DamageTypes damageType)
        {
            Debug.Log("Object got damaged!");
            Debug.Log(_currentHealth);
            if (damage <= 0) return 0;
            if (!_vulnerabilities.TryGetValue(damageType, out float coefficient))
                coefficient = 1f;
            var total = (int)(damage * coefficient);
            if (_damageImmunity) total = 0;
            CurrentHealth -= total;
            OnHealthChanged?.Invoke(CurrentHealth);
            
            if (CurrentHealth <= 0) OnDeath?.Invoke();
            return _currentHealth;
        }

        public int Heal(int heal)
        {
            if (heal <= 0) return 0;
            CurrentHealth += heal;
            OnHealthChanged?.Invoke(CurrentHealth);
            //анналогично
            return heal;
        }
        public Health()
        {
            _currentHealth = _maxHealth;
        }
        public void SetVulnerability(Enums.DamageTypes damageType, float vulnerability)
        {
            _vulnerabilities[damageType] = vulnerability;
        }

        public void AddVulnerability(Enums.DamageTypes damageType, float vulnerability)
        {
            if (_vulnerabilities.ContainsKey(damageType))
                _vulnerabilities[damageType] += vulnerability;
            else
                _vulnerabilities[damageType] = 1f + vulnerability;
        }

        public void SetMaxHealth(int newMaxHealth, bool isFullHealNeeded)
        {
            _maxHealth = newMaxHealth;
            if (isFullHealNeeded) CurrentHealth = _maxHealth;
        }
        
    }
}