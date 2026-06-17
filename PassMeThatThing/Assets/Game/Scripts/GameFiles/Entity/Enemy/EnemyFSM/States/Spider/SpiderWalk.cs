using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class SpiderWalk : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Walk;
        
        private EnemySpider _spider;
        
        private TargetDetector _targetDetector;
        private EnemyMovementController  _movementController;
        
        public SpiderWalk(EnemySpider enemy, 
            EnemyStateMachine stateMachine) 
            : base(enemy, stateMachine)
        {
            _spider = enemy;
            _targetDetector = enemy.TargetDetector;
            _movementController = enemy.MovementController;
        }

        public override void Enter()
        {
            base.Enter();
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();
        }

        public override void PhysicsUpdate()
        {
            if (_targetDetector.IsTargetVisible)
            {
                if (_targetDetector.DistanceToTarget < _spider.ChaseDistance)
                {
                    _movementController.StopNavigating();
                    StateMachine.ChangeState(_spider.SpiderChase);
                    return;
                }
                else
                {
                    _movementController.SetSpeed(_spider.Speed / 2);
                    _movementController.NavigateTo(_targetDetector.DetectedTarget);
                
                }
            }
        }
        
        public override void Exit()
        {
            base.Exit();
        }
        
    }
}