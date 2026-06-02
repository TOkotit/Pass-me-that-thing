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

        protected virtual void Start()
        {
            Registry?.Register(this);
            DamagableModel.HealthPool.OnDeath += () => OnDeath();
            DamagableModel.HealthPool.OnHealthChanged += OnHealthChanged;
        }

        protected virtual void OnDestroy()
        {
            Registry?.Unregister(this); 
        } 
        
        abstract public void OnDeath();
        abstract public void OnHealthChanged(int diff);
    }
}
