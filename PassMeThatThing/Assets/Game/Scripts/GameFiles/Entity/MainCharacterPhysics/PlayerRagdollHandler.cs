using Game.Entity;
using Game.Scripts.GameFiles.Entity.GlobalView;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.MainCharacterPhysics
{
    public class PlayerRagdollHandler : RagdollHandler
    {
        [SerializeField] private GameObject ragdollPrefab;
        [SerializeField] private MainCharacter player;
        private PlayerNetworkRagdoll _ragdollInstance;

        public override void EnableRagdoll()
        {
            if (!_ragdollInstance)
            {
                var go = Instantiate(ragdollPrefab, transform.position, transform.rotation);
                _ragdollInstance = go.GetComponent<PlayerNetworkRagdoll>();
                if (player) _ragdollInstance.Setup(player);
            }
            _ragdollInstance?.EnableRagdoll();
        }

        public override void DisableRagdoll()
        {
            _ragdollInstance?.DisableRagdoll();
        }

        private void DisablePlayer()
        {
            
        }

        private void EnablePlayer()
        {
            
        }
    }
}