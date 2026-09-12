using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class SpiderWalk : EnemyState
    {
        
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

            _spider.SpiderEnemyView.SetWalk(true);

            _movementController.EnableNavAgent();
            _movementController.SetSpeed(_spider.Speed);
        }

        public override void LogicUpdate()
        {
            if (_targetDetector.IsTargetVisible)
            {
                if (_targetDetector.DistanceToTarget <= _spider.ChaseDistance)
                {
                    _movementController.StopNavigating();
                    StateMachine.ChangeState(_spider.SpiderPrepare);
                    return;
                }
                else
                {
                    _movementController.NavigateTo(_targetDetector.DetectedTarget);
                }
            }
            else
            {
                StateMachine.ChangeState(_spider.SpiderWander);
            }
        }

        public override void PhysicsUpdate()
        {
            
        }
        
        public override void Exit()
        {

            _spider.SpiderEnemyView.SetWalk(false);

            base.Exit();
        }
        
    }
}