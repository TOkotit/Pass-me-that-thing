using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
using Unity.VisualScripting;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class EnemyZombie : Enemy
    {
        public WalkState PatrolState { get; private set; }
        public ChaseState ChaseState { get; private set; }
        public AttackState AttackState { get; private set; }
        public DeathState DeathState { get; private set; }

        private new void Awake()
        {
            base.Awake();
            
            PatrolState = new WalkState(this, stateMachine, targetDetector);
            ChaseState = new ChaseState(this, stateMachine, targetDetector);
            AttackState = new AttackState(this, 
                stateMachine, 
                attackController,
                targetDetector);
            DeathState = new DeathState(this, stateMachine);
        }
        
        private new void Start()
        {
            base.Start();
            
            stateMachine.Initialize(PatrolState);
            
            
        }
        
        private new void Update()
        {
            base.Update();
        }

        private new void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }
}