using Entity.Entity;

namespace Game.Scripts.GameFiles.Entity
{
    // Game/Entity/DamagableImpactReceiver.cs
    using Entity;
    using UnityEngine;

    namespace Game.Entity
    {
        public class DamagableImpactReceiver : MonoBehaviour
        {
            [SerializeField] private float treshold = 2f;
            [SerializeField] private float damageMultiplier = 1f;
            private Damagable _damagable;

            public void SetDamagable(Damagable damagable) => _damagable = damagable;

            private void OnCollisionEnter(Collision collision)
            {
                if (_damagable == null || _damagable.DamagableModel?.HealthPool == null)
                    return;

                float velocity = collision.relativeVelocity.magnitude;
                if (velocity < treshold) return;

                int damage = (int)(damageMultiplier * velocity);
                _damagable.DamagableModel.HealthPool.TakeDamage(damage);
            }
        }
    }
}