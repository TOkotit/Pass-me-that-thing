using System;
using Entity;
using Game.Scripts.GameFiles.Entity.Enemy;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings.Turrets
{
    /// <summary>
    /// турели которые бьют мобов и требуют определенный тип ресов
    /// </summary>
    public class Turret : Building
    {
        [SerializeField] protected TargetDetector targetDetector;
        [SerializeField] protected TurretAttackController turretAttackController;
        
        protected TurretModel TurretModel;
        protected TurretData TurretData;
        


        private new void Awake()
        {
            base.Awake();
            TurretModel = new TurretModel();
        }

        public override void OnDeath()
        {
            base.OnDeath();
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
            base.OnHealthChanged(currentHealth, maxHealth);
        }
    }
}