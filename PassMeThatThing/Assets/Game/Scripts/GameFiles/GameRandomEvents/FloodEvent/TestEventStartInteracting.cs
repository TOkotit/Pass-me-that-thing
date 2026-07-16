using Game.Scripts.Enums;
using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Events.FloodEvent
{
    public class TestEventStartInteracting : NetworkBehaviour, Interactable
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

        public void InteractWithItem(PhysicalItem item)
        {
            
        }

        [Command(requiresAuthority = false)]
        private void CmdTest()
        {
            Debug.Log("CmdTest");
            floodEvent.GameRandomEventManager.ActivateEvent(floodEvent.EventId);
        }

        [Command(requiresAuthority = false)]
        private void CmdTest2()
        {
            GlobalVisionShaderManager.Instance.ToggleAllLampsServerOnly();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            InteractableRegistry.Instance.Register(gameObject, this);
        }
    }
}