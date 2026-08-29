using DI;
using Enums;
using Game.Scripts.GameFiles.Entity;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Entity
{
    public abstract class Damageable : NetworkBehaviour
    {
        [SerializeField] protected int defaultHealth;
        [SerializeField] protected DamagableType type;
        [SerializeField] protected StatusEffectHandler statusEffectHandler;
        [Inject] protected DamagableRegistry Registry { get; private set; }
        
        [SyncVar(hook = nameof(OnSyncedHealthChanged))]
        protected int _syncedHealth;

        [SyncVar(hook = nameof(OnSyncedMaxHealthChanged))]
        protected int _syncedMaxHealth;
        
        public abstract DamagableModel DamagableModel { get; }
        public StatusEffectHandler StatusEffectHandler => statusEffectHandler;
        
        public DamagableType Type => type;

        protected virtual void Start()
        {
            Debug.LogWarning("Damageable: Start " + gameObject.name);
            if (isServer)
            {
                if (DamagableModel.HealthPool == null)
                    DamagableModel.HealthPool = new HealthPool(defaultHealth);
                DamagableModel.OnHealthChanged += OnHealthChanged;
                DamagableModel.OnDeath += OnDeath;

                DamagableModel.OnDamage += RpcTakeDamage;
                DamagableModel.OnHeal += RpcHeal;
            }
            else
            {
                if (DamagableModel.HealthPool == null)
                    DamagableModel.HealthPool = new HealthPool(defaultHealth);
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

                DamagableModel.OnDamage -= RpcTakeDamage;
                DamagableModel.OnHeal -= RpcHeal;
            }
        }

        [Server]
        public void ServerSetHealth(int newHealth)
        {
            Debug.Log("[DAM] ServerSetHealth");
            DamagableModel.SetHealth(newHealth);
            _syncedHealth = DamagableModel.HealthPool.CurrentHealth;
        }
        
        [Server]
        public void ServerSetMaxHealth(int newHealth, bool fullHeal=false)
        {
            Debug.Log("[DAM] ServerSetMaxHealth");
            DamagableModel.SetMaxHealth(newHealth, fullHeal);
            _syncedMaxHealth = DamagableModel.HealthPool.MaxHealth;
        }

        [Client]
        protected void ClientInitMaxHealth(int newHealth, bool fullHeal = false)
        {
            //Debug.Log("[DAM] ClientInit");

            if (DamagableModel.HealthPool == null)
                DamagableModel.HealthPool = new HealthPool(defaultHealth);

            DamagableModel.SetMaxHealth(newHealth, fullHeal);
            OnHealthChanged(DamagableModel.HealthPool.CurrentHealth,
                    DamagableModel.HealthPool.MaxHealth);
        }

        [Server]
        public virtual void ServerTakeDamage(int damage)
        {
            DamagableModel.TakeDamage(damage);
            _syncedHealth = DamagableModel.HealthPool.CurrentHealth;
        }

        [Server]
        public virtual void ServerHeal(int value)
        {
            DamagableModel.Heal(value);
            _syncedHealth = DamagableModel.HealthPool.CurrentHealth;
        }



        // Хуки 
        private void OnSyncedHealthChanged(int oldHealth, int newHealth)
        {
            if (!isServer) 
            {
                DamagableModel.SetHealth(newHealth);
                
                OnHealthChanged(DamagableModel.HealthPool.CurrentHealth, 
                    DamagableModel.HealthPool.MaxHealth);
                if (newHealth <= 0) OnDeath();
            }
        }

        private void OnSyncedMaxHealthChanged(int oldMax, int newMax)
        {
            if (!isServer)
            {
                DamagableModel.SetMaxHealth(newMax, false);
            }
        }

        public abstract void OnDeath();
        public abstract void OnHealthChanged(int currentHealth, int maxHealth);

        // Доп коллбеки (TODO переделать чтобы все было единообразно)
        [ClientRpc]
        private void RpcTakeDamage(int deltaHp) => OnTakeDamage(deltaHp);
        [ClientRpc]
        private void RpcHeal(int deltaHp) => OnHeal(deltaHp);

        public virtual void OnTakeDamage(int deltaHp) { }
        public virtual void OnHeal(int deltaHp) { }
    }
}