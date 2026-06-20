using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class ZombieKnockout : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Knockout;
        
        private EnemyZombie _zombie;
        
        public ZombieKnockout(EnemyZombie enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
            _zombie = enemy;
        }

        public override void Enter()
        {
            base.Enter();

            _zombie.EnableRagdoll();
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
            _zombie.DisableRagdoll();
        }
        
    }
}