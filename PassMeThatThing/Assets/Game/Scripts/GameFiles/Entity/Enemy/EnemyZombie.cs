using DG.Tweening;
using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemyZombie : Enemy
    {
        private EnemyData _zombieData;
        
        public float elapsedAttack;
        public float AttackCooldown => _zombieData.AttackCooldown;
        public float ChaseDistance => _zombieData.ChaseDistance;
        public float AttackDistance => _zombieData.AttackDistance;
        
        public float Speed => _zombieData.Speed;
        public float Damage => _zombieData.Damage;
        
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
        }

        public override void OnDeath()
        {
            base.OnDeath();
            stateMachine.ChangeState(ZombieDeath);
            
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