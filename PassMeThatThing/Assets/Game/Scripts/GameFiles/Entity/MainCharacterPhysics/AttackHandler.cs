using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.MainCharacterPhysics
{
    public class AttackHandler : NetworkBehaviour
    {
        [SerializeField] Animator animator;
        [Command]
        public void CmdPerformAttack()
        {
            
        }

        private void RpcAttack()
        {
            
        }
    }
}