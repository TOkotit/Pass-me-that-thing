using Mirror;
using UnityEngine;
using VContainer;

namespace Entity
{
    public abstract class Damagable : NetworkBehaviour
    {
        [Inject] protected DamagableRegistry Registry { get; private set; }
        public abstract DamagableModel DamagableModel { get; }

        protected virtual void Start()
        {
            Registry?.Register(this);
        }

        protected virtual void OnDestroy()
        {
            Registry?.Unregister(this);
        }
    }
}