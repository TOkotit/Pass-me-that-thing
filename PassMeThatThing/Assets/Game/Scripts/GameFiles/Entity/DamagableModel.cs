using System;
using Enums;
using Game.Scripts.GameFiles.Entity;
using MainCharacter_old;
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

        public event Action OnTakeHit;
        public void TakeHit() => OnTakeHit?.Invoke();
    }
}