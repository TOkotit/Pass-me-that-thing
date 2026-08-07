using System;
using System.Collections;
using Entity;
using Game.Scripts.GameFiles.Entity.Buildings.WireSystem;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Buildings.Turrets
{
    /// <summary>
    /// турели которые бьют мобов и требуют определенный тип ресов
    /// </summary>
    public class FireTurret : Turret, IDependsOnWireNet
    {
        private const float rotationSpeed = 20f;

        [SerializeField] private WireNodePort inputPort;
        [SerializeField] private GameObject turretHead;
        [SerializeField] private GameObject firePoint;

        [SerializeField] private LineRenderer lineRenderer;

        private float _elapsedAttack;
        private Vector3 _dir;
        private Vector3 _headRotation;
        private Coroutine _endDrawRayCoroutine;

        public float Damage => TurretData.damage;
        public float AttackSpeed => TurretData.attackSpeed;

        private bool _isTurretWork;

        private new void Awake()
        {
            base.Awake();
            lineRenderer.positionCount = 2;
        }

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
            BuildingData = buildingsDatabase.GetBuildingFromAll("fireTurret");
            TurretData = turretDatabas.GetTurret("fireTurret");
        }

        private void FixedUpdate()
        {
            if (!isServer) return;
            
            if (!_isTurretWork) return;
            if (!targetDetector.IsTargetVisible) return;
            if (targetDetector.DetectedTargetObject == null) return;

            _dir = targetDetector.DetectedTargetObject.transform.position - turretHead.transform.position;

            _headRotation = Quaternion.Lerp(turretHead.transform.rotation,
                Quaternion.LookRotation(_dir),
                rotationSpeed * Time.deltaTime).eulerAngles;
            RpcRotateTurret(_headRotation);

            _elapsedAttack += Time.fixedDeltaTime;
            if (_elapsedAttack >= (1 / AttackSpeed))
            {
                Attack();
                _elapsedAttack = 0f;
            }
        }

        [Server]
        public void Attack()
        {
            turretAttackController.AttackRay(Damage, targetDetector.DetectedTargetObject);

            RpcDrawLine(
                firePoint.transform.position, 
                targetDetector.DetectedTargetObject.transform.position);
        }

        [ClientRpc]
        public void RpcRotateTurret(Vector3 headRotation)
        {
            turretHead.transform.rotation = Quaternion.Euler(0f, headRotation.y, 0f);
        }

        [ClientRpc]
        public void RpcDrawLine(Vector3 firepoint, Vector3 target)
        {
            lineRenderer.enabled = true;

            lineRenderer.SetPosition(0, firepoint);
            lineRenderer.SetPosition(1, target);

            if (_endDrawRayCoroutine != null) StopCoroutine(_endDrawRayCoroutine);
            StartCoroutine(EndDrawRayCoroutine());
        }

        private IEnumerator EndDrawRayCoroutine()
        {
            yield return new WaitForSeconds(1f);
            lineRenderer.enabled = false;
        }


        public override void OnDeath()
        {
            
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
            Debug.Log($"[Fire Turret] OnHealthChanged {currentHealth} / {maxHealth}");
        }

        public void OnWireNetWorkingStateChanged(WireType type, bool isNetWorking)
        {
            _isTurretWork = isNetWorking;
            Debug.Log($"[Fire Turret] IsTurretWork {_isTurretWork}");
        }
    }
}