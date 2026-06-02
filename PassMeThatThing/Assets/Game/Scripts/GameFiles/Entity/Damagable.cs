using Mirror;
using UnityEngine;
using VContainer;

namespace Entity
{
    public abstract class Damagable : NetworkBehaviour
    {
        [Inject] protected DamagableRegistry Registry { get; private set; }
        public abstract DamagableModel DamagableModel { get; }
        [SerializeField] protected int health;

        [SyncVar(hook = nameof(OnSyncedHealthChanged))]
        private int _syncedHealth;

        [SyncVar(hook = nameof(OnSyncedMaxHealthChanged))]
        private int _syncedMaxHealth;

        protected virtual void Start()
        {
            if (isServer)
            {
                _syncedHealth = health;
                _syncedMaxHealth = health;
                if (DamagableModel.HealthPool == null)
                    DamagableModel.HealthPool = new HealthPool(health);
            }
            else
            {
                DamagableModel.HealthPool = new HealthPool(health);
            }

            Registry?.Register(this);
        }

        protected virtual void OnDestroy()
        {
            Registry?.Unregister(this);
        }

        [Server]
        public virtual void ServerTakeDamage(int damage)
        {
            if (DamagableModel.HealthPool == null) return;
            int newHealth = DamagableModel.HealthPool.TakeDamage(damage);
            _syncedHealth = newHealth; 
            OnHealthChanged(newHealth);
            if (newHealth <= 0)
                OnDeath();
        }

        [Server]
        public void ServerSetHealth(int newHealth)
        {
            DamagableModel.HealthPool?.SetCurrentHealth(newHealth);
            _syncedHealth = newHealth;
            OnHealthChanged(newHealth);
            if (newHealth <= 0)
                OnDeath();
        }

        private void OnSyncedHealthChanged(int oldHealth, int newHealth)
        {
            DamagableModel.HealthPool?.SetCurrentHealth(newHealth);
            OnHealthChanged(newHealth);
            if (newHealth <= 0)
                OnDeath();
        }

        private void OnSyncedMaxHealthChanged(int oldMax, int newMax)
        {
            DamagableModel.HealthPool?.SetMaxHealth(newMax, false);
        }

        public abstract void OnDeath();
        public abstract void OnHealthChanged(int currentHealth);
    }
}