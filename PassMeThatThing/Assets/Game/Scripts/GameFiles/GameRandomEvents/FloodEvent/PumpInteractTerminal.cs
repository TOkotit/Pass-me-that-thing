using Game.Scripts.GameFiles.Events;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.GameRandomEvents.FloodEvent
{
    public class PumpInteractTerminal : EventTerminal
    {
        
        [SyncVar]
        public bool _isFixed = true;
        
        [SerializeField] PipeBreakEvent _pipeBreakEvent;
        
        [Server]
        public override void TerminalAct(NetworkConnectionToClient conn)
        {
            base.TerminalAct(conn);
            
            if (_isFixed) return;
            CmdFixPipe();
        }
        
        [Command(requiresAuthority = false)]
        private void CmdFixPipe()
        {
            if (_isFixed) return;
            
            _pipeBreakEvent.PlayerFixedPressure();
        }
    }
}