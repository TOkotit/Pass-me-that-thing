using System;
using UnityEngine;

namespace Game.Entity
{
    public class MCLocalModel
    {
        private int _health;
        private int _maxHealth;

        private bool _isDead;

        private string _currentInteractableText;

        public event Action<int, int> OnHealthChanged;
        public event Action<bool> OnDeathChanged;
        public event Action<float> OnCameraYRotationChanged;
        public event Action<Vector3> OnCameraPositionChanged;
        public event Action<Vector3> OnPlayerPositionChanged;
        public event Action<string> OnCurrentInteractableTextChanged;

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

        public string CurrentInteractableText 
        { 
            get => _currentInteractableText;
            set
            {
                if (_currentInteractableText != value) 
                    OnCurrentInteractableTextChanged?.Invoke(value);
                _currentInteractableText = value;
            }
        }

        public void ReportCameraRotation(float angleY)
        {
            OnCameraYRotationChanged?.Invoke(angleY);
        }

        public void ReportCameraPosition(Vector3 value)
        {
            OnCameraPositionChanged?.Invoke(value);
        }
        
        public void ReportPlayerPosition(Vector3 value)
        {
            OnPlayerPositionChanged?.Invoke(value);
        }
    }
}