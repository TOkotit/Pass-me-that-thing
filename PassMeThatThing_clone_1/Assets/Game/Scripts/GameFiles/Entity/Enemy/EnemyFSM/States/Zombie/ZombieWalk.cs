using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class ZombieWalk : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Walk;
        
        private EnemyZombie _zombie;
        
        private TargetDetector _targetDetector;
        
        public ZombieWalk(EnemyZombie enemy, 
            EnemyStateMachine stateMachine, 
            TargetDetector targetDetector) 
            : base(enemy, stateMachine)
        {
            _zombie = enemy;
            _targetDetector = targetDetector;
        }

        public override void Enter()
        {
            
        }

        public override void LogicUpdate()
        {
            
        }

        public override void PhysicsUpdate()
        {
            if (_targetDetector.IsTargetVisible)
            {
                if (_targetDetector.DistanceToTarget < _zombie.chaseDistance)
                {
                    StateMachine.ChangeState(_zombie.ZombieChase);
                    return;
                }
            }
            
            
        }
        
        public override void Exit()
        {
            
        }
        
    }
}