using Game.Scripts.GameFiles.Events;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.GameRandomEvents.FloodEvent
{
    public class PumpInteractTerminal : EventTerminal
    {
        
        [SyncVar]
        public bool _isFixed = true;
        
        [SerializeField] PipeBreakEvent _pipeBreakEvent;
        [SerializeField] ParticleSystem _particleSystem;
        
        [Server]
        public override void TerminalAct(NetworkConnectionToClient conn)
        {
            base.TerminalAct(conn);
            
            if (_isFixed) return;
            RpcPlayImpactParticles();
            CmdFixPipe();
        }
        
        [Command(requiresAuthority = false)]
        private void CmdFixPipe()
        {
            if (_isFixed) return;
            
            _pipeBreakEvent.PlayerFixedPressure();
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