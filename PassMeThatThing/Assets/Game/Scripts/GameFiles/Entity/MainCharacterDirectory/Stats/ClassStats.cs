using UnityEngine;

namespace Game.Entity.Stats
{
    [CreateAssetMenu(fileName = "ClassStats", menuName = "Stats/ClassStats")]
    public class ClassStats : ScriptableObject
    {
        [Header("Health")]
        public float maxHealthBonus = 1f;

        [Header("Movement Multipliers")]
        public float speedMultiplier = 1f;
        public float sprintMultiplier = 1f;
        public float jumpMultiplier = 1f;
        public float gravityMultiplier = 1f;

        [Header("Carry & Throw Multipliers")]
        public float baseCarryMultiplier = 1f;
        public float maxHoldDistanceMultiplier = 1f;
        public float holdSoftZoneMultiplier = 1f;
        public float throwForceGrowMultiplier = 1f;
        public float maxThrowForceMultiplier = 1f;
        public float minChargeTimeMultiplier = 1f;

        [Header("Joint Multipliers")]
        public float jointSpringMultiplier = 1f;
        public float jointDamperMultiplier = 1f;
        public float angularSpringMultiplier = 1f;
        public float angularDamperMultiplier = 1f;

        [Header("Hold Force Multipliers")]
        public float baseHoldForceMultiplier = 1f;
        public float holdDampingMultiplier = 1f;
        public float maxAngularSpeedMultiplier = 1f;
        public float angularResponsivenessMultiplier = 1f;

        [Header("Strength Multiplier")]
        public float strengthMultiplier = 1f;
    }
}