using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class SpiderKnockout : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Knockout;
        
        private EnemySpider _spider;
        
        public SpiderKnockout(EnemySpider enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

        }

        public override void LogicUpdate()
        {
            
        }

        public override void PhysicsUpdate()
        {

        }
        
        public override void Exit()
        {
            base.Exit();
        }
        
    }
}