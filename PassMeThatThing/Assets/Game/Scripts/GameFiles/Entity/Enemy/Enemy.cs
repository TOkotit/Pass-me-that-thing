using DI;
using Entity;
using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
using Game.Scripts.GameFiles.Items;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class Enemy : Damagable
    {
        [Inject] private DamagableRegistry _damagableRegistry;
        
        public override DamagableModel DamagableModel
        {
            get => EnemyModel;
        }

        public override void OnDeath()
        {
            Destroy(gameObject);
        }

        public override void OnHealthChanged(int diff)
        {
            Debug.Log($"Zombie taken Damage {diff}");
        }


        protected EnemyModel EnemyModel;
        protected EnemyStateMachine stateMachine;
        
        [SerializeField] protected TargetDetector targetDetector;
        [SerializeField] protected EnemyMovementController movementController;
        [SerializeField] protected EnemyAttackController attackController;

        
        
        public override void OnStartServer()
        {
            LifetimeScope.Find<GameplayScope>().Container.Inject(this);

            stateMachine = new EnemyStateMachine();
            EnemyModel = new EnemyModel();
            
            
            
            _damagableRegistry.Register(this);
        }

        protected void Update()
        {
            if(!isServer) return;
            stateMachine.CurrentState.LogicUpdate();
        }

        protected void FixedUpdate()
        {
            if(!isServer) return;
            stateMachine.CurrentState.PhysicsUpdate();
        }
        
        protected virtual void Awake()
        {
            if (EnemyModel == null)
                EnemyModel = new EnemyModel();
        }
    }
}