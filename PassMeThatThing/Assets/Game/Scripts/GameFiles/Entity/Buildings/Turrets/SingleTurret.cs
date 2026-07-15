using System;
using Entity;
using Game.Scripts.GameFiles.Entity.Buildings.WireSystem;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.Turrets
{
    /// <summary>
    /// турели которые бьют мобов и требуют определенный тип ресов
    /// </summary>
    public class SingleTurret : Turret, IDependsOnWireNet
    {
        [SerializeField] private WireNodePort inputPort;
        
        private float _elapsedAttack;
        
        public float Damage => TurretData.damage;
        public float AttackSpeed => TurretData.attackSpeed;

        public bool IsTurretWork;
        
        public new void Start()
        {
            base.Start();
            if (isServer)
                inputPort.OnWireNetStateChanged += OnWireNetWorkingStateChanged;
        }

        public new void OnDestroy()
        {
            if (isServer)
                inputPort.OnWireNetStateChanged -= OnWireNetWorkingStateChanged;
            base.OnDestroy();
        }
        
        [Inject]
        public void Construct(BuildingsDatabase buildingsDatabase, TurretDatabase turretDatabas)
        {
            BuildingData = buildingsDatabase.GetBuildingFromAll("singleTurret");
            TurretData = turretDatabas.GetTurret("singleTurret");
        }

        private void FixedUpdate()
        {
            if (!isServer) return;
            
            if (!IsTurretWork) return;
            if (!targetDetector.IsTargetVisible) return;
            
            
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

        public void OnWireNetWorkingStateChanged(bool isNetWorking)
        {
            IsTurretWork = isNetWorking;
            Debug.Log($"[Single Turret] IsTurretWork {IsTurretWork}");
        }
    }
}