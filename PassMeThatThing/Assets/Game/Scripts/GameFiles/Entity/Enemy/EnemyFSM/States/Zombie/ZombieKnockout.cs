using System.Collections;
using Game.Scripts.Enums;
using Game.Scripts.Utils;
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

            if (!_zombie.IsFall)
            {
                _zombie.IsFall = true;
                _zombie.RpcFall();
                _zombie.StunChanged(true);
            }

            _zombie.StartCoroutine(Wait());
        }

        private IEnumerator Wait()
        {
            for (var i = 0; i < 1; i++)
            {
                yield return new WaitForSeconds(RandomUtilities.RandNearVal(1f, 0.2f));
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
            _zombie.IsFall = false;
            _zombie.RpcStandUp();
            _zombie.StunChanged(false);
        }
        
    }
}