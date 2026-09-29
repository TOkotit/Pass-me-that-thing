using System;
using DI;
using Enums;
using Game.Scripts.GameFiles.Entity;
using Mirror;
using UnityEngine;
using VContainer;

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
        
        public event Action<int, int> HealthChanged;

        public abstract DamagableModel DamagableModel { get; }
        public StatusEffectHandler StatusEffectHandler => statusEffectHandler;

        public DamagableType Type => type;

        protected virtual void Start()
        {
            Debug.LogWarning("Damageable: Start " + gameObject.name);

            if (DamagableModel.HealthPool == null)
                DamagableModel.HealthPool = new HealthPool(defaultHealth);

            if (isServer)
            {
                DamagableModel.OnHealthChanged += OnHealthChanged;
                DamagableModel.OnDeath += OnDeath;

                DamagableModel.OnDamage += RpcTakeDamage;
                DamagableModel.OnHeal += RpcHeal;
            }

            Registry?.Register(this);

            RaiseHealthChanged();
        }

        protected virtual void OnDestroy()
        {
            Registry?.Unregister(this);

            if (isServer && DamagableModel != null)
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
            Debug.Log("damageable: ServerSetHealth");
            DamagableModel.SetHealth(newHealth);
            _syncedHealth = DamagableModel.HealthPool.CurrentHealth;
            RaiseHealthChanged();
        }

        [Server]
        public void ServerSetMaxHealth(int newHealth, bool fullHeal = false)
        {
            Debug.Log("damageable: ServerSetMaxHealth");
            DamagableModel.SetMaxHealth(newHealth, fullHeal);
            _syncedMaxHealth = DamagableModel.HealthPool.MaxHealth;
            RaiseHealthChanged();
        }

        [Client]
        protected void ClientInitMaxHealth(int newHealth, bool fullHeal = false)
        {
            if (DamagableModel.HealthPool == null)
                DamagableModel.HealthPool = new HealthPool(defaultHealth);

            DamagableModel.SetMaxHealth(newHealth, fullHeal);
            OnHealthChanged(DamagableModel.HealthPool.CurrentHealth,
                            DamagableModel.HealthPool.MaxHealth);
            RaiseHealthChanged();
        }

        [Server]
        public virtual void ServerTakeDamage(int damage)
        {
            DamagableModel.TakeDamage(damage);
            _syncedHealth = DamagableModel.HealthPool.CurrentHealth;
            RaiseHealthChanged();
        }

        [Server]
        public virtual void ServerHeal(int value)
        {
            DamagableModel.Heal(value);
            _syncedHealth = DamagableModel.HealthPool.CurrentHealth;
            RaiseHealthChanged();
        }
        
        private void OnSyncedHealthChanged(int oldHealth, int newHealth)
        {
            if (!isServer)
            {
                DamagableModel.SetHealth(newHealth);

                OnHealthChanged(DamagableModel.HealthPool.CurrentHealth,
                                DamagableModel.HealthPool.MaxHealth);

                if (newHealth <= 0) OnDeath();
            }

            RaiseHealthChanged();
        }

        private void OnSyncedMaxHealthChanged(int oldMax, int newMax)
        {
            if (!isServer)
            {
                DamagableModel.SetMaxHealth(newMax, false);
            }

            RaiseHealthChanged();
        }
        
        protected void RaiseHealthChanged()
        {
            if (DamagableModel?.HealthPool == null) return;

            HealthChanged?.Invoke(DamagableModel.HealthPool.CurrentHealth,
                                  DamagableModel.HealthPool.MaxHealth);
        }
        
        public abstract void OnDeath();
        public abstract void OnHealthChanged(int currentHealth, int maxHealth);

        [ClientRpc]
        private void RpcTakeDamage(int deltaHp) => OnTakeDamage(deltaHp);

        [ClientRpc]
        private void RpcHeal(int deltaHp) => OnHeal(deltaHp);

        public virtual void OnTakeDamage(int deltaHp) { }
        public virtual void OnHeal(int deltaHp) { }
    }
}