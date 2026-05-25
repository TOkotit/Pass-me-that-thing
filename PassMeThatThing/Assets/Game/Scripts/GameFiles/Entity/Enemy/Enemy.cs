using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Scripts.GameFiles.Entity.Enemy
{
    public class Enemy : NetworkBehaviour
    {
        protected EnemyStateMachine stateMachine;
        
        [SerializeField] protected TargetDetector targetDetector;
        [SerializeField] protected EnemyMovementController movementController;
        [SerializeField] protected EnemyAttackController attackController;
        
        protected void Awake()
        {
            stateMachine = new EnemyStateMachine();
            
        }

        protected void Start()
        {
            
        }

        protected void Update()
        {
            stateMachine.CurrentState.LogicUpdate();
        }

        protected void FixedUpdate()
        {
            stateMachine.CurrentState.PhysicsUpdate();
        }
    }
}