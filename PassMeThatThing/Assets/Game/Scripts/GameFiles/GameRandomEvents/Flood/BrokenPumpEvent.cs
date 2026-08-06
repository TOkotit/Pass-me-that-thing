using Game.Scripts.GameFiles.GameRandomEvents;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.GameRandomEvents.Flood
{
    public class BrokenPumpEvent : BaseGameEvent
    {
        [SerializeField] private float _chanceBoost = 0.5f;
        
        //[SerializeField] private Events.FloodEvent.FloodEvent _siblingFloodEvent;

        [SerializeField] private PumpInteractTerminal pumpInteractTerminal;

        private void Awake()
        {
            //if (!_siblingFloodEvent)
            //    _siblingFloodEvent = GetComponent<Events.FloodEvent.FloodEvent>();
        }
        
        protected override void OnStartEvent()
        {
            if (pumpInteractTerminal)
                pumpInteractTerminal.IsFixed = false;

            RpcEnableOutline();

            //if (_siblingFloodEvent)
            //{
            //    _siblingFloodEvent.CurrentTriggerChance += _chanceBoost;
            //    Debug.Log($"[PressureEvent] Давление повышено! Шанс локальной протечки увеличен.");
            //}
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

        //View
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