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
            // if (GlobalVisionShaderManager.Instance)
            // {
            //     GlobalVisionShaderManager.Instance.ToggleAllLampsServerOnly();
            //     Debug.Log("[PowerOutageEvent] Электричество вырубилось! Лампы погасли.");
            // }
            
            if (terminal) 
                terminal.IsFixed = false;

            RpcEnableOutline();
        }
        
        [Server]
        public void FixEvent() 
        {
            StopEvent();
        }

        [Server]
        protected override void OnStopEvent()
        {
            // if (GlobalVisionShaderManager.Instance)
            // {
            //     GlobalVisionShaderManager.Instance.ToggleAllLampsServerOnly();
            //     Debug.Log("[PowerOutageEvent] Электричество восстановлено! Лампы горят.");
            // }
            RpcDisableOutline();
            GameRandomEventManager.DisableEvent(EventId);
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