using Game.Scripts.GameFiles.Items;
using Mirror;

namespace Game.Scripts.GameFiles.InteractableObjects
{
    public abstract class Interactable : NetworkBehaviour
    {
        public abstract void Interact();

        public abstract void SrbToggle();

        public override void OnStartServer()
        {
            base.OnStartServer();
            InteractableRegistry.Instance.Register(gameObject, this);
        }
    }
}