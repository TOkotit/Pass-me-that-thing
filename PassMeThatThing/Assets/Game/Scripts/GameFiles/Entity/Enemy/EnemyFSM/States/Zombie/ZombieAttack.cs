using System.Collections;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Entity.Enemy.View;
using Mirror.BouncyCastle.Asn1.X509;
using UnityEngine;
using Time = UnityEngine.Time;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class ZombieAttack : EnemyState
    {

        private EnemyZombie _zombie;
        
        private EnemyAttackController _attackController;
        private TargetDetector _targetDetector;
        private EnemyMovementController _movementController;
        
        private EnemyView _enemyView;
        
        private float _attackProgress;

        private Coroutine _attackCoroutine;
        
        
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
            
            if (_attackCoroutine !=  null) _zombie.StopCoroutine(_attackCoroutine);
            _attackCoroutine = _zombie.StartCoroutine(Attack());
        }

        public override void LogicUpdate()
        {
            
        }

        public override void PhysicsUpdate()
        {
            
        }
        
        public IEnumerator Attack()
        {
            while (_attackProgress < _zombie.AttackCooldown)
            {
                _attackProgress += Time.deltaTime;
                yield return null;
            }
            
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
            
            _attackProgress = 0f;
            StateMachine.ChangeState(_zombie.ZombieChase);
        }
        
        public override void Exit()
        {
            
            base.Exit();
        }
        
    }
}