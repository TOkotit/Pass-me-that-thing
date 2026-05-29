using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class ZombieDeath : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Death;
        
        private EnemyZombie _zombie;
        
        public ZombieDeath(Enemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            
            if (_zombie != null)
            {
                _zombie.SelfDestroy();
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