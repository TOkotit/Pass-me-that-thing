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

            _targetLayer = LayerMask.GetMask("Player", "BunkerDoor");
            _onlyDoorLayer =  LayerMask.GetMask("BunkerDoor");
        }

        private void FixedUpdate()
        {
            if (!isServer) return;
            
            _timer += Time.fixedDeltaTime;

            if (_timer >= detectionInterval)
            {
                DetectBunkerDoor();
                DetectTarget();
                _timer = 0f;
            }
        }

        [Server]
        public void DetectTarget()
        {
            var maxRange = Mathf.Max(proximityAreaRadius, sightDistance);
            var targetsInRadius = new Collider[10];
            var size = Physics.OverlapSphereNonAlloc(transform.position, maxRange, 
                targetsInRadius, _targetLayer);
            
            if (size > 0)
            {
                var target = targetsInRadius[0].transform;
                var directionToTarget = (target.position - transform.position).normalized;
                var distance = Vector3.Distance(transform.position, target.position);
                
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

                if (inProximity || inSight)
                {
                    OnDetectedTarget?.Invoke(target);
                    _detectedTarget = target;
                    _distanceToTarget = distance;
                    _isTargetVisible = true;
                    
                    // if (!IsTargetVisibleByGroup)
                    // {
                    //     var enemiesInRadius = new Collider[10];
                    //     var enemiesSize = Physics.OverlapSphereNonAlloc(transform.position, 
                    //         sharingAreaRadius, 
                    //         enemiesInRadius, 
                    //         enemyLayer);
                    //     
                    //     if (enemiesSize > 0)
                    //     {
                    //         // Debug.Log("FOR ENEMIES: TARGET DETECTED IN AREA");
                    //         foreach (var enemy in enemiesInRadius)
                    //         {
                    //             enemy?.GetComponent<TargetDetector>()?.SetTargetFromOther(DetectedTarget,
                    //                 DistanceToTarget, IsTargetVisible);
                    //         }
                    //     }
                    // }
                    return;
                }
            }
            
            _detectedTarget = null;
            _distanceToTarget = -1f;
            _isTargetVisible = false;
            // _isTargetVisibleByGroup = false;
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
                return;
            }
            
            _door = null;
        }
        
        
        // Отрисовка зон в редакторе
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(proximityAreaCenter ? proximityAreaCenter.position : transform.position, proximityAreaRadius);
            
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