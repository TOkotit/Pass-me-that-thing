using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;

using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.GameFiles.Events.Blackout
{
    public class BlackoutCutWiresTerminal : EventTerminal
    {
        // [SyncVar]
        public readonly SyncVar<bool> _isFixed = new(true);
        
        [SerializeField] private BlackoutCutWiresEvent _cutWiresEvent;
        [SerializeField] private ParticleSystem _particleSystem;
        
        [Server]
        public override void TerminalAct(NetworkConnection conn)
        {
            base.TerminalAct(conn);
            if (IsTerminalBusy) return;
            if (_isFixed.Value) return;
            
            RpcPlayImpactParticles();
            if (ActivateMinigame(conn, _cutWiresEvent))
            {
                Debug.Log("<color=yellow> [Server] IsTerminalBusy = true");
                IsTerminalBusy = true;
                currentClient = conn;
            }
            
            // FixFuse();
        }
        
        [ServerRpc(RequireOwnership = false)]
        public override void CmdMinigameComplete()
        {
            FixFuse();
        }

        [ServerRpc(RequireOwnership = false)]
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
        
        [ObserversRpc]
        private void RpcPlayImpactParticles()
        {
            if (_particleSystem && !_particleSystem.isPlaying) 
            {
                _particleSystem.Play();
            }
        }
        
        [Server]
        private void FixFuse()
        {
            _isFixed.Value = true;
            
            if (_cutWiresEvent != null)
            {
                _cutWiresEvent.PlayerFixedPower();
            }
        }
    }
}