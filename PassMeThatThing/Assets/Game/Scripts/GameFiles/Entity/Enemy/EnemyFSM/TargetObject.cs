using Entity;
using Enums;
using Game.Entity;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Internal;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class TargetObject : MonoBehaviour
    {
        [SerializeField] private Damageable damageable;
        [SerializeField] private int priority;
        [SerializeField] private DamagableType damagableType;

        public Damageable Damageable => damageable;
        
        [Inject] 
        private TargetsRegistry Registry { get; set; }
        
        public int Priority => priority;

        public DamagableType DamagableType => damagableType;

        private void Start()
        {
            Registry?.Register(this);
        }
        
        protected virtual void OnDestroy()
        {
            Registry?.Unregister(this);
        }
        
    }
}