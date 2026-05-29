using System;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

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
        // [SerializeField] private LayerMask playerLayer; 
        // [SerializeField] private LayerMask doorLayer; 
        // [SerializeField] private LayerMask enemyLayer;  
        [SerializeField] private LayerMask obstacleLayer;

        // [Header("Sharing signal settings")]
        // private Transform SharingAreaCenter => transform;
        // [SerializeField] private float sharingAreaRadius;
        
        public Transform DetectedTarget => _detectedTarget;
        public float DistanceToTarget => _distanceToTarget;
        public bool IsTargetVisible => _isTargetVisible;

        public Transform Door => _door;

        // public bool IsTargetVisibleByGroup => _isTargetVisibleByGroup;

        private LayerMask _targetLayer;
        private LayerMask _onlyDoorLayer;
        
        private float _timer;
        
        private Transform _detectedTarget;
        private float _distanceToTarget = -1f;
        private bool _isTargetVisible;
        // private bool _isTargetVisibleByGroup;
        
        private Transform _door;
        
        public event Action<Transform> OnDetectedTarget;
        
        // public void SetTargetFromOther(Transform detectedTarget, float distanceToTarget, bool isTargetVisible)
        // {
        //     // Debug.Log("SetTargetFromOther");
        //     _detectedTarget = detectedTarget;
        //     _distanceToTarget = distanceToTarget;
        //     // _isTargetVisibleByGroup = isTargetVisible;
        //     _isTargetVisible = isTargetVisible;
        // }

        // public float UpdateDistanceToTarget()
        // {
        //     return Vector3.Distance(transform.position, DetectedTarget.position);
        // }

        public override void OnStartServer()
        {
            base.OnStartServer();

            _targetLayer = LayerMask.GetMask("ServerCollider");
            _onlyDoorLayer =  LayerMask.GetMask("BunkerDoor");
        }

        private void FixedUpdate()
        {
            if (!isServer) return;
            
            _timer += Time.fixedDeltaTime;

            if (_timer >= detectionInterval)
            {
                DetectTarget();
                
                DetectBunkerDoor();
                
                _timer = 0f;
            }
        }

        [Server]
        public void DetectTarget()
        {
            var maxRange = Mathf.Max(proximityAreaRadius, sightDistance);
            var targetsInRadius = new Collider[100];
            var size = Physics.OverlapSphereNonAlloc(transform.position, maxRange, 
                targetsInRadius, _targetLayer);

            if (size > 0)
            {
                Transform bestTarget = null;
                float minDistance = float.MaxValue;

                for (var i = 0; i < size; i++)
                {
                    var potentialTarget = targetsInRadius[i].transform;
                    if (potentialTarget == transform) continue;

                    var directionToTarget = (potentialTarget.position - transform.position).normalized;
                    var distance = Vector3.Distance(transform.position, potentialTarget.position);

                    var inProximity = distance <= proximityAreaRadius;
                    var inSight = false;

                    if (distance <= sightDistance)
                    {
                        var angle = Vector3.Angle(transform.forward, directionToTarget);
                        if (angle < sightAngle / 2f)
                        {
                            if (!Physics.Raycast(transform.position, directionToTarget, distance, obstacleLayer))
                            {
                                inSight = true;
                            }
                        }
                    }
                    
                    if ((inProximity || inSight) && distance < minDistance)
                        bestTarget = potentialTarget;
                    minDistance = distance;
                }


                if (bestTarget != null)
                {
                    // Debug.Log($"{size} targets detected");
                    OnDetectedTarget?.Invoke(bestTarget);
                    _detectedTarget = bestTarget;
                    _distanceToTarget = minDistance;
                    _isTargetVisible = true;
                    return;
                }
            }

            _detectedTarget = null;
            _distanceToTarget = -1f;
            _isTargetVisible = false;
        }

        [Server]
        private void DetectBunkerDoor()
        {
            var maxRange = proximityAreaRadius * 20;
            var targetsInRadius = new Collider[10];
            var size = Physics.OverlapSphereNonAlloc(transform.position, maxRange, 
                targetsInRadius, _onlyDoorLayer);
            
            if (size > 0)
            {
                _door = targetsInRadius[0].transform;
                if (!_isTargetVisible)
                {
                    OnDetectedTarget?.Invoke(_door);
                    _detectedTarget = _door;
                    _distanceToTarget = Vector3.Distance(transform.position, _door.position);;
                    _isTargetVisible = true;
                }
                return;
            }
            
            _door = null;
        }
        
        
        // Отрисовка зон в редакторе
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(proximityAreaCenter.position, proximityAreaRadius);
            
            // Gizmos.color = Color.green;
            // Gizmos.DrawWireSphere(SharingAreaCenter ? SharingAreaCenter.position : transform.position, sharingAreaRadius);
            
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