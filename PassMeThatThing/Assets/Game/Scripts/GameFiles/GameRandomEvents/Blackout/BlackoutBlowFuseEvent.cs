using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.GameFiles.Events.Blackout
{
    public class BlackoutBlowFuseEvent : BaseGameEvent
    {

        [SerializeField] private BlackoutBlowFuseTerminal powerTerminal;
        protected override void OnStartEvent()
        {
            RpcEnableOutline();
            if (GlobalVisionShaderManager.Instance)
            {
                GlobalVisionShaderManager.Instance.SetAllRoomsStateServerOnly(false);
                Debug.Log("[PowerOutageEvent] Электричество вырубилось! Лампы погасли.");
            }

            if (powerTerminal) powerTerminal._isFixed = false;
        }
        
        [Server]
        public void PlayerFixedPower() 
        {
            StopEvent();
        }

        [Server]
        protected override void OnStopEvent()
        {
            if (GlobalVisionShaderManager.Instance)
            {
                GlobalVisionShaderManager.Instance.SetAllRoomsStateServerOnly(true);
                Debug.Log("[PowerOutageEvent] Электричество восстановлено! Лампы горят.");
            }

            RpcDisableOutline();
            GameRandomEventManager.DisableEvent(EventId);
        }
        
        [ClientRpc]
        private void RpcEnableOutline()
        {
            powerTerminal._outline.enabled = true;
        }

        [ClientRpc]
        private void RpcDisableOutline()
        {
            powerTerminal._outline.enabled = false;
        }
    }
}