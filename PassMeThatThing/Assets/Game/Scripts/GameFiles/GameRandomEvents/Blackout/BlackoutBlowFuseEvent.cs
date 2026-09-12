using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.GameFiles.GameRandomEvents.Blackout
{
    public class BlackoutBlowFuseEvent : BaseGameEvent
    {

        [SerializeField] private BlackoutBlowFuseTerminal terminal;

        protected override void OnStartEvent()
        {
            if (terminal) 
                terminal.IsFixed = false;

            RpcEnableOutline();
            
            NetworkVisionManager.Instance.SetGlobalPower(false);
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
            NetworkVisionManager.Instance.SetGlobalPower(true);
        }

        //View
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