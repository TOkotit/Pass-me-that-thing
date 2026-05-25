using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class ChaseState : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Walk;
        
        public ChaseState(Enemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }


        
    }
}