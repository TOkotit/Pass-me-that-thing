using Ami.BroAudio;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Game.Scripts.GameFiles.Events;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.GameRandomEvents.FloodEvent
{
    public class PumpInteractTerminal : EventTerminal
    {
        [SyncVar]
        public bool _isFixed = true;

        [SerializeField] private SoundSource pipeSound = default;
        [SerializeField] PipeBreakEvent _pipeBreakEvent;
        [SerializeField] ParticleSystem _particleSystem;

        [Server]
        public override void TerminalAct(NetworkConnectionToClient conn)
        {
            base.TerminalAct(conn);
            
            if (_isFixed) return;
            RpcPlayImpactParticles();
            RpcPlayImpactSound();
            CmdFixPipe();
        }
        
        [Command(requiresAuthority = false)]
        private void CmdFixPipe()
        {
            if (_isFixed) return;
            
            _pipeBreakEvent.PlayerFixedPressure();
        }
        
        [ClientRpc]
        private void RpcPlayImpactSound()
        {
            if (pipeSound && !pipeSound.IsPlaying) 
            {
                pipeSound.Play();
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