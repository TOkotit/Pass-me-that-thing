using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class ZombieChase : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Chase;
        
        private EnemyZombie _zombie;
        
        private TargetDetector _targetDetector;
        
        public ZombieChase(Enemy enemy, EnemyStateMachine stateMachine, TargetDetector targetDetector) 
            : base(enemy, stateMachine)
        {
            _targetDetector = targetDetector;
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
            if (!_targetDetector.IsTargetVisible)
            {
                StateMachine.ChangeState(_zombie.ZombieWalk);
                return;
            }

            if (_targetDetector.DistanceToTarget < _zombie.attackDistance)
            {
                StateMachine.ChangeState(_zombie.ZombieAttack);
                return;
            }
            
        }
        
        public override void Exit()
        {
            
            base.Exit();
        }
        
    }
}