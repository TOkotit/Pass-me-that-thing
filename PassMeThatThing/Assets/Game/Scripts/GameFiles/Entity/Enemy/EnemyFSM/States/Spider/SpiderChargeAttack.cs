using System.Collections;
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

        private float _progress;
        private float _timeToCharge = 1f;
        private float _dashProgress;
        private float _timeToDash = 0.5f;
        
        private Vector3 _positionStart;
        private Vector3 _positionEnd;
        private Quaternion _rotationStart;
        private Quaternion _rotationEnd;
        
        private Coroutine _dashCoroutine;
        private Coroutine _waitCoroutine;
        
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
            
            _movementController.DisableNavAgent();
            
            
            _positionStart = _spider.transform.position;
            _rotationStart = _spider.transform.rotation;
            _rotationEnd = _spider.transform.rotation * Quaternion.Euler(0, 0, 180);
            
            if (_targetDetector.IsTargetVisible)
            {
                if (_waitCoroutine!=null) _spider.StopCoroutine(_waitCoroutine);
                _waitCoroutine = _spider.StartCoroutine(WaitFor());
            }
            else
            {
                StateMachine.ChangeState(_spider.SpiderWalk);
            }
        }

        public override void LogicUpdate()
        {
            
        }

        public override void PhysicsUpdate()
        {

        }

        public IEnumerator WaitFor()
        {
            while (_progress < _timeToCharge)
            {
                _progress += Time.deltaTime;
                yield return null;
            }
            
            if (_dashCoroutine!=null) _spider.StopCoroutine(_dashCoroutine);
            _dashCoroutine = _spider.StartCoroutine(DashAttack());
            _progress = 0f;
        }
        
        public IEnumerator DashAttack()
        {
            _positionEnd = _targetDetector.DetectedTarget;
            
            while (_dashProgress < _timeToDash)
            {
                _dashProgress += Time.deltaTime;
                var progressInPercantage = _dashProgress / _timeToDash;

                _spider.transform.position = Vector3.Lerp(_positionStart, _positionEnd, progressInPercantage);
                _spider.transform.rotation = Quaternion.Slerp(_rotationStart, _rotationEnd, progressInPercantage);
                yield return null;
            }

            _dashProgress = 0f;
            StateMachine.ChangeState(_spider.SpiderWalk);
        }
        
        public override void Exit()
        {
            base.Exit();
        }
        
    }
}