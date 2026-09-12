using Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM;
using Game.Scripts.GameFiles.Entity.Enemy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AI;
using UnityEngine;
using Game.Scripts.Utils;

namespace Assets.Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM.States.Spider
{
    public class SpiderWander : EnemyState
    {
        private EnemySpider _spider;

        private TargetDetector _targetDetector;
        private EnemyMovementController _movementController;

        private float _timeRanMult = 0.2f;

        private bool _isWandering;
        private float _waitProgress;
        private float _rWaitTime;
        private float _wanderProgress;
        private float _rWanderTime;

        public SpiderWander(EnemySpider enemy,
            EnemyStateMachine stateMachine)
            : base(enemy, stateMachine)
        {
            _spider = enemy;
            _targetDetector = enemy.TargetDetector;
            _movementController = enemy.MovementController;
        }

        public override void Enter()
        {
            base.Enter();

            _movementController.SetSpeed(_spider.Speed * _spider.WanderSpeedMult);
        }

        public override void LogicUpdate()
        {
            if (_targetDetector.IsTargetVisible)
            {
                StateMachine.ChangeState(_spider.SpiderWalk);
            }
        }

        public override void PhysicsUpdate()
        {
            if (_isWandering)
            {
                _wanderProgress += Time.fixedDeltaTime;
                if (_wanderProgress >= _rWanderTime)
                {
                    //начало ожидания
                    _wanderProgress = 0;
                    _isWandering = false;
                    _rWaitTime = RandomUtilities.RandNearMult(_spider.WaitTime, _timeRanMult);
                    _movementController.StopNavigating();

                    _spider.SpiderEnemyView.SetWalk(false);
                }
            }
            else
            {
                _waitProgress += Time.fixedDeltaTime;
                if (_waitProgress >= _rWaitTime)
                {
                    //начало брожения
                    _waitProgress = 0;
                    _isWandering = true;
                    _rWanderTime = RandomUtilities.RandNearMult(_spider.WanderTime, _timeRanMult);
                    SetNewRandomDestination();

                    _spider.SpiderEnemyView.SetWalk(true);
                }
            }
        }

        private void SetNewRandomDestination()
        {
            var randomPoint = RandomNavSphere(_spider.transform.position,
                _targetDetector.SightDistance, -1);
            _movementController.NavigateTo(randomPoint);
        }

        private Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
        {
            var randomDirection = UnityEngine.Random.insideUnitSphere * dist;
            randomDirection += origin;

            NavMesh.SamplePosition(randomDirection, out var navHit, dist, layermask);

            return navHit.position;
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
