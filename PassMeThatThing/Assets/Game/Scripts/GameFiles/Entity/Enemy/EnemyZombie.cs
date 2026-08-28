using System;
using System.Collections;
using DG.Tweening;
using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
using Game.Scripts.GameFiles.Entity.Enemy.View;
using Mirror;
using R3;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemyZombie : Enemy
    {
        [SerializeField] protected ZombieView enemyView;
        
        private EnemyData _zombieData;
        //
        // private bool _hitRagdollCoroutine;
        [SyncVar(hook = nameof(OnElapsedChanged))]
        private float _elapsedAttack;

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
        public float ElapsedAttack { get => _elapsedAttack; set => _elapsedAttack = value; }

        public event Action<int, int> OnEnemyHealthChanged;
        public event Action<int, int> OnEnemyToughnessChanged;
        public event Action<float, float> OnEnemyElapsedAttackChanged;
        public event Action<bool> OnEnemyStunChanged;

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
            EnemyView.InitUI(this);

            if (isServer)
            {
                ServerSetMaxHealth(MaxHealth, true);
                ServerSetMaxToughness(MaxToughness, true);
            }
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            
            ZombieWalk = new ZombieWalk(this, stateMachine);
            ZombieChase = new ZombieChase(this, stateMachine);
            ZombieAttack = new ZombieAttack(this, stateMachine);
            ZombieDeath = new ZombieDeath(this, stateMachine);
            ZombieKnockout =  new ZombieKnockout(this, stateMachine);
            
            stateMachine.Initialize(ZombieWalk);
            
            DisableRagdoll();
        }

        public void OnElapsedChanged(float oldElapsed, float newElapsed)
        {
            if (oldElapsed != newElapsed)
                OnEnemyElapsedAttackChanged?.Invoke(newElapsed, AttackCooldown);
        }

        //Damagable callbacks
        public override void OnDeath()
        {
            base.OnDeath();
            if (!isServer) return;
            
            stateMachine.ChangeState(ZombieDeath);
        }

        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
            Debug.Log($"[Zombie] OnHealthChanged {currentHealth}/{maxHealth}");
            OnEnemyHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public override void OnToughnessChanged(int currentToughness, int maxToughness)
        {
            Debug.Log($"[Zombie] OnToughnessChanged {currentToughness}/{maxToughness}");
            OnEnemyToughnessChanged?.Invoke(currentToughness, maxToughness);
        }

        public override void OnToughnessBreak()
        {
            Debug.Log($"[Zombie] OnToughnessBreak");

            stateMachine.ChangeState(ZombieKnockout);
        }

        public void StunChanged(bool value)
        {
            OnEnemyStunChanged?.Invoke(value);
        }
        
        //SM
        private new void Update()
        {
            base.Update();
        }

        private new void FixedUpdate()
        {
            base.FixedUpdate();
        }

        //Destroy
        [ClientRpc]
        public void RpcSelfDestroy()
        {
            enemyView.PlayParticles();
            enemyView.transform.DOScale(0f, 0.5f)
                .OnComplete((() =>
                {
                    if (isServer)   
                    {
                        SelfNetDestroy();
                    }
                }));
        }

        [Server]
        private void SelfNetDestroy()
        {
            if (gameObject != null)
            {
                NetworkServer.Destroy(gameObject);
            }
        }

        //Ragdoll

        [ClientRpc]
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

        [ClientRpc]
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
    }
}