using Game.Scripts.Enums;
using Mirror.BouncyCastle.Asn1.X509;
using UnityEngine;
using Time = UnityEngine.Time;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class SpiderAttack : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Attack;

        private EnemySpider _spider;
        
        private EnemyAttackController _attackController;
        private TargetDetector _targetDetector;
        private EnemyMovementController _movementController;
        
        private Animator _animator;
        
        public SpiderAttack(EnemySpider enemy, 
            EnemyStateMachine stateMachine, 
            EnemyAttackController attackController,
            TargetDetector targetDetector,
            EnemyMovementController  movementController,
            Animator animator) 
                : base(enemy, stateMachine)
        {
            _attackController = attackController;
            _targetDetector = targetDetector;
            _movementController = movementController;
            
            _animator = animator;
            _spider = enemy;
            
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
                StateMachine.ChangeState(_spider.SpiderWalk);
                return;
            }

            if (_targetDetector.DistanceToTarget > _spider.AttackDistance)
            {
                StateMachine.ChangeState(_spider.SpiderChase);
                return;
            }
            
            _spider.elapsedAttack += Time.fixedDeltaTime;
            if (_spider.elapsedAttack >= _spider.AttackCooldown)
            {
                _movementController.RotateTo(_targetDetector.DetectedTarget.position);
                _attackController.AttackMelee(new Vector3(3f, 3f, 3f), _spider.Damage);
                
                _spider.elapsedAttack = 0f;
            }
        }
        
        public override void Exit()
        {
            
            base.Exit();
        }
        
    }
}