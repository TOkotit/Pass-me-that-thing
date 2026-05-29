using Game.Scripts.Enums;
using Mirror.BouncyCastle.Asn1.X509;
using UnityEngine;
using Time = UnityEngine.Time;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class ZombieAttack : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Attack;

        private EnemyZombie _zombie;
        
        private EnemyAttackController _attackController;
        private TargetDetector _targetDetector;
        
        public ZombieAttack(EnemyZombie enemy, 
            EnemyStateMachine stateMachine, 
            EnemyAttackController attackController,
            TargetDetector targetDetector) 
                : base(enemy, stateMachine)
        {
            _attackController = attackController;
            _targetDetector = targetDetector;
            _zombie = enemy;
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

            if (_targetDetector.DistanceToTarget > _zombie.AttackDistance)
            {
                StateMachine.ChangeState(_zombie.ZombieChase);
                return;
            }
            
            _zombie.elapsedAttack += Time.fixedDeltaTime;
            if (_zombie.elapsedAttack >= _zombie.AttackCooldown)
            {
                _attackController.AttackMelee(_targetDetector.DetectedTarget, new Vector3(3f, 3f, 3f));
                _zombie.elapsedAttack = 0f;
            }
        }
        
        public override void Exit()
        {
            
            base.Exit();
        }
        
    }
}