using Ami.BroAudio;
using Game.Scripts.GameFiles.Entity.Buildings.WireSystem;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.GameFiles.GameRandomEvents.Blackout
{
    public class BlackoutCutWiresTerminal : EventTerminal
    {
        [SerializeField] private BlackoutCutWiresEvent cutWiresEvent;
        [SerializeField] private WireNodePort port;

        [SerializeField] private Outline outline;
        [SerializeField] private ParticleSystem impactParticles;
        [SerializeField] private SoundSource electricity = default;


        public Outline Outline { get => outline; set => outline = value; }

        [Server]
        public override void TerminalAct(NetworkConnectionToClient conn)
        {
            if (IsTerminalBusy) return;
            if (IsFixed) return;

            
            if (ActivateMinigame(conn, cutWiresEvent))
            {
                Debug.Log("<color=yellow> [Server] IsTerminalBusy = true");
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
            
            if (cutWiresEvent != null)
            {
                cutWiresEvent.FixEvent();
            }
        }

        public override void OnFixedChanged(bool oldValue, bool newValue)
        {
            if (isServer)
            {
                Debug.Log("[EVENT] wires terminal OnFixedChanged");

                UpdatePort();
            }
        }

        [Server]
        public void UpdatePort()
        {
            port.IsOn = IsFixed;
        }

        //View
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