using Game.Entity.Stats;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.InteractableObjects.Examples
{
    public class ClassSwitchInteractable : NetworkBehaviour, Interactable
    {
        [SerializeField] private ClassStats newClass;   
        [SerializeField] private bool changeForLocalPlayer = true; 

        public void Interact()
        {
            var player = NetworkClient.localPlayer?.GetComponent<Game.Entity.MainCharacter>();
            if (player)
            {
                CmdChangeClassForPlayer(player);
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdChangeClassForPlayer(Game.Entity.MainCharacter targetPlayer)
        {
            if (!targetPlayer) return;
            targetPlayer.ChangeClass(newClass);
        }

        public void SrbToggle() { }

        public void InteractWithItem(PhysicalItem item) { }

        public void OnStartClient()
        {
            base.OnStartClient();
            InteractableRegistry.Instance.Register(gameObject, this);
        }
    }
}