using FishNet.Object;
using FishNet.Object.Synchronizing;
using Game.Scripts.GameFiles.Events;

using UnityEngine;

namespace Game.Scripts.GameFiles.GameRandomEvents.FloodEvent
{
    public class PipeBreakEvent : BaseGameEvent
    {
        [SerializeField] private float _chanceBoost = 0.5f;
        
        // [SyncVar]
        private readonly SyncVar<bool> _isPressureCritical = new(false);
        
        [SerializeField] private Events.FloodEvent.FloodEvent _siblingFloodEvent;
        [SerializeField] private PumpInteractTerminal pumpInteractTerminal;
        private void Awake()
        {
            if(!_siblingFloodEvent)
                _siblingFloodEvent = GetComponent<Events.FloodEvent.FloodEvent>();
        }
        
        protected override void OnStartEvent()
        {
            _isPressureCritical.Value = true;
            pumpInteractTerminal._isFixed.Value = false;
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
            _isPressureCritical.Value = false;
            
            if (_siblingFloodEvent)
            {
                _siblingFloodEvent.CurrentTriggerChance -= _chanceBoost;
            }

            GameRandomEventManager.DisableEvent(EventId);
        }
    }
}