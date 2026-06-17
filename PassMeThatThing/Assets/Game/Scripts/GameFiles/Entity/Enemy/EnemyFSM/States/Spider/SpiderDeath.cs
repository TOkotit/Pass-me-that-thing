using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class SpiderDeath : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Death;
        
        private EnemySpider _spider;
        
        public SpiderDeath(EnemySpider enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
            _spider = enemy;
        }

        public override void Enter()
        {
            base.Enter();
            
            if (_spider != null)
            {
                _spider.SelfDestroy();
            }
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