using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class ZombieChase : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Chase;
        
        private EnemyZombie _zombie;
        
        private TargetDetector _targetDetector;
        private EnemyMovementController  _movementController;
        
        public ZombieChase(EnemyZombie enemy, EnemyStateMachine stateMachine, 
            TargetDetector targetDetector,
            EnemyMovementController  movementController) 
            : base(enemy, stateMachine)
        {
            _zombie = enemy;
            _targetDetector = targetDetector;
            _movementController = movementController;
        }

        public override void Enter()
        {
            base.Enter();
            
            
        }

        public override void LogicUpdate()
        {
            
        }

        public override void PhysicsUpdate()
        {
            if (!_targetDetector.IsTargetVisible)
            {
                StateMachine.ChangeState(_zombie.ZombieWalk);
                return;
            }

            _movementController.NavigateTo(_targetDetector.DetectedTarget);
            
            // Debug.Log($"{_targetDetector.DistanceToTarget}");
            if (_targetDetector.DistanceToTarget < _zombie.AttackDistance)
            {
                
                StateMachine.ChangeState(_zombie.ZombieAttack);
                return;
            }
            
        }
        
        public override void Exit()
        {
            
            base.Exit();
        }
        
    }
}