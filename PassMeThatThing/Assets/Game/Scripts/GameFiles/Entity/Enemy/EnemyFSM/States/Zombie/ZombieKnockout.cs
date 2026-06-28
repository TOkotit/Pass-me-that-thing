using System.Collections;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Enemy.EnemyFSM
{
    public class ZombieKnockout : EnemyState
    {
        protected override EnemyStates EnemyStateType => EnemyStates.Knockout;
        
        private EnemyZombie _zombie;
        
        public ZombieKnockout(EnemyZombie enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
            _zombie = enemy;
        }

        public override void Enter()
        {
            base.Enter();

            _zombie.RpcEnableRagdoll();
            _zombie.StartCoroutine(Wait());
        }

        private IEnumerator Wait()
        {
            for (var i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(1);
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
            _zombie.StandUp();
        }
        
    }
}