using Entity;
using Enums;
using Game.Entity;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Internal;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class EnemyTargetObject : MonoBehaviour
    {
        // [SerializeField] private Damagable _damagable;
        [SerializeField] private int _priority;
        [SerializeField] private DamagableType damagableType;
        
        [Inject] 
        private EnemyTargetsRegistry Registry { get; set; }
        
        public int Priority => _priority;

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