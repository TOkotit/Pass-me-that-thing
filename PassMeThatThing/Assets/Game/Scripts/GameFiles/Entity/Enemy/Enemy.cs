using DG.Tweening;
using DI;
using Entity;
using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
using Game.Scripts.GameFiles.Entity.Enemy.View;
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
        [SerializeField] protected TargetDetector targetDetector;
        [SerializeField] protected EnemyMovementController movementController;
        [SerializeField] protected EnemyAttackController attackController;

        [SerializeField] protected EnemyRagdollHandler  ragdollHandler;
        
        
        [Inject] private DamagableRegistry _damagableRegistry;
        
        protected EnemyModel EnemyModel;
        protected EnemyStateMachine stateMachine;
        
        public override DamagableModel DamagableModel => EnemyModel;

        public TargetDetector TargetDetector => targetDetector;
        public EnemyMovementController MovementController => movementController;
        public EnemyAttackController AttackController => attackController;
        

        public override void OnDeath()
        {
            
        }
        
        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
            
        }
        
        protected virtual void Awake()
        {
            EnemyModel = new EnemyModel();
        }


        #region ServerLogic
        public override void OnStartServer()
        {
            LifetimeScope.Find<GameplayScope>().Container.Inject(this);

            stateMachine = new EnemyStateMachine();
            
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
        #endregion

        
    }
}