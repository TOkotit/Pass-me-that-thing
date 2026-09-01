using DG.Tweening;
using Game.Scripts.GameFiles.Entity.Enemy.View;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class EnemySpider : Enemy
    {
        [SerializeField] protected SpiderView enemyView;
        [SerializeField] protected LayerMask ceilingLayer;
        
        private EnemyData _spiderData;

        [SyncVar(hook = nameof(OnElapsedChanged))]
        private float _elapsedAttack;

        public override EnemyView EnemyView => enemyView;
        public SpiderView SpiderEnemyView => enemyView;

        public override float AttackCooldown => _spiderData.AttackCooldown;
        public override float ElapsedAttack { get => _elapsedAttack; set => _elapsedAttack = value; }
        public float ChaseDistance => _spiderData.ChaseDistance;
        public float AttackDistance => _spiderData.AttackDistance;
        public float Damage => _spiderData.Damage;
        public Vector3 AttackArea => _spiderData.AttackSphereArea;

        public float Speed => _spiderData.Speed;
        public int MaxHealth => _spiderData.MaxHealth;
        public int MaxToughness => _spiderData.MaxToughness;
        
        public SpiderWalk SpiderWalk { get; private set; }
        public SpiderFindPlace SpiderFindPlace { get; private set; }
        public SpiderPrepare SpiderPrepare { get; private set; }
        public SpiderChargeAttack SpiderChargeAttack { get; private set; }
        public SpiderDeath SpiderDeath { get; private set; }
        public SpiderKnockout SpiderKnockout { get; private set; }

        public LayerMask CeilingLayer => ceilingLayer;

        [Inject]
        private void Construct(EnemyDatabase enemyDatabase)
        {
            _spiderData = enemyDatabase.GetEnemy("spider");
        }
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            
            SpiderWalk = new SpiderWalk(this, stateMachine);
            SpiderFindPlace = new SpiderFindPlace(this, stateMachine);
            SpiderPrepare = new SpiderPrepare(this, stateMachine);
            SpiderChargeAttack = new SpiderChargeAttack(this, stateMachine);
            SpiderDeath = new SpiderDeath(this, stateMachine);
            SpiderKnockout =  new SpiderKnockout(this, stateMachine);
            
            stateMachine.Initialize(SpiderWalk);
        }
        protected override void Start()
        {
            base.Start();


            if (isServer)
            {
                ServerSetMaxHealth(MaxHealth, true);
                ServerSetMaxToughness(MaxToughness, true);
            }
            else if (isClient)
            {
                ClientInitMaxHealth(MaxHealth, true);
                ClientInitMaxToughness(MaxToughness, true);
            }
        }
        
        public override void OnHealthChanged(int currentHealth, int maxHealth)
        {
            base.OnHealthChanged(currentHealth, maxHealth);

            Debug.Log($"[Spider] OnHealthChanged {currentHealth}/{maxHealth}");
        }

        public override void OnToughnessChanged(int currentToughness, int maxToughness)
        {
            base.OnToughnessChanged(currentToughness, maxToughness);

            Debug.Log($"[Spider] OnToughnessChanged {currentToughness}/{maxToughness}");
        }

        public override void OnDeath()
        {
            base.OnDeath();

            stateMachine.ChangeState(SpiderDeath);
        }

        public void OnElapsedChanged(float oldElapsed, float newElapsed)
        {
            EnemyElapsedAttackChanged(newElapsed, AttackCooldown);
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
    }
}