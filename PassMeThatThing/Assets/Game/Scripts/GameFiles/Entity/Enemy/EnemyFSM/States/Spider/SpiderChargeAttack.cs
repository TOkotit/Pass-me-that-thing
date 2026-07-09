using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Entity.Enemy.View;
using Mirror.BouncyCastle.Asn1.X509;
using UnityEngine;
using Time = UnityEngine.Time;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class SpiderChargeAttack : EnemyState
    {

        private EnemySpider _spider;
        
        private EnemyAttackController _attackController;
        private TargetDetector _targetDetector;
        private EnemyMovementController _movementController;
        
        private EnemyView _enemyView;
        
        public SpiderChargeAttack(EnemySpider enemy, 
            EnemyStateMachine stateMachine) 
                : base(enemy, stateMachine)
        {
            _attackController = enemy.AttackController;
            _targetDetector = enemy.TargetDetector;
            _movementController = enemy.MovementController;
            
            // _enemyView = enemy.EnemyView;
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
            
        }
        
        public override void Exit()
        {
            
            base.Exit();
        }
        
    }
}