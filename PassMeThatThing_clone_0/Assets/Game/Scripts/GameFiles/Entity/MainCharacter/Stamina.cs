using System;
using UnityEngine;

namespace MainCharacter
{
    public class Stamina
    {
        private float _maxStamina = 100;
        private float _currentStamina;
        public event Action<float> OnStaminaChanged;
        
        public float CurrentStamina
        {
            get { return _currentStamina; }
            private set 
            {
                _currentStamina = Mathf.Clamp(value, 0, _maxStamina);
                OnStaminaChanged?.Invoke(_currentStamina);
            }
        }

        public bool SpendStamina( float stamina )
        {
            if (stamina >= CurrentStamina){ return false; }
            CurrentStamina -= stamina;
            return true;
        }

        public float RestoreStamina(float stamina)
        {
            CurrentStamina += stamina;
            return CurrentStamina;
        }
    }
}