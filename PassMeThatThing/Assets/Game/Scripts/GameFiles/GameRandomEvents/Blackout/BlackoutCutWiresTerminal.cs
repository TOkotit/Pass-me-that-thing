using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.GameFiles.Events.Blackout
{
    public class BlackoutCutWiresTerminal : EventTerminal
    {
        [SyncVar]
        public bool _isFixed = true;
        
        [SerializeField] private BlackoutCutWiresEvent _cutWiresEvent;
        
        [Server]
        public override void TerminalAct(NetworkConnectionToClient conn)
        {
            base.TerminalAct(conn);
            if (IsTerminalBusy) return;
            if (_isFixed) return;
            
            // RpcPlayImpactParticles();
            if (ActivateMinigame(conn, _cutWiresEvent))
            {
                Debug.Log("<color=yellow> [Server] IsTerminalBusy = true");
                IsTerminalBusy = true;
                currentClient = conn;
            }
            
            // FixFuse();
        }
        
        [Command(requiresAuthority = false)]
        public override void CmdMinigameComplete()
        {
            FixFuse();
        }

        [Command(requiresAuthority = false)]
        public override void CmdMinigameClose()
        {
            IsTerminalBusy = false;
            Debug.Log("<color=yellow> [Server] IsTerminalBusy = false");
            if (currentClient != null)
            {
                CloseMinigame(currentClient);
                currentClient = null;
            }
        }
        
        [Server]
        private void FixFuse()
        {
            _isFixed = true;
            
            if (_cutWiresEvent != null)
            {
                _cutWiresEvent.PlayerFixedPower();
            }
        }
    }
}