using DG.Tweening;
using DI;
using Entity;
using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
using Game.Scripts.GameFiles.Entity.Enemy.View;
using Game.Scripts.GameFiles.Items;

using UnityEngine;
using UnityEngine.AI;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class Enemy : ToughnessDamagable
    {
        [SerializeField] protected TargetDetector targetDetector;
        [SerializeField] protected EnemyMovementController movementController;
        [SerializeField] protected EnemyAttackController attackController;

        [SerializeField] protected EnemyRagdollHandler  ragdollHandler;
        
        
        [Inject] private DamagableRegistry _damagableRegistry;
        private float SMTimer;
        private float SMInterval = 0.1f;
        
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
        
        public override void OnToughnessBreak()
        {
            
        }

        public override void OnToughnessChanged(int currentToughness, int maxToughness)
        {
            
        }
        
        protected virtual void Awake()
        {
            EnemyModel = new EnemyModel();
            _toughnessModel = new ToughnessModel();
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
            if(!IsServerStarted) return;
            stateMachine.CurrentState.LogicUpdate();
        }

        protected void FixedUpdate()
        {
            if(!IsServerStarted) return;
            
            SMTimer += Time.fixedDeltaTime;

            if (SMTimer >= SMInterval)
            {
                stateMachine.CurrentState.PhysicsUpdate();
                SMTimer = 0f;
            }
        }
        #endregion

    }
}