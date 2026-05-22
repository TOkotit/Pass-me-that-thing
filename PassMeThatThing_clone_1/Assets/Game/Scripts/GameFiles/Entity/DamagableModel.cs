using System;
using Enums;
using Game.Scripts.GameFiles.Entity;
using MainCharacter;
using UnityEngine;

namespace Entity
{
    public abstract class DamagableModel
    {
        protected GlobalHealthPool _health;  
        public GlobalHealthPool Health
        {
            get
            {
                return _health;
            }
            set
            {
                _health = value;
            }
        }
        
        public event Action OnTakeHit;
        public void TakeHit()
        {
            OnTakeHit?.Invoke();
        }
    }
}