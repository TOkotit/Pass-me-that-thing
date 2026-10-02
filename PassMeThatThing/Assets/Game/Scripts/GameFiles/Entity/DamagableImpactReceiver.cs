using Entity;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using UnityEngine;

namespace Game.Entity
{
    public class DamagableImpactReceiver : MonoBehaviour
    {
        [Header("Threshold")]
        [SerializeField] private float thresholdSpeed = 2f;          // скорость, ниже которой удара нет (м/с)
        [SerializeField] private float baseMass = 5f;                // эталонная масса

        [Header("Damage")]
        [SerializeField] private float damageMultiplier = 1f;        // множитель урона
        [SerializeField] private float baseDamageSpeed = 5f;         // эталонная скорость для нормализации урона
        [SerializeField] private float nonPhysicalMultiplier = 0.3f; // множитель для стен / не-PhysicalItem
        [SerializeField] private float nonPhysicalTresholdMultiplier = 1f;

        [SerializeField] private Damageable damageable;

        public void SetDamagable(Damageable damageable) => this.damageable = damageable;

        private void OnCollisionEnter(Collision collision)
        {
            if (!damageable || damageable.DamagableModel?.HealthPool == null)
                return;

            var velocity = collision.relativeVelocity.magnitude;

            float mass;
            float effectiveThreshold;

            if (PhysicalItemRegistry.Instance != null &&
                PhysicalItemRegistry.Instance.TryGetItem(collision.gameObject, out var physicalItem))
            {
                mass = physicalItem.Rigidbody ? physicalItem.Rigidbody.mass : 1f;
                effectiveThreshold = thresholdSpeed;
            }
            else
            {
                mass = baseMass * nonPhysicalMultiplier;
                effectiveThreshold = thresholdSpeed * nonPhysicalTresholdMultiplier;
            }

            if (velocity < effectiveThreshold)
                return;

            var energy = 0.5f * mass * velocity * velocity;
            var thresholdEnergy = 0.5f * mass * effectiveThreshold * effectiveThreshold;
            var baseEnergy = 0.5f * baseMass * baseDamageSpeed * baseDamageSpeed;
            var excessEnergy = energy - thresholdEnergy;
            if (excessEnergy <= 0f) return;
            var damage = (int)(excessEnergy / baseEnergy * damageMultiplier);

            if (damage <= 0) return;
            damageable.ServerTakeDamage(damage);
        }
    }
}