using System;
using Entity;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.Turrets
{
    /// <summary>
    /// турели которые бьют мобов и требуют определенный тип ресов
    /// </summary>
    public class SingleTurret : Turret
    {
        private float _elapsedAttack;
        
        public float Damage => TurretData.damage;
        public float AttackSpeed => TurretData.attackSpeed;
        
        [Inject]
        public void Construct(BuildingsDatabase buildingsDatabase, TurretDatabase turretDatabas)
        {
            BuildingData = buildingsDatabase.GetBuildingFromAll("singleTurret");
            TurretData = turretDatabas.GetTurret("singleTurret");
        }

        private void FixedUpdate()
        {
            if (!targetDetector.IsTargetVisible) return;
            if (!isServer) return;
            
            
            _elapsedAttack += Time.fixedDeltaTime;
            if (_elapsedAttack >= (1 / AttackSpeed))
            {
                turretAttackController.AttackRay(Damage, targetDetector.DetectedTargetObject);
                
                _elapsedAttack = 0f;
            }
        }


        public override void OnDeath()
        {
            
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
            Debug.Log($"[Single Turret] OnHealthChanged {currentHealth} / {maxHealth}");
        }
    }
}