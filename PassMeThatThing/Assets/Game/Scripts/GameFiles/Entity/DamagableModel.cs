using System;
using Enums;
using MainCharacter;
using UnityEngine;

namespace Entity
{
    public abstract class DamagableModel
    {
        protected Health _health;  
        public Health Health
        {
            get
            {
                return _health;
            }
            set
            {
                _health = value;
                Debug.Log(_health);
            }
        }

        protected TeamsType teamType;
        public TeamsType TeamType => teamType;
        
        public event Action OnTakeHit;
        public void TakeHit()
        {
            OnTakeHit?.Invoke();
        }
    }
}