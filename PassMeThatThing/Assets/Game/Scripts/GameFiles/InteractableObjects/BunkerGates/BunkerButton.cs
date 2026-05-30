using UnityEngine;

namespace Game.Scripts.GameFiles.InteractableObjects.BunkerGates
{
    public class BunkerButton : Interactable
    {
        [SerializeField] private BunkerGates linkedGate;

        public override void Interact()
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

        public override void SrbToggle()
        {
            if (linkedGate) linkedGate.SrbToggle();
        }
    }
}