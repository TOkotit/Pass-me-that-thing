using Ami.BroAudio;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.GameRandomEvents.Flood
{
    public class PumpInteractTerminal : EventTerminal
    {
        [SerializeField] private BrokenPumpEvent brokenPumpEvent;

        [SerializeField] private SoundID pipeSound;
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] public Outline _outline;

        [Server]
        public override void TerminalAct(NetworkConnectionToClient conn)
        {
            base.TerminalAct(conn);
            
            if (IsFixed) return;

            FixTerminal();

            RpcPlayImpactParticles();
            RpcPlayImpactSound();
        }
        

        [Server]
        private void FixTerminal()
        {
            IsFixed = true;

            if (brokenPumpEvent != null)
            {
                brokenPumpEvent.FixEvent();
            }
        }

        [ClientRpc]
        private void RpcPlayImpactSound()
        { 
            BroAudio.Play(pipeSound, transform.position);
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