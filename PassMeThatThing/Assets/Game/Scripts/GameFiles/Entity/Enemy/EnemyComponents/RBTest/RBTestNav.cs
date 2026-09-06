using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Game.Scripts.GameFiles.Entity.Enemy.EnemyComponents.RBTest
{
    public class RBTestNav : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent navMeshAgent;

        [Header("Движение через Rigidbody Иначе через navAgent")]
        [SerializeField] private bool isMovingRB;

        [SerializeField] private Rigidbody rb;
        [SerializeField] private Transform point;

        private Vector3 _targetPosition;
        public float _moveForce;
        public float _maxSpeed;

        public Vector3 _moveDirection;
        public Vector3 _currentVelocity;

        public Vector3 _rotateDirection;
        public Quaternion _lookRotation;
        public float _rotationSpeed = 10f;


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
                NavigateTo(point.position);
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
            _moveDirection = navMeshAgent.desiredVelocity;

            _moveDirection.y = 0;

            if (_moveDirection.sqrMagnitude > 0.1f)
            {
                _moveDirection.Normalize();

                _currentVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

                if (_currentVelocity.magnitude < _maxSpeed)
                {
                    rb.AddForce(_moveDirection * _moveForce, ForceMode.VelocityChange);
                }

                rb.MoveRotation(Quaternion.Slerp(rb.rotation,
                    Quaternion.LookRotation(_moveDirection), Time.fixedDeltaTime * _rotationSpeed));
            }
        }

        public void NavigateTo(Vector3 pos)
        {
            if (navMeshAgent.enabled)
            {
                navMeshAgent.isStopped = false;

                _targetPosition = pos;
                navMeshAgent.SetDestination(_targetPosition);
            }
        }


        public void StopNavigating()
        {
            if (navMeshAgent.enabled)
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


        public void RotateTo(Vector3 target, float rotationSpeed = 1f)
        {
            _rotateDirection = (target - transform.position).normalized;

            _rotateDirection.y = 0;

            _lookRotation = Quaternion.LookRotation(_rotateDirection);

            if (isMovingRB)
            {
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, _lookRotation, Time.fixedDeltaTime * _rotationSpeed));
            }
            else
            {
                while (Quaternion.Angle(_lookRotation, transform.rotation) >= 5f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        _lookRotation,
                        Time.fixedDeltaTime * rotationSpeed);
                }
            }
        }
    }
}