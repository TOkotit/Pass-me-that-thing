using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Items.ItemPhysics
{
    public class Projectile : NetworkBehaviour
    {
        [Header("Common")]
        [SerializeField] private float speed = 20f;               
        [SerializeField] private float startSpeed = 0f;           
        [SerializeField] private float maxSpeed = 0f;             
        [SerializeField] private ProjectileMode mode = ProjectileMode.Ballistic;
        [SerializeField] protected Rigidbody rb;
        [Header("Deceleration")]
        [SerializeField] private float deceleration = 2f;    


        private void Start()
        {
            var initialVelocity = startSpeed > 0 ? startSpeed : speed;
            var velocity = transform.forward * initialVelocity;
            if (maxSpeed > 0 && velocity.magnitude > maxSpeed)
                velocity = velocity.normalized * maxSpeed;
            rb.linearVelocity = velocity;
            rb.useGravity = mode switch
            {
                ProjectileMode.Ballistic or ProjectileMode.DeceleratingBallistic => true,
                ProjectileMode.ConstantSpeedNoGravity => false,
                ProjectileMode.ConstantThrustWithGravity => true,
                _ => rb.useGravity
            };
        }

        private void FixedUpdate()
        {
            var forward = transform.forward;

            switch (mode)
            {
                case ProjectileMode.ConstantSpeedNoGravity:
                    var targetVel = forward * speed;
                    if (maxSpeed > 0 && targetVel.magnitude > maxSpeed)
                        targetVel = targetVel.normalized * maxSpeed;
                    rb.linearVelocity = targetVel;
                    break;

                case ProjectileMode.ConstantThrustWithGravity:
                    var currentForwardSpeed = Vector3.Dot(rb.linearVelocity, forward);
                    var speedError = speed - currentForwardSpeed;
                    var accel = speedError / Time.fixedDeltaTime;
                    accel = Mathf.Clamp(accel, -100f, 100f); 
                    rb.AddForce(forward * accel, ForceMode.Acceleration);
                    if (maxSpeed > 0 && rb.linearVelocity.magnitude > maxSpeed)
                        rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
                    break;

                case ProjectileMode.DeceleratingBallistic:
                    rb.linearVelocity *= (1 - deceleration * Time.fixedDeltaTime);
                    if (maxSpeed > 0 && rb.linearVelocity.magnitude > maxSpeed)
                        rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
                    break;
            }
        }
        [Server]
        public void StopProjectile()
        {
            RpcStopProjectile();
            StopProjectileInternal();
        }

        [ClientRpc]
        private void RpcStopProjectile()
        {
            StopProjectileInternal();
        }

        private void StopProjectileInternal()
        {
            if (rb)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false;
                rb.isKinematic = true;      
            }
            enabled = false;               
        }
        private void Update()
        {
            if (rb.linearVelocity != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }

        public void DestroyAfterDelay(float delay)
        {
            Destroy(gameObject, delay);
        }
    }
}