using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class ZombieWalk : EnemyState
    {
        
        private EnemyZombie _zombie;
        
        private TargetDetector _targetDetector;
        private EnemyMovementController  _movementController;
        
        public ZombieWalk(EnemyZombie enemy, 
            EnemyStateMachine stateMachine) 
            : base(enemy, stateMachine)
        {
            _zombie = enemy;
            _targetDetector = enemy.TargetDetector;
            _movementController = enemy.MovementController;
        }

        public override void Enter()
        {
            base.Enter();
            
            _movementController.SetSpeed(_zombie.Speed);

            _zombie.ZombieEnemyView.SetWalk(true);
        }

        public override void LogicUpdate()
        {
            if (_targetDetector.IsTargetVisible)
            {
                if (_targetDetector.DistanceToTarget < _zombie.ChaseDistance)
                {
                    _movementController.StopNavigating();
                    StateMachine.ChangeState(_zombie.ZombieChase);
                    return;
                }
                else
                {
                    _movementController.NavigateTo(_targetDetector.DetectedTarget);
                }
            }
            else
            {
                StateMachine.ChangeState(_zombie.ZombieWander);
            }
        }

        public override void PhysicsUpdate()
        {
            
        }
        
        public override void Exit()
        {
            _zombie.ZombieEnemyView.SetWalk(false);
            base.Exit();
        }
        
    }
}