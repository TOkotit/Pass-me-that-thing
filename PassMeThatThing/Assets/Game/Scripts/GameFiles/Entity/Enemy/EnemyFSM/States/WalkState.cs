using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class WalkState : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Walk;
        
        public WalkState(Enemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }


        
    }
}