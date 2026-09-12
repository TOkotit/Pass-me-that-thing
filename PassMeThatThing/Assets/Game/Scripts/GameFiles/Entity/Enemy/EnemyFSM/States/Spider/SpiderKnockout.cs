using Game.Scripts.Enums;
using Game.Scripts.Utils;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class SpiderKnockout : EnemyState
    {
        
        private EnemySpider _spider;
        
        private float _atkCooldown;
        private bool _isAtkCooldown;
        private EnemyMovementController _movementController;

        private float rTime;
        
        public SpiderKnockout(EnemySpider enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
            _spider = enemy;
            _movementController = enemy.MovementController;
        }

        public override void Enter()
        {
            base.Enter();
            _isAtkCooldown = true;
            _atkCooldown = 0f;

            rTime = RandomUtilities.RandNearMult(_spider.AttackCooldown, 0.1f);

            _spider.SpiderEnemyView.GroupUp();

            //_movementController.Rb.constraints = RigidbodyConstraints.None;
            //_movementController.Rb.AddTorque(new Vector3(1f, 1f, 1f) * 3, ForceMode.VelocityChange);
        }
        
        public override void LogicUpdate()
        {
            
        }

        public override void PhysicsUpdate()
        {
            if (_isAtkCooldown)
            {
                _atkCooldown += Time.fixedDeltaTime;
                if (_atkCooldown >= rTime)
                {
                    _spider.SpiderEnemyView.UnGroup();
                    _isAtkCooldown = false;
                    StateMachine.ChangeState(_spider.SpiderWalk);
                }
            }
        }
        
        public override void Exit()
        {
            _movementController.EnableNavAgent();
            //_movementController.Rb.angularVelocity = Vector3.zero;
            //_movementController.Rb.MoveRotation(Quaternion.identity);
            //_movementController.Rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            base.Exit();
        }
        
    }
}