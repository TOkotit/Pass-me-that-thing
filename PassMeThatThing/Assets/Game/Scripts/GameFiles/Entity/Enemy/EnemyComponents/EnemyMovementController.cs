using Mirror;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemyMovementController : NetworkBehaviour
    {
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Rigidbody rb;
        
        [Header("Настройки движения")]
        private Transform _target;
        private float _moveForce = 15f;
        private float _maxSpeed = 5f;
        private float _rotationSpeed = 10f;

        private void Start()
        {
            navMeshAgent.updatePosition = false;
            navMeshAgent.updateRotation = false;
            
            navMeshAgent.updateUpAxis = false;
        }

        private void Update()
        {
            navMeshAgent.nextPosition = rb.position;
        }

        private void FixedUpdate()
        {
            MoveWithPhysics();
        }

        private void MoveWithPhysics()
        {
            var direction = navMeshAgent.desiredVelocity;

            direction.y = 0;

            if (direction.sqrMagnitude > 0.1f)
            {
                direction.Normalize();

                var currentVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

                if (currentVelocity.magnitude < _maxSpeed)
                {
                    rb.AddForce(direction * _moveForce, ForceMode.Force);
                }

                var targetRotation = Quaternion.LookRotation(direction);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * _rotationSpeed));
            }
        }
        
        
        
        [Server]
        public void NavigateTo(Transform target)
        {
            navMeshAgent.isStopped = false;
            
            _target = target;
            navMeshAgent.SetDestination(_target.position);
        }

        [Server]
        public void StopNavigating()
        {
            navMeshAgent.isStopped = true;
        }

        [Server]
        public void SetSpeed(float speed)
        {
            _moveForce = speed;
            _maxSpeed = speed;
        }

        [Server]
        public void RotateTo(Vector3 target, float rotationSpeed=1f)
        {
            var direction = (target - transform.position).normalized;
            
            direction.y = 0;
            
            var lookRotation = Quaternion.LookRotation(direction);
            
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, lookRotation, Time.fixedDeltaTime * _rotationSpeed));
            
            
            
            // while (Quaternion.Angle(lookRotation, transform.rotation) >= 5f)
            // {
            //     transform.rotation = Quaternion.Slerp(transform.rotation, 
            //         lookRotation, 
            //         Time.fixedDeltaTime * rotationSpeed);
            // }
        }
    }
}