using FishNet.Object;

using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.GameFiles.Events.Blackout
{
    public class BlackoutCutWiresEvent : BaseGameEvent
    {

        [SerializeField] private BlackoutCutWiresTerminal powerTerminal;
        protected override void OnStartEvent()
        {
            // if (GlobalVisionShaderManager.Instance)
            // {
            //     GlobalVisionShaderManager.Instance.ToggleAllLampsServerOnly();
            //     Debug.Log("[PowerOutageEvent] Электричество вырубилось! Лампы погасли.");
            // }
            
            if (powerTerminal) powerTerminal._isFixed.Value = false;
        }
        
        [Server]
        public void PlayerFixedPower() 
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

            GameRandomEventManager.DisableEvent(EventId);
        }
    }
}