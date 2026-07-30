using Game.Scripts.GameFiles.Events;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.GameRandomEvents.FloodEvent
{
    public class BrokenPumpEvent : BaseGameEvent
    {
        [SerializeField] private float _chanceBoost = 0.5f;
        
        [SyncVar]
        private bool _isPressureCritical = false;
        
        [SerializeField] private Events.FloodEvent.FloodEvent _siblingFloodEvent;
        [SerializeField] private PumpInteractTerminal pumpInteractTerminal;
        private void Awake()
        {
            if(!_siblingFloodEvent)
                _siblingFloodEvent = GetComponent<Events.FloodEvent.FloodEvent>();
        }
        
        protected override void OnStartEvent()
        {
            _isPressureCritical = true;
            pumpInteractTerminal._isFixed = false;
            RpcEnableOutline();
            if (_siblingFloodEvent)
            {
                _siblingFloodEvent.CurrentTriggerChance += _chanceBoost;
                Debug.Log($"[PressureEvent] Давление повышено! Шанс локальной протечки увеличен.");
            }
        }
        
        
        [Server]
        public void PlayerFixedPressure() 
        {
            StopEvent();
        }

        [Server]
        protected override void OnStopEvent()
        {
            _isPressureCritical = false;
            
            if (_siblingFloodEvent)
            {
                _siblingFloodEvent.CurrentTriggerChance -= _chanceBoost;
            }

            RpcDisableOutline();
            GameRandomEventManager.DisableEvent(EventId);
        }
        
        [ClientRpc]
        private void RpcEnableOutline()
        {
            pumpInteractTerminal._outline.enabled = true;

        }

        [ClientRpc]
        private void RpcDisableOutline()
        {
            pumpInteractTerminal._outline.enabled = false;
        }
    }
}