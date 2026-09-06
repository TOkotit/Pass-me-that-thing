using System.Collections;
using System.Collections.Generic;
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

        private float _timeToGoUp = 0.5f;
        private bool _isGoingUp;
        private bool _isUp;

        private Vector3 _positionStart;
        private Vector3 _positionEnd;
        private Quaternion _rotationStart;
        private Quaternion _rotationEnd;
        private float _progress;
        private Coroutine _goUpCor;
        

        
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
            
            _isGoingUp = false;
            _isUp =  false;
            
            _movementController.DisableNavAgent();
        }

        public override void LogicUpdate()
        {
            if (!_isUp && !_isGoingUp)
            {
                if (!_targetDetector.IsTargetVisible
                    || _targetDetector.DistanceToTarget > _spider.ChaseDistance)
                {
                    if (_goUpCor != null) _spider.StopCoroutine(_goUpCor);
                    StateMachine.ChangeState(_spider.SpiderWalk);
                    return;
                }
            }
            
            if (_isUp)
            {
                if (_targetDetector.IsTargetVisible)
                {
                    StateMachine.ChangeState(_spider.SpiderChargeAttack);
                }
            }
            else if (!_isGoingUp)
            {
                _ray = new Ray(_spider.transform.position, _spider.transform.up);
                if (Physics.Raycast(_ray, out _hit, float.MaxValue, _spider.CeilingLayer))
                {
                    _positionStart =  _spider.transform.position;
                    _positionEnd = _hit.point - _spider.transform.up * 2f;
                    _rotationStart = _spider.transform.rotation;
                    _rotationEnd = _spider.transform.rotation * Quaternion.Euler(0, 0, 180);
                    
                    if (_goUpCor != null) _spider.StopCoroutine(_goUpCor);
                    _goUpCor = _spider.StartCoroutine(GoUp());
                }
            }
            
            
        }

        public override void PhysicsUpdate()
        {

        }

        public IEnumerator GoUp()
        {
            _movementController.Rb.useGravity = false;
            _movementController.Rb.isKinematic = true;
            _movementController.Rb.linearVelocity = Vector3.zero;
            _progress = 0f;
            _isGoingUp = true;
            
            while (_progress < _timeToGoUp)
            {
                _progress += Time.deltaTime;
                var progressInPercantage = _progress / _timeToGoUp;

                _movementController.Rb.MovePosition(Vector3.Lerp(_positionStart, _positionEnd, progressInPercantage));
                _movementController.Rb.MoveRotation(Quaternion.Slerp(_rotationStart, _rotationEnd, progressInPercantage));


                //_spider.transform.position = Vector3.Lerp(_positionStart, _positionEnd, progressInPercantage);
                //_spider.transform.rotation = Quaternion.Slerp(_rotationStart, _rotationEnd, progressInPercantage);
                yield return null;
            }
            _isGoingUp = false;
            _isUp = true;
            
        }
        
        
        public override void Exit()
        {
            
            base.Exit();
        }
        
    }
}