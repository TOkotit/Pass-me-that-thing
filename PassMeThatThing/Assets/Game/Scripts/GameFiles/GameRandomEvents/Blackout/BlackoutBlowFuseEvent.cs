using FishNet.Object;

using UnityEngine;

namespace Game.Scripts.GameFiles.Events.Blackout
{
    public class BlackoutBlowFuseEvent : BaseGameEvent
    {

        [SerializeField] private BlackoutBlowFuseTerminal _powerTerminal;
        protected override void OnStartEvent()
        {
            if (GlobalVisionShaderManager.Instance)
            {
                GlobalVisionShaderManager.Instance.ToggleAllLampsServerOnly();
                Debug.Log("[PowerOutageEvent] Электричество вырубилось! Лампы погасли.");
            }
            
            if (_powerTerminal) _powerTerminal._isFixed.Value = false;
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
                GlobalVisionShaderManager.Instance.ToggleAllLampsServerOnly();
                Debug.Log("[PowerOutageEvent] Электричество восстановлено! Лампы горят.");
            }

            GameRandomEventManager.DisableEvent(EventId);
        }
    }
}