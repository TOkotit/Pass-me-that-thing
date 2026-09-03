
using Entity;
using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
using Game.Scripts.GameFiles.Entity.Enemy.View;
using Mirror;
using System;
using UnityEngine;
using VContainer;


namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class Enemy : ToughnessDamageable
    {
        [SerializeField] protected TargetDetector targetDetector;
        [SerializeField] protected EnemyMovementController movementController;
        [SerializeField] protected EnemyAttackController attackController;

        private float SMLogicTimer;
        private float SMLogicInterval = 0.1f;
        private bool isAlive = true;

        protected EnemyModel EnemyModel;
        protected EnemyStateMachine stateMachine;

        public virtual EnemyView EnemyView { get; }
        public virtual EnemyData EnemyData { get; }

        public override DamagableModel DamagableModel => EnemyModel;
        public TargetDetector TargetDetector => targetDetector;
        public EnemyMovementController MovementController => movementController;
        public EnemyAttackController AttackController => attackController;
        public EnemySpawner EnemySpawner { get; set; }

        public virtual float ElapsedAttack { get; set; }
        public virtual float AttackCooldown { get; set; }


        public event Action<int, int> OnEnemyHealthChanged;
        public event Action<int, int> OnEnemyToughnessChanged;
        public event Action<float, float> OnEnemyElapsedAttackChanged;
        public event Action<bool> OnEnemyStunChanged;

        public void EnemyHealthChanged(int currentHealth, int maxHealth) 
            => OnEnemyHealthChanged?.Invoke(currentHealth, maxHealth);
        public void EnemyToughnessChanged(int currentToughness, int maxToughness)
            => OnEnemyToughnessChanged?.Invoke(currentToughness, maxToughness);

        public void EnemyElapsedAttackChanged(float current, float total)
            => OnEnemyElapsedAttackChanged?.Invoke(current, total);

        public void EnemyStunChanged(bool value)
            => OnEnemyStunChanged?.Invoke(value);

        [Server]
        public void SpawnDropItem()
        {
            EnemySpawner.EnemyDropItemSpawner.ServerSpawnItemsFromChanceDict(EnemyData.Drops, transform.position);
        }

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

        public override void OnTakeDamage(int deltaHp)
        {
            EnemyView.TakeDamage();
        }

        protected virtual void Awake()
        {
            EnemyModel = new EnemyModel();
            ToughnessModel = new ToughnessModel();
        }

        protected override void Start()
        {
            base.Start();

            EnemyView.InitUI(this);
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