using FishNet.Object;
using Game.Scripts.GameFiles.Items;
using Mirror;

namespace Game.Scripts.GameFiles.InteractableObjects
{
    public abstract class Interactable : NetworkBehaviour
    {
        public abstract void Interact();

        public abstract void SrbToggle();

        public override void OnStartClient()
        {
            base.OnStartClient();
            InteractableRegistry.Instance.Register(gameObject, this);
        }
        
        
    }
}