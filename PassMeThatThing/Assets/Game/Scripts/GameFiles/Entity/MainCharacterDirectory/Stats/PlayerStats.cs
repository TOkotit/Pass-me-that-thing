using UnityEngine;

namespace Game.Entity.Stats
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Stats/PlayerStats")]
    public class PlayerStats : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField] private float speed = 140f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float gravity = 9.81f;

        [Header("Carry & Throw")]
        [SerializeField] private float baseCarry = 15f;
        [SerializeField] private float maxHoldDistance = 3.5f;
        [SerializeField] private float holdSoftZone = 3f;
        [SerializeField] private float throwForceGrow = 5f;
        [SerializeField] private float maxThrowForce = 15f;
        [SerializeField] private float minChargeTime = 0.3f;

        [Header("Joint Settings")]
        [SerializeField] private float jointSpring = 3000f;
        [SerializeField] private float jointDamper = 50f;
        [SerializeField] private float angularSpring = 500f;
        [SerializeField] private float angularDamper = 10f;

        [Header("Hold Force")]
        [SerializeField] private float baseHoldForce = 500f;
        [SerializeField] private float holdDamping = 50f;
        [SerializeField] private float maxAngularSpeed = 20f;
        [SerializeField] private float angularResponsiveness = 0.6f;

        [Header("Strength")]
        [SerializeField] private float strength = 10f; 
        
        public float Speed => speed;
        public float SprintMultiplier => sprintMultiplier;
        public float JumpHeight => jumpHeight;
        public float Gravity => gravity;
        public float BaseCarry => baseCarry;
        public float MaxHoldDistance => maxHoldDistance;
        public float HoldSoftZone => holdSoftZone;
        public float ThrowForceGrow => throwForceGrow;
        public float MaxThrowForce => maxThrowForce;
        public float MinChargeTime => minChargeTime;
        public float JointSpring => jointSpring;
        public float JointDamper => jointDamper;
        public float AngularSpring => angularSpring;
        public float AngularDamper => angularDamper;
        public float BaseHoldForce => baseHoldForce;
        public float HoldDamping => holdDamping;
        public float MaxAngularSpeed => maxAngularSpeed;
        public float AngularResponsiveness => angularResponsiveness;
        public float Strength => strength;
    }
}