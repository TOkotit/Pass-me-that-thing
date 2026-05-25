using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class AttackState : EnemyState
    {
        private EnemyAttackController _attackController;
        private TargetDetector _targetDetector;
        protected override EnemyStates EnemyStateType => EnemyStates.Attack;
        
        public AttackState(Enemy enemy, 
            EnemyStateMachine stateMachine, 
            EnemyAttackController attackController,
            TargetDetector targetDetector) 
                : base(enemy, stateMachine)
        {
            _attackController = attackController;
            _targetDetector = targetDetector;
        }

        public override void Enter()
        {
            base.Enter();
            var a=0;
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