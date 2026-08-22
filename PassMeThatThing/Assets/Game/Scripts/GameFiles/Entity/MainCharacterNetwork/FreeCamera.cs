using Game.Scripts.GameFiles.Entity.MainCharacterNetwork;
using UnityEngine;

namespace MainCharacterNetwork
{
    public class FreeCamera : MonoBehaviour, IControllable
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float sprintMultiplier = 2f;
        [SerializeField] private float verticalSpeed = 5f;
        [SerializeField] private float jumpHeight = 2f;   
        
        [Header("Distance Constraint")]
        [SerializeField] private Transform anchorObject;
        [SerializeField] private float maxDistance = 20f;
        [SerializeField] private float slowStartDistance = 15f;
        [SerializeField] private float minSpeedMultiplier = 0.1f;

        private Vector3 _moveDirection;
        private bool _isSprinting;
        private bool _isEnabled = true;
        private bool _isDescending;   
        public Vector3 LastVelocity { get; private set; }

        public void Move(Vector3 direction)
        {
            _moveDirection = direction;
        }

        public void Rotate(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        public void Jump()
        {
            transform.position += Vector3.up * jumpHeight;
        }

        public void SetSprinting(bool isSprinting)
        {
            _isSprinting = isSprinting;
        }

        public void Control(bool isPressed)
        {
            _isDescending = isPressed;   
        }

        public void LockUpMovement()
        {
            _isEnabled = false;
            _moveDirection = Vector3.zero;
            _isDescending = false;
        }

        public void UnlockMovement()
        {
            _isEnabled = true;
        }

        public void DisableController()
        {
        }

        public void EnableController()
        {
        }

        public Vector3 GetCurrentVelocity()
        {
            var horizontal = _moveDirection.normalized * moveSpeed * (_isSprinting ? sprintMultiplier : 1f);
            var vertical = _isDescending ? Vector3.down * verticalSpeed : Vector3.zero;
            return horizontal + vertical;
        }

        private void Update()
        {
            if (!_isEnabled) return;

            var speedMultiplier = 1f;
            if (anchorObject)
            {
                var dist = Vector3.Distance(transform.position, anchorObject.position);
                if (dist > slowStartDistance && maxDistance > slowStartDistance)
                {
                    var t = Mathf.InverseLerp(slowStartDistance, maxDistance, dist);
                    speedMultiplier = Mathf.Lerp(1f, minSpeedMultiplier, t);
                }
                else if (dist >= maxDistance)
                {
                    speedMultiplier = minSpeedMultiplier;
                }
            }

            var currentSpeed = moveSpeed * (_isSprinting ? sprintMultiplier : 1f) * speedMultiplier;
            var motion = _moveDirection.normalized * (currentSpeed * Time.deltaTime);

            if (_isDescending)
            {
                motion += Vector3.down * (verticalSpeed * Time.deltaTime);
            }

            transform.position += motion;
            LastVelocity = motion / Time.deltaTime;
        }
    }
}