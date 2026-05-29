using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
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

        [Inject]
        private void Construct(EnemyDatabase enemyDatabase)
        {
            _zombieData = enemyDatabase.GetEnemy("Zombie");
        }
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            
            ZombieWalk = new ZombieWalk(this, 
                stateMachine, 
                targetDetector, 
                movementController);
            ZombieChase = new ZombieChase(this, 
                stateMachine, 
                targetDetector, 
                movementController);
            ZombieAttack = new ZombieAttack(this, 
                stateMachine, 
                attackController,
                targetDetector,
                movementController);
            ZombieDeath = new ZombieDeath(this, stateMachine);
            
            stateMachine.Initialize(ZombieWalk);
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
            Destroy(gameObject);
        }
    }
}