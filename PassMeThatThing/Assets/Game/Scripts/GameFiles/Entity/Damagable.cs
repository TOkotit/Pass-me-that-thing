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
            Debug.LogWarning("Damagable: Start " + gameObject.name);
            if (isServer)
            {
                _syncedHealth = health;
                _syncedMaxHealth = health;
                if (DamagableModel.HealthPool == null)
                    DamagableModel.HealthPool = new HealthPool(health);
                DamagableModel.OnHealthChanged += OnHealthChanged;
                DamagableModel.OnDeath += OnDeath;
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
            if (isServer)
            {
                DamagableModel.OnHealthChanged -= OnHealthChanged;
                DamagableModel.OnDeath -= OnDeath;
            }
        }

        [Server]
        public void ServerSetHealth(int newHealth)
        {
            DamagableModel.SetHealth(newHealth);
            _syncedHealth = newHealth;
        }

        [Server]
        public virtual void ServerTakeDamage(int damage)
        {
            DamagableModel.TakeDamage(damage);
            _syncedHealth = DamagableModel.HealthPool.CurrentHealth;
        }

        // Хуки 
        private void OnSyncedHealthChanged(int oldHealth, int newHealth)
        {
            DamagableModel.HealthPool?.SetCurrentHealth(newHealth);
            
            if (!isServer) 
            {
                OnHealthChanged(newHealth);
                if (newHealth <= 0) OnDeath();
            }
        }

        private void OnSyncedMaxHealthChanged(int oldMax, int newMax)
        {
            DamagableModel.HealthPool?.SetMaxHealth(newMax, false);
        }

        public abstract void OnDeath();
        public abstract void OnHealthChanged(int currentHealth);
    }
}