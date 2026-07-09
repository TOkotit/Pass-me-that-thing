using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class SpiderPrepare : EnemyState
    {
        private EnemySpider _spider;
        
        private TargetDetector _targetDetector;
        private EnemyMovementController  _movementController;

        private Ray _ray;
        private RaycastHit _hit;

        private bool _isGoingUp;
        private Vector3 _snapPosition;
        
        public SpiderPrepare(EnemySpider enemy, EnemyStateMachine stateMachine) 
            : base(enemy, stateMachine)
        {
            _spider = enemy;
            _targetDetector = enemy.TargetDetector;
            _movementController = enemy.MovementController;
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
            if (!_isGoingUp)
            {
                _ray = new Ray(_spider.transform.position, _spider.transform.up);
                if (Physics.Raycast(_ray, out _hit, 30f, _spider.CeilingLayer))
                {
                    _snapPosition = _hit.point;
                    _isGoingUp = true;
                }
            }
            else
            {
                
                
                if (true)
                {
                    _isGoingUp = false;
                }
            }
        }
        
        public override void Exit()
        {
            
            base.Exit();
        }
        
    }
}