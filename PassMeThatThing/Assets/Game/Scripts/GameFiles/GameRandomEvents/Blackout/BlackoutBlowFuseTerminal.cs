using Ami.BroAudio;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.GameRandomEvents.Blackout
{
    public class BlackoutBlowFuseTerminal : EventTerminal
    {
        [SerializeField] private BlackoutBlowFuseEvent blowFuseEvent;

        [SerializeField] private ParticleSystem impactParticles;
        [SerializeField] private Outline outline;
        [SerializeField] private SoundSource electricity = default;


        public Outline Outline => outline;


        [Server]
        public override void TerminalAct(NetworkConnectionToClient conn)
        {
            if (IsTerminalBusy) return;
            if (IsFixed) return;


            if (ActivateMinigame(conn, blowFuseEvent))
            {
                Debug.Log("[EVENTS] IsTerminalBusy = true");
                IsTerminalBusy = true;
                currentClient = conn;
            }

            RpcPlayImpactSound();
            RpcPlayImpactParticles();
        }
        
        [Command(requiresAuthority = false)]
        public override void CmdMinigameComplete()
        {
            FixTerminal();
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
        private void FixTerminal()
        {
            IsFixed = true;
            
            if (blowFuseEvent != null)
            {
                blowFuseEvent.FixEvent();
            }
        }

        [ClientRpc]
        private void RpcPlayImpactParticles()
        {
            if (impactParticles && !impactParticles.isPlaying) 
            {
                impactParticles.Play();
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
    }
}