using System.Collections;
using DG.Tweening;
using FishNet.Object;
using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
using Game.Scripts.GameFiles.Entity.Enemy.View;

using Unity.VisualScripting;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemyZombie : Enemy
    {
        [SerializeField] protected ZombieView enemyView;
        
        private EnemyData _zombieData;
        
        private bool hitRagdollCoroutine;
        
        public float elapsedAttack;
        public float AttackCooldown => _zombieData.AttackCooldown;
        public float ChaseDistance => _zombieData.ChaseDistance;
        public float AttackDistance => _zombieData.AttackDistance;
        
        public float Speed => _zombieData.Speed;
        public float Damage => _zombieData.Damage;

        public int MaxHealth => _zombieData.MaxHealth;
        public int MaxToughness => _zombieData.MaxToughness;
        
        
        public ZombieView EnemyView => enemyView;
        
        public ZombieWalk ZombieWalk { get; private set; }
        public ZombieChase ZombieChase { get; private set; }
        public ZombieAttack ZombieAttack { get; private set; }
        public ZombieDeath ZombieDeath { get; private set; }
        
        public ZombieKnockout ZombieKnockout { get; private set; }

        [Inject]
        private void Construct(EnemyDatabase enemyDatabase)
        {
            _zombieData = enemyDatabase.GetEnemy("zombie");
        }

        public new void Awake()
        {
            base.Awake();
        }

        public new void Start()
        {
            base.Start();
            
            EnemyView.Initialize();

            if (IsServerStarted)
            {
                ServerSetMaxHealth(MaxHealth, true);
                ServerSetMaxToughness(MaxToughness, true);
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            ZombieWalk = new ZombieWalk(this, 
                stateMachine);
            ZombieChase = new ZombieChase(this, 
                stateMachine);
            ZombieAttack = new ZombieAttack(this, 
                stateMachine);
            ZombieDeath = new ZombieDeath(this, stateMachine);
            
            ZombieKnockout =  new ZombieKnockout(this, stateMachine);
            
            stateMachine.Initialize(ZombieWalk);
            
            DisableRagdoll();
        }
        
        [ObserversRpc]
        public void RpcFall()
        {
            EnableRagdoll();
        }

        public void EnableRagdoll()
        {
            movementController.DisableNavAgent();
            
            enemyView.DisableAnimator();
            ragdollHandler.EnableRagdoll();
        }
        
        [ObserversRpc]
        public void RpcStandUp()
        {
            enemyView.PlayStandingUp((() => DisableRagdoll()));
            
        }
        public void DisableRagdoll()
        {
            movementController.EnableNavAgent();
            
            ragdollHandler.DisableRagdoll();
            enemyView.EnableAnimator();
        }

        public override void OnDeath()
        {
            base.OnDeath();
            if (!IsServerStarted) return;
            
            stateMachine.ChangeState(ZombieDeath);
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
            if (!IsServerStarted) return;
            
            Debug.Log($"[Zombie] OnHealthChanged {currentHealth}/{maxHealth}");
        }

        public override void OnToughnessChanged(int currentToughness, int maxToughness)
        {
            Debug.Log($"[Zombie] OnToughnessChanged {currentToughness}/{maxToughness}");
        }

        public override void OnToughnessBreak()
        {
            stateMachine.ChangeState(ZombieKnockout);
        }
        
        private new void Update()
        {
            base.Update();
        }

        private new void FixedUpdate()
        {
            base.FixedUpdate();
        }

        [ObserversRpc]
        public void RpcSelfDestroy()
        {
            // RpcPlayParticles();
            if (gameObject != null)
                Destroy(gameObject);
        }
        
        // [ObserversRpc]
        // private void RpcPlayParticles()
        // {
        //     particles.Play();
        //     animator.transform.DOScale(0f, 0.5f)
        //         .OnComplete((() =>
        //         {
        //             Destroy(gameObject);
        //         }));
        // }
    }
}