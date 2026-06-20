using Mirror;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemyMovementController : NetworkBehaviour
    {
        [SerializeField] private NavMeshAgent navMeshAgent;

        [Header("Движение через Rigidbody Иначе через navAgent")]
        [SerializeField] private bool isMovingRB;
        
        [SerializeField] private Rigidbody rb;
        
        private Transform _target;
        private float _moveForce;
        private float _maxSpeed;
        private float _rotationSpeed = 10f;

        private void Start()
        {
            if (isMovingRB)
            {
                navMeshAgent.updatePosition = false;
                navMeshAgent.updateRotation = false;
                navMeshAgent.updateUpAxis = false;
            }
        }

        private void Update()
        {
            if (isMovingRB)
            {
                navMeshAgent.nextPosition = rb.position;
            }
        }

        private void FixedUpdate()
        {
            if (isMovingRB)
            {
                MoveWithPhysics();
            }
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

        public void EnableNavAgent()
        {
            navMeshAgent.enabled = true;
        }
        
        public void DisableNavAgent()
        {
            navMeshAgent.enabled = false;
        }

        [Server]
        public void SetSpeed(float speed)
        {
            if (isMovingRB)
            {
                _moveForce = speed;
                _maxSpeed = speed;
            }
            else
            {
                navMeshAgent.speed = speed;
            }
        }

        [Server]
        public void RotateTo(Vector3 target, float rotationSpeed=1f)
        {
            var direction = (target - transform.position).normalized;
            
            direction.y = 0;
            
            var lookRotation = Quaternion.LookRotation(direction);

            if (isMovingRB)
            {
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, lookRotation, Time.fixedDeltaTime * _rotationSpeed));
            }
            else
            {
                while (Quaternion.Angle(lookRotation, transform.rotation) >= 5f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, 
                        lookRotation, 
                        Time.fixedDeltaTime * rotationSpeed);
                }
            }
        }
    }
}