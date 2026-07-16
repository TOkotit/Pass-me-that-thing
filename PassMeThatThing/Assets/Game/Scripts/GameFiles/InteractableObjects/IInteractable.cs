using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;

namespace Game.Scripts.GameFiles.InteractableObjects
{
    public interface Interactable
    {
        public void Interact();
        public void SrbToggle();
        public void InteractWithItem(PhysicalItem item);

        public void OnStartClient();
    }
}