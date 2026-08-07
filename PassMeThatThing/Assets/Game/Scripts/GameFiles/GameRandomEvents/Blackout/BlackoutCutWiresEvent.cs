using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.GameFiles.GameRandomEvents.Blackout
{
    public class BlackoutCutWiresEvent : BaseGameEvent
    {
        [SerializeField] private BlackoutCutWiresTerminal terminal;

        protected override void OnStartEvent()
        {
            if (terminal) 
                terminal.IsFixed = false;

            RpcEnableOutline();
        }
        
        [Server]
        public void FixEvent() 
        {
            GameRandomEventManager.DeactivateEvent(EventId);
        }

        [Server]
        protected override void OnStopEvent()
        {

            RpcDisableOutline();
        }
        
        [ClientRpc]
        private void RpcEnableOutline()
        {
            terminal.Outline.enabled = true;
        }

        [ClientRpc]
        private void RpcDisableOutline()
        {
            terminal.Outline.enabled = false;
        }
    }
}