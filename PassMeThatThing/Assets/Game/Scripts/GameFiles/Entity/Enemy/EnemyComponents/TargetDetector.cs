using System;
using FishNet.Object;
using Game.Entity;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class TargetDetector : NetworkBehaviour
    {
        [SerializeField] private float detectionInterval = 0.5f;
        
        [Header("Proximity settings")]
        [SerializeField] private Transform proximityAreaCenter;
        [SerializeField] private float proximityAreaRadius;
        
        [Header("Sight settings")]
        [SerializeField] private float sightDistance;
        [SerializeField] private float sightAngle; 
        [SerializeField] private LayerMask obstacleLayer;

        [Inject]
        private EnemyTargetsRegistry _enemyTargetsRegistry;
        
        //temp
        private float _timer;
        private float _maxDetectionRange;
        private float _tempDistance;
        private float _minDistance;
        private int _bestPriority;
        private Vector3 _bestTarget;
        private bool _bestTargetFound;
        private Vector3 _direction;
        private float _tempAngleToTargetDirection;
        private bool _inSight;
        private bool _inProximity;


        public Vector3 DetectedTarget { get; private set; }
        public float DistanceToTarget { get; private set; } = -1f;
        public bool IsTargetVisible { get; private set; }


        public event Action<Vector3> OnDetectedTarget;

        public override void OnStartServer()
        {
            _maxDetectionRange = Mathf.Max(proximityAreaRadius, sightDistance);
        }

        private void FixedUpdate()
        {
            if (!isServer) return;
            
            _timer += Time.fixedDeltaTime;

            if (_timer >= detectionInterval)
            {
                CalculateTarget();
                _timer = 0f;
            }
        }

        [Server]
        private void CalculateTarget()
        {
            _minDistance = float.MaxValue;
            _bestTargetFound = false;
            
            foreach (var d in _enemyTargetsRegistry.EnemyTargetObjects)
            {
                _tempDistance =  Vector3.Distance(d.Key.transform.position, transform.position);
                if (_tempDistance > _maxDetectionRange) continue;
                
                _direction = (d.Key.transform.position - transform.position).normalized;

                _inProximity = false;
                _inSight = false;
                
                if (_tempDistance <= proximityAreaRadius)
                {
                    if (!Physics.Raycast(transform.position, _direction, _tempDistance, obstacleLayer))
                    {
                        _inProximity = true;
                    }
                }
                else if (_tempDistance <= sightDistance)
                {
                    _tempAngleToTargetDirection = Vector3.Angle(transform.forward, _direction);
                    if (_tempAngleToTargetDirection < sightAngle / 2f)
                    {
                        if (!Physics.Raycast(transform.position, _direction, _tempDistance, obstacleLayer))
                        {
                            _inSight = true;
                        }
                    }
                }
                
                if ((_inProximity || _inSight) 
                    && _tempDistance <= _minDistance
                    && d.Value.Priority >= _bestPriority)
                {
                    _bestTarget = d.Key.transform.position;
                    _bestTargetFound = true;
                    _minDistance = _tempDistance;
                    _bestPriority = d.Value.Priority;
                }
            }
            
            if (_bestTargetFound)
            {
                OnDetectedTarget?.Invoke(_bestTarget);
                DetectedTarget = _bestTarget;
                DistanceToTarget = _minDistance;
                IsTargetVisible = true;
            }
            else
            {
                DetectedTarget = new Vector3();
                DistanceToTarget = -1f;
                IsTargetVisible = false;
            }
        }
        
        
        // Отрисовка зон в редакторе
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(proximityAreaCenter.position, proximityAreaRadius);
            
            Gizmos.color = Color.red;
            var leftRayRotation = Quaternion.AngleAxis(-sightAngle / 2, Vector3.up);
            var rightRayRotation = Quaternion.AngleAxis(sightAngle / 2, Vector3.up);
            var leftRayDirection = leftRayRotation * transform.forward;
            var rightRayDirection = rightRayRotation * transform.forward;

            Gizmos.DrawRay(transform.position, leftRayDirection * sightDistance);
            Gizmos.DrawRay(transform.position, rightRayDirection * sightDistance);
        }
    }
}