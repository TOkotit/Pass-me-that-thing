using Game.Scripts.Enums;
using System.Collections;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class ZombieDeath : EnemyState
    {
        private EnemyZombie _zombie;

        public override bool AbleToInterupt => false;

        public ZombieDeath(EnemyZombie enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
            _zombie = enemy;
        }

        public override void Enter()
        {
            base.Enter();

            if (!_zombie.IsFall)
            {
                _zombie.IsFall = true;
                _zombie.RpcFall();
            }
            _zombie.StartCoroutine(Wait());
            _zombie.MovementController.StopNavigating();
        }

        private IEnumerator Wait()
        {
            for (var i = 0; i < 1; i++)
            {
                yield return new WaitForSeconds(2);
            }
            _zombie.SpawnDropItem();
            _zombie.RpcSelfDestroy();
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