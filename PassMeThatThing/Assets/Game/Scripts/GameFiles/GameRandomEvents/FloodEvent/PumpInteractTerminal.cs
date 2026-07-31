using Ami.BroAudio;
using Game.Scripts.GameFiles.Events;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.GameRandomEvents.FloodEvent
{
    public class PumpInteractTerminal : EventTerminal
    {
        [SyncVar]
        public bool _isFixed = true;

        [SerializeField] private SoundSource pipeSound = default;

        [SerializeField] private BrokenPumpEvent brokenPumpEvent;
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] public Outline _outline;

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
            
            brokenPumpEvent.PlayerFixedPressure();
        }
        
        [ClientRpc]
        private void RpcPlayImpactSound()
        {
            if (pipeSound) 
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