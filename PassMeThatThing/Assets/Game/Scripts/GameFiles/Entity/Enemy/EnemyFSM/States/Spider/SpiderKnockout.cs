using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class SpiderKnockout : EnemyState
    {
        
        private EnemySpider _spider;
        
        private float _atkCooldown;
        private bool _isAtkCooldown;
        
        
        public SpiderKnockout(EnemySpider enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            _isAtkCooldown = true;
        }

        public override void LogicUpdate()
        {
            
        }

        public override void PhysicsUpdate()
        {
            if (_isAtkCooldown)
            {
                _atkCooldown += Time.fixedDeltaTime;
                if (_atkCooldown >= _spider.AttackCooldown)
                {
                    _isAtkCooldown = false;
                    StateMachine.ChangeState(_spider.SpiderWalk);
                }
            }
        }
        
        public override void Exit()
        {
            base.Exit();
        }
        
    }
}