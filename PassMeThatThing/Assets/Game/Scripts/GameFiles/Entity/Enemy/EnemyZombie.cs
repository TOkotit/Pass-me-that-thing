using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemyZombie : Enemy
    {
        public float attackCooldown = 3f;
        public float elapsedAttack;
        
        public float chaseDistance = 3f;

        public float attackDistance = 3f;
        
        public ZombieWalk ZombieWalk { get; private set; }
        public ZombieChase ZombieChase { get; private set; }
        public ZombieAttack ZombieAttack { get; private set; }
        public ZombieDeath ZombieDeath { get; private set; }
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            
            ZombieWalk = new ZombieWalk(this, stateMachine, targetDetector);
            ZombieChase = new ZombieChase(this, 
                stateMachine, 
                targetDetector, 
                movementController);
            ZombieAttack = new ZombieAttack(this, 
                stateMachine, 
                attackController,
                targetDetector);
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