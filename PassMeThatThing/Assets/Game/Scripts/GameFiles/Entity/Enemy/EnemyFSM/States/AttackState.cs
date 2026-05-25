using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class AttackState : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Walk;
        
        public AttackState(Enemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }


        
    }
}