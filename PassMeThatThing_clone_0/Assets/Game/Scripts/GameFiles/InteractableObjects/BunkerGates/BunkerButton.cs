using UnityEngine;

namespace Game.Scripts.GameFiles.InteractableObjects.BunkerGates
{
    public class BunkerButton : MonoBehaviour, IInteractable
    {
        [SerializeField] private BunkerGates linkedGate;

        public void Interact()
        {
            if (linkedGate != null)
            {
                linkedGate.Interact();
            }
        }

        public void Open()
        {
            if (linkedGate != null) linkedGate.Open();
        }

        public void Close()
        {
            if (linkedGate != null) linkedGate.Close();
        }

        public void SrbToggle()
        {
            if (linkedGate != null) linkedGate.SrbToggle();
        }
    }
}