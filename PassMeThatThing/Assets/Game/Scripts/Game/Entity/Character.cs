using UnityEngine;
using VContainer;

namespace Entity
{
    public abstract class Character : MonoBehaviour
    {
        [Inject] protected DamagableRegistry Registry { get; private set; }
        public abstract DamagableModel Damagable { get; }

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