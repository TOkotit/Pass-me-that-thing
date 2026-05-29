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
            CmdTest2();
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

        [Command(requiresAuthority = false)]
        private void CmdTest2()
        {
            GlobalVisionManager.Instance.ToggleAllLampsServerOnly();
        }
    }
}