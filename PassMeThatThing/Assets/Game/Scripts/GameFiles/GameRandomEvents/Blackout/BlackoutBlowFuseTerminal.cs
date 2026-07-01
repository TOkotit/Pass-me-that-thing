using FishNet.Object;
using FishNet.Object.Synchronizing;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Events.Blackout
{
    public class BlackoutBlowFuseTerminal : EventTerminal
    {
        [SyncVar]
        public bool _isFixed = true;
        
        [SerializeField] private BlackoutBlowFuseEvent _powerOutageEvent;
        [SerializeField] private ParticleSystem _particleSystem;
        [Server]
        public override void TerminalAct(NetworkConnectionToClient conn)
        {
            base.TerminalAct(conn);
            if (IsTerminalBusy) return;
            if (_isFixed) return;
            
            RpcPlayImpactParticles();
            if (ActivateMinigame(conn, _powerOutageEvent))
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
            
            if (_powerOutageEvent != null)
            {
                _powerOutageEvent.PlayerFixedPower();
            }
        }
        [ClientRpc]
        private void RpcPlayImpactParticles()
        {
            if (_particleSystem && !_particleSystem.isPlaying) 
            {
                _particleSystem.Play();
            }
        }
    }
}