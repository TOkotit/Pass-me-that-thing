using System.Collections;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class ZombieKnockout : EnemyState
    {
        
        private EnemyZombie _zombie;
        
        public ZombieKnockout(EnemyZombie enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
            _zombie = enemy;
        }

        public override void Enter()
        {
            base.Enter();

            _zombie.RpcFall();
            _zombie.StartCoroutine(Wait());

            _zombie.StunChanged(true);
        }

        private IEnumerator Wait()
        {
            for (var i = 0; i < 1; i++)
            {
                yield return new WaitForSeconds(3);
            }
            StateMachine.ChangeState(_zombie.ZombieWalk);
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
            _zombie.ServerFullToughnessRecover();
            _zombie.RpcStandUp();
            _zombie.StunChanged(false);
        }
        
    }
}