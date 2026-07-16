using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.InteractableObjects.BunkerGates
{
    public class BunkerButton : NetworkBehaviour, Interactable
    {
        [SerializeField] private BunkerGates linkedGate;

        public void Interact()
        {
            if (linkedGate)
            {
                linkedGate.Interact();
            }
        }

        public void Open()
        {
            if (linkedGate) linkedGate.Open();
        }

        public void Close()
        {
            if (linkedGate) linkedGate.Close();
        }

        public void SrbToggle()
        {
            if (linkedGate) linkedGate.SrbToggle();
        }

        public void InteractWithItem(PhysicalItem item)
        {
            
        }


        public override void OnStartClient()
        {
            base.OnStartClient();
            InteractableRegistry.Instance.Register(gameObject, this);
        }
    }
}