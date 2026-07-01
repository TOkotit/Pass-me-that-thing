using Ami.BroAudio;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Game.Scripts.GameFiles.Events;

using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.GameRandomEvents.FloodEvent
{
    public class PumpInteractTerminal : EventTerminal
    {
        // [SyncVar]
        public readonly SyncVar<bool> _isFixed = new(true);

        [SerializeField] private SoundSource pipeSound = default;
        [SerializeField] PipeBreakEvent _pipeBreakEvent;
        [SerializeField] ParticleSystem _particleSystem;

        [Server]
        public override void TerminalAct(NetworkConnection conn)
        {
            base.TerminalAct(conn);
            
            if (_isFixed.Value) return;
            RpcPlayImpactParticles();
            RpcPlayImpactSound();
            CmdFixPipe();
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void CmdFixPipe()
        {
            if (_isFixed.Value) return;
            
            _pipeBreakEvent.PlayerFixedPressure();
        }
        
        [ObserversRpc]
        private void RpcPlayImpactSound()
        {
            if (pipeSound && !pipeSound.IsPlaying) 
            {
                pipeSound.Play();
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
    }
}