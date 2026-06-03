using Entity;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using UnityEngine;

namespace Game.Entity
{
    public class DamagableImpactReceiver : MonoBehaviour
    {
        [SerializeField] private float treshold = 2f;                 // базовый порог для объектов с массой = baseMass
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private float baseMass = 5f;                 // эталонная масса для нормализации
        [SerializeField] private float nonPhysicalMultiplier = 0.3f;  // множитель урона для стен/нефизических объектов
        [SerializeField] private float nonPhysicalTresholdMultiplier = 1f; // множитель порога для стен 
        [SerializeField] private Damagable _damagable;

        public void SetDamagable(Damagable damagable) => _damagable = damagable;

        private void OnCollisionEnter(Collision collision)
        {
            if (!_damagable || _damagable.DamagableModel?.HealthPool == null)
                return;

            float velocity = collision.relativeVelocity.magnitude;

            float massMultiplier;
            float effectiveThreshold;

            if (PhysicalItemRegistry.Instance != null &&
                PhysicalItemRegistry.Instance.TryGetItem(collision.gameObject, out var physicalItem))
            {
                effectiveThreshold = treshold;
                float mass = physicalItem.Rigidbody ? physicalItem.Rigidbody.mass : 1f;
                massMultiplier = mass / baseMass;
            }
            else
            {
                effectiveThreshold = treshold * nonPhysicalTresholdMultiplier;
                massMultiplier = nonPhysicalMultiplier;
            }

            if (velocity < effectiveThreshold)
                return;

            int damage = (int)(velocity * damageMultiplier * massMultiplier);
            _damagable.ServerTakeDamage(damage);
        }
    }
}