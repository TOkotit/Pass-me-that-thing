using Ami.BroAudio;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.InteractableObjects.BunkerGates
{
    public class BunkerButton : NetworkBehaviour, Interactable
    {
        [SerializeField] private BunkerGates linkedGate;
        [SerializeField] private SoundSource buttonSound;
        [SerializeField] private SoundSource errorButtonSound;
        
        
        public void Interact()
        {

            if (linkedGate)
            {
                linkedGate.Interact();
                RpcPlayButtonSound();
            }
            else
            {
                RpcPlayButtonErrorSound();
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

        [ClientRpc]
        private void RpcPlayButtonSound()
        {
            if (buttonSound)
            {
                buttonSound.Play();
            }
        }
        
        [ClientRpc]
        private void RpcPlayButtonErrorSound()
        {
            if (errorButtonSound)
            {
                errorButtonSound.Play();
            }
        }
    }
}