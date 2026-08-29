
using Entity;
using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
using UnityEngine;
using VContainer;


namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class Enemy : ToughnessDamageable
    {
        [SerializeField] protected TargetDetector targetDetector;
        [SerializeField] protected EnemyMovementController movementController;
        [SerializeField] protected EnemyAttackController attackController;
        
        [Inject] private DamagableRegistry _damagableRegistry;

        private float SMLogicTimer;
        private float SMLogicInterval = 0.1f;
        private bool isAlive = true;

        protected EnemyModel EnemyModel;
        protected EnemyStateMachine stateMachine;
        
        public override DamagableModel DamagableModel => EnemyModel;
        

        public TargetDetector TargetDetector => targetDetector;
        public EnemyMovementController MovementController => movementController;
        public EnemyAttackController AttackController => attackController;

        public EnemySpawner EnemySpawner {get; set;}


        public override void OnDeath()
        {
            if (!isServer) return;
            if (!isAlive) return;
            EnemySpawner.EnemyCount--;
            isAlive = false;
        }
        
        public override void OnHealthChanged(int currentHealth, int maxHealth) { }
        
        public override void OnToughnessBreak() { }

        public override void OnToughnessChanged(int currentToughness, int maxToughness) { }
        
        protected virtual void Awake()
        {
            EnemyModel = new EnemyModel();
            ToughnessModel = new ToughnessModel();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            stateMachine = new EnemyStateMachine();
        }

        protected void Update()
        {
            if(!isServer) return;
            
            SMLogicTimer += Time.deltaTime;

            if (SMLogicTimer >= SMLogicInterval)
            {
                stateMachine.CurrentState.LogicUpdate();
                SMLogicTimer = 0f;
            }
        }

        protected void FixedUpdate()
        {
            if(!isServer) return;
            
            stateMachine.CurrentState.PhysicsUpdate();
        }
    }
}