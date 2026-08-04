using Ami.BroAudio;
using Game.Scripts.GameFiles.Entity.Buildings.WireSystem;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.GameFiles.Events.Blackout
{
    public class BlackoutCutWiresTerminal : EventTerminal
    {
        [SyncVar(hook = nameof(UpdatePort))]
        public bool _isFixed = true;
        
        [SerializeField] private BlackoutCutWiresEvent _cutWiresEvent;
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private SoundSource electricity = default;

        [SerializeField] private WireNodePort port;
        [SerializeField] public Outline _outline;

        [Server]
        public override void TerminalAct(NetworkConnectionToClient conn)
        {
            base.TerminalAct(conn);
            if (IsTerminalBusy) return;
            if (_isFixed) return;

            RpcPlayImpactSound();
            RpcPlayImpactParticles();
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
        
        [ClientRpc]
        private void RpcPlayImpactParticles()
        {
            if (_particleSystem && !_particleSystem.isPlaying) 
            {
                _particleSystem.Play();
            }
        }
        
        [ClientRpc]
        private void RpcPlayImpactSound()
        {
            if (electricity && !electricity.IsPlaying) 
            {
                electricity.Play();
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
        
        public void UpdatePort(bool oldValue, bool newValue)
        {
            if (isServer)
                port.IsOn = _isFixed;
        }
    }
}