using Game.Scripts.GameFiles.GameRandomEvents;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.GameRandomEvents.Flood
{
    public class BrokenPumpEvent : BaseGameEvent
    {
        [SerializeField] private float _chanceBoost = 0.5f;
        [SerializeField] private PumpInteractTerminal pumpInteractTerminal;
        
        protected override void OnStartEvent()
        {
            //GameRandomEventManager.PipebreakChanceBoost = _chanceBoost;


            if (pumpInteractTerminal)
                pumpInteractTerminal.IsFixed = false;

            RpcEnableOutline();
        }

        [Server]
        protected override void OnTaskTimerEnd(int taskId)
        {
            GameRandomEventManager.PipebreakChanceBoost = _chanceBoost;
        }


        [Server]
        public void FixEvent() 
        {
            GameRandomEventManager.DeactivateEvent(EventId);
        }

        [Server]
        protected override void OnStopEvent()
        {
            GameRandomEventManager.PipebreakChanceBoost = 0f;

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