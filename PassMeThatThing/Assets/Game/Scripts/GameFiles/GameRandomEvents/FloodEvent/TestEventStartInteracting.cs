using FishNet.Object;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.InteractableObjects;

using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Events.FloodEvent
{
    public class TestEventStartInteracting : Interactable
    {
        
        [SerializeField] private FloodEvent floodEvent;

        public override void Interact()
        {
            CmdTest();
            CmdTest2();
        }

        public override void SrbToggle()
        {
            throw new System.NotImplementedException();
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void CmdTest()
        {
            Debug.Log("CmdTest");
            floodEvent.GameRandomEventManager.ActivateEvent(floodEvent.EventId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void CmdTest2()
        {
            GlobalVisionShaderManager.Instance.ToggleAllLampsServerOnly();
        }
    }
}