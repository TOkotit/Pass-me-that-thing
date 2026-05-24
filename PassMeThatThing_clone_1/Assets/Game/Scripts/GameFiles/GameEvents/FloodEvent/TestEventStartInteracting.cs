using Game.Scripts.Enums;
using Game.Scripts.GameFiles.InteractableObjects;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Events.FloodEvent
{
    public class TestEventStartInteracting : NetworkBehaviour, IInteractable
    {
        
        [SerializeField] private FloodEvent floodEvent;

        public void Interact()
        {
            CmdTest();
        }

        public void SrbToggle()
        {
            throw new System.NotImplementedException();
        }
        
        [Command(requiresAuthority = false)]
        private void CmdTest()
        {
            Debug.Log("CmdTest");
            floodEvent.GameEventManager.ActivateEvent(floodEvent.EventId);
        }
    }
}