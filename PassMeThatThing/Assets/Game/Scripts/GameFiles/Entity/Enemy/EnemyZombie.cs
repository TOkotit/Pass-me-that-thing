using DG.Tweening;
using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
using Game.Scripts.GameFiles.Entity.Enemy.View;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemyZombie : Enemy
    {
        [SerializeField] protected ZombieView enemyView;
        
        private EnemyData _zombieData;
        
        public float elapsedAttack;
        public float AttackCooldown => _zombieData.AttackCooldown;
        public float ChaseDistance => _zombieData.ChaseDistance;
        public float AttackDistance => _zombieData.AttackDistance;
        
        public float Speed => _zombieData.Speed;
        public float Damage => _zombieData.Damage;
        
        
        public EnemyView EnemyView => enemyView;
        
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
            
            RpcDisableRagdoll();
        }
        
        [ClientRpc]
        public void RpcEnableRagdoll()
        {
            movementController.DisableNavAgent();
            
            enemyView.DisableAnimator();
            ragdollHandler.EnableRagdoll();
        }
        public void RpcDisableRagdoll()
        {
            movementController.EnableNavAgent();
            
            ragdollHandler.DisableRagdoll();
            enemyView.EnableAnimator();
        }

        [ClientRpc]
        public void StandUp()
        {
            enemyView.PlayStandingUp();
            RpcDisableRagdoll();
        }

        public override void OnDeath()
        {
            base.OnDeath();
            if (!isServer) return;
            
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

        [ClientRpc]
        public void RpcSelfDestroy()
        {
            // RpcPlayParticles();
            if (gameObject != null)
                Destroy(gameObject);
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