using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class DeathState : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Death;
        
        public DeathState(Enemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
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