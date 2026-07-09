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
        
        public float elapsedAttack;
        public float AttackCooldown => _spiderData.AttackCooldown;
        public float ChaseDistance => _spiderData.ChaseDistance;
        public float AttackDistance => _spiderData.AttackDistance;
        
        public float Speed => _spiderData.Speed;
        public float Damage => _spiderData.Damage;
        
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
            // RpcPlayParticles();
        }
        
        // [ClientRpc]
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