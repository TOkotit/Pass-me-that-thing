using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Entity.Enemy.View;

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
        private EnemyMovementController _movementController;
        
        private EnemyView _enemyView;
        
        public ZombieAttack(EnemyZombie enemy, 
            EnemyStateMachine stateMachine) 
                : base(enemy, stateMachine)
        {
            _zombie = enemy;
            
            _attackController = enemy.AttackController;
            _targetDetector = enemy.TargetDetector;
            _movementController = enemy.MovementController;
            
            _enemyView = enemy.EnemyView;
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
                _movementController.RotateTo(_targetDetector.DetectedTarget);
                _attackController.AttackMelee(new Vector3(10f, 10f, 10f), _zombie.Damage);
                
                var rand =  new System.Random();
                // if (rand.Next(0, 2) == 0)
                // {
                //     _animator.SetTrigger("attack1");
                // }
                // else
                // {
                //     _animator.SetTrigger("attack2");
                // }
                
                _zombie.elapsedAttack = 0f;
            }
        }
        
        public override void Exit()
        {
            
            base.Exit();
        }
        
    }
}