using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class WalkState : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Walk;
        
        private TargetDetector _targetDetector;
        
        public WalkState(Enemy enemy, EnemyStateMachine stateMachine, TargetDetector targetDetector) 
            : base(enemy, stateMachine)
        {
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

        }
        
        public override void Exit()
        {
            
        }
        
    }
}