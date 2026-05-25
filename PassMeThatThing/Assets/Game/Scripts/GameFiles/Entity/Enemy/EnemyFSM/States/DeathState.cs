using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class DeathState : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Walk;
        
        public DeathState(Enemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }


        
    }
}