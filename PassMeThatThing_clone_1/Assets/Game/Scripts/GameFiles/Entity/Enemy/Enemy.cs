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
        [Inject]
        private DamagableRegistry _damagableRegistry;
        
        
        public override DamagableModel DamagableModel
        {
            get => EnemyModel;
        }

        public EnemyModel EnemyModel;
        
        protected EnemyStateMachine stateMachine;
        
        [SerializeField] protected TargetDetector targetDetector;
        [SerializeField] protected EnemyMovementController movementController;
        [SerializeField] protected EnemyAttackController attackController;
        
        public override void OnStartClient()
        {
            LifetimeScope.Find<GameplayScope>().Container.Inject(this);

            EnemyModel = new EnemyModel();
            
            _damagableRegistry.Register(this);
        }
        
        protected void Awake()
        {
            stateMachine = new EnemyStateMachine();
            
        }

        

        protected void Start()
        {
            
        }

        protected void Update()
        {
            stateMachine.CurrentState.LogicUpdate();
        }

        protected void FixedUpdate()
        {
            stateMachine.CurrentState.PhysicsUpdate();
        }
    }
}