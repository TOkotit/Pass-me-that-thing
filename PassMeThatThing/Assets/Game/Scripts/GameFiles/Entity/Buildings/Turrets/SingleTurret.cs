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
        private const float rotationSpeed = 20f;

        [SerializeField] private WireNodePort inputPort;
        [SerializeField] private GameObject turretHead;
        [SerializeField] private GameObject firePoint;
        
        private float _elapsedAttack;
        private Vector3 _dir;
        private Vector3 _headRotation;

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

            _dir = targetDetector.DetectedTargetObject.transform.position - turretHead.transform.position;

            _headRotation = Quaternion.Lerp(turretHead.transform.rotation,
                Quaternion.LookRotation(_dir),
                rotationSpeed * Time.deltaTime).eulerAngles;
            turretHead.transform.rotation = Quaternion.Euler(0f, _headRotation.y, 0f);

            _elapsedAttack += Time.fixedDeltaTime;
            if (_elapsedAttack >= (1 / AttackSpeed))
            {
                Attack();
                _elapsedAttack = 0f;
            }
        }

        public void Attack()
        {
            turretAttackController.AttackRay(Damage,
                    firePoint.transform, targetDetector.DetectedTargetObject);
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