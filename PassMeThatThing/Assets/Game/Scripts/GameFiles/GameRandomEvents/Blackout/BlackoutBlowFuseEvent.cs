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
            if (NetworkVisionManager.Instance)
            {
                NetworkVisionManager.Instance.SetAllRoomsPower(false);
                Debug.Log("[PowerOutageEvent] Электричество вырубилось! Лампы погасли.");
            }

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
            if (NetworkVisionManager.Instance)
            {
                NetworkVisionManager.Instance.SetAllRoomsPower(true);
                Debug.Log("[PowerOutageEvent] Электричество восстановлено! Лампы горят.");
            }

            RpcDisableOutline();
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