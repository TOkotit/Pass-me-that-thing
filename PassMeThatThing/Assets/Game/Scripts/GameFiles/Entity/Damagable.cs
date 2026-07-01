using Enums;
using FishNet.Object;
using FishNet.Object.Synchronizing;

using UnityEngine;
using VContainer;

namespace Entity
{
    /// <summary>
    /// Хост считает хп у себя в DamagableModel обновляет syncHealth
    /// хуки syncHealth обновляют damagableModel на клиентах
    /// 
    /// Хост выполняет OnHealthChanged при срабатываний событий DamagableModel
    /// Клиенты выполняют OnHealthChanged при срабатывании хуков
    /// </summary>
    public abstract class Damagable : NetworkBehaviour
    {
        [SerializeField] protected int defaultHealth;
        [SerializeField] protected DamagableType type;
        
        [Inject] protected DamagableRegistry Registry { get; private set; }
        
        // [SyncVar(OnChange = nameof(OnSyncedHealthChanged))]
        private readonly SyncVar<int> _syncedHealth = new();

        //[SyncVar(OnChange = nameof(OnSyncedMaxHealthChanged))]
        private readonly SyncVar<int> _syncedMaxHealth = new();
        
        public abstract DamagableModel DamagableModel { get; }
        
        public DamagableType Type => type;

        protected virtual void Start()
        {
            _syncedHealth.OnChange += OnSyncedHealthChanged;
            _syncedMaxHealth.OnChange += OnSyncedMaxHealthChanged;
            
            Debug.LogWarning("Damagable: Start " + gameObject.name);
            if (IsServerStarted)
            {
                if (DamagableModel.HealthPool == null)
                    DamagableModel.HealthPool = new HealthPool(defaultHealth);
                DamagableModel.OnHealthChanged += OnHealthChanged;
                DamagableModel.OnDeath += OnDeath;
            }
            else
            {
                DamagableModel.HealthPool = new HealthPool(defaultHealth);
            }

            Registry?.Register(this);
        }

        protected virtual void OnDestroy()
        {
            Registry?.Unregister(this);
            
            _syncedHealth.OnChange -= OnSyncedHealthChanged;
            _syncedMaxHealth.OnChange -= OnSyncedMaxHealthChanged;
            
            if (IsServerStarted)
            {
                DamagableModel.OnHealthChanged -= OnHealthChanged;
                DamagableModel.OnDeath -= OnDeath;
            }
        }

        [Server]
        public void ServerSetHealth(int newHealth)
        {
            DamagableModel.SetHealth(newHealth);
            _syncedHealth.Value = DamagableModel.HealthPool.CurrentHealth;
        }
        
        [Server]
        public void ServerSetMaxHealth(int newHealth, bool fullHeal=false)
        {
            DamagableModel.SetMaxHealth(newHealth, fullHeal);
            _syncedMaxHealth.Value = DamagableModel.HealthPool.MaxHealth;
        }

        [Server]
        public virtual void ServerTakeDamage(int damage)
        {
            DamagableModel.TakeDamage(damage);
            _syncedHealth.Value = DamagableModel.HealthPool.CurrentHealth;
        }

        // Хуки 
        private void OnSyncedHealthChanged(int oldHealth, int newHealth, bool asServer)
        {
            if (!IsServerStarted) 
            {
                DamagableModel.SetHealth(newHealth);
                
                OnHealthChanged(DamagableModel.HealthPool.CurrentHealth, 
                    DamagableModel.HealthPool.MaxHealth);
                if (newHealth <= 0) OnDeath();
            }
        }

        private void OnSyncedMaxHealthChanged(int oldMax, int newMax, bool asServer)
        {
            if (!IsServerStarted)
            {
                DamagableModel.SetMaxHealth(newMax, false);
            }
        }

        public abstract void OnDeath();
        public abstract void OnHealthChanged(int currentHealth, int maxHealth);
    }
}