using Game.Scripts.Enums;
using System.Collections;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class ZombieDeath : EnemyState
    {
        
        private EnemyZombie _zombie;
        
        public ZombieDeath(EnemyZombie enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
            _zombie = enemy;
        }

        public override void Enter()
        {
            base.Enter();
            
            
            _zombie.RpcFall();
            _zombie.StartCoroutine(Wait());
        }

        private IEnumerator Wait()
        {
            for (var i = 0; i < 1; i++)
            {
                yield return new WaitForSeconds(3);
            }
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