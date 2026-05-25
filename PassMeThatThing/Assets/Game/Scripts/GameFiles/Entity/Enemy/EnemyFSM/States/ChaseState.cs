using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class ChaseState : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Chase;
        
        private TargetDetector _targetDetector;
        
        public ChaseState(Enemy enemy, EnemyStateMachine stateMachine, TargetDetector targetDetector) 
            : base(enemy, stateMachine)
        {
            _targetDetector = targetDetector;
        }


        
    }
}