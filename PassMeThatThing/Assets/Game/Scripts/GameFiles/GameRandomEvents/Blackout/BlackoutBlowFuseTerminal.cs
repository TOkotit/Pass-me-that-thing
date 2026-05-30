using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Events.Blackout
{
    public class BlackoutBlowFuseTerminal : EventTerminal
    {
        [SyncVar]
        public bool _isFixed = true;
        
        [SerializeField] private BlackoutBlowFuseEvent _powerOutageEvent;
        
        [Server]
        public override void TerminalAct(NetworkConnectionToClient conn)
        {
            base.TerminalAct(conn);
            
            if (_isFixed) return;
            
            FixFuse();
        }
        
        [Server]
        private void FixFuse()
        {
            _isFixed = true;
            
            if (_powerOutageEvent != null)
            {
                _powerOutageEvent.PlayerFixedPower();
            }
        }
    }
}