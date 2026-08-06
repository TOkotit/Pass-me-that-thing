using Ami.BroAudio;
using Game.Scripts.GameFiles.GameRandomEvents;
using Mirror;
using UnityEngine;
using static UnityEditor.Profiling.RawFrameDataView;

namespace Game.Scripts.GameFiles.GameRandomEvents.Flood
{
    public class PumpInteractTerminal : EventTerminal
    {
        [SerializeField] private BrokenPumpEvent brokenPumpEvent;

        [SerializeField] private SoundSource pipeSound = default;
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

        //View
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