using DG.Tweening;
using Mirror;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class EnemySpider : Enemy
    {
        private EnemyData _spiderData;
        
        public float elapsedAttack;
        public float AttackCooldown => _spiderData.AttackCooldown;
        public float ChaseDistance => _spiderData.ChaseDistance;
        public float AttackDistance => _spiderData.AttackDistance;
        
        public float Speed => _spiderData.Speed;
        public float Damage => _spiderData.Damage;
        
        public SpiderWalk SpiderWalk { get; private set; }
        public SpiderChase SpiderChase { get; private set; }
        public SpiderAttack SpiderAttack { get; private set; }
        public SpiderDeath SpiderDeath { get; private set; }
        
        public SpiderKnockout SpiderKnockout { get; private set; }

        [Inject]
        private void Construct(EnemyDatabase enemyDatabase)
        {
            _spiderData = enemyDatabase.GetEnemy("spider");
        }
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            
            SpiderWalk = new SpiderWalk(this, 
                stateMachine, 
                targetDetector, 
                movementController);
            SpiderChase = new SpiderChase(this, 
                stateMachine, 
                targetDetector, 
                movementController);
            SpiderAttack = new SpiderAttack(this, 
                stateMachine, 
                attackController,
                targetDetector,
                movementController,
                animator);
            SpiderDeath = new SpiderDeath(this, stateMachine);
            
            SpiderKnockout =  new SpiderKnockout(this, stateMachine);
            
            stateMachine.Initialize(SpiderWalk);
        }

        public override void OnDeath()
        {
            base.OnDeath();
            stateMachine.ChangeState(SpiderDeath);
            
        }
        
        private new void Update()
        {
            base.Update();
        }

        private new void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public void SelfDestroy()
        {
            RpcPlayParticles();
        }
        
        [ClientRpc]
        private void RpcPlayParticles()
        {
            particles.Play();
            animator.transform.DOScale(0f, 0.5f)
                .OnComplete((() =>
                {
                    Destroy(gameObject);
                }));
        }
    }
}