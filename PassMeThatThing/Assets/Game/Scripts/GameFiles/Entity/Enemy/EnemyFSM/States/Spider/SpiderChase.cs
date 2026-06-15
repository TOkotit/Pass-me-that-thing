using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class SpiderChase : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Chase;
        
        private EnemySpider _spider;
        
        private TargetDetector _targetDetector;
        private EnemyMovementController  _movementController;
        
        public SpiderChase(EnemySpider enemy, EnemyStateMachine stateMachine, 
            TargetDetector targetDetector,
            EnemyMovementController  movementController) 
            : base(enemy, stateMachine)
        {
            _spider = enemy;
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
                StateMachine.ChangeState(_spider.SpiderWalk);
                return;
            }
            
            if (_targetDetector.DetectedTarget == null) return; 
            
            _movementController.SetSpeed(_spider.Speed);
            _movementController.NavigateTo(_targetDetector.DetectedTarget);
            
            if (_targetDetector.DistanceToTarget < _spider.AttackDistance)
            {
                _movementController.StopNavigating();
                StateMachine.ChangeState(_spider.SpiderAttack);
                return;
            }
            
        }
        
        public override void Exit()
        {
            
            base.Exit();
        }
        
    }
}