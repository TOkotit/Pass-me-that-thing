using Ami.BroAudio;
using Game.Scripts.GameFiles.Entity.Enemy;
using Game.Scripts.GameFiles.GameRandomEvents;
using Mirror;
using System.Collections;
using UnityEngine;


namespace Game.Scripts.GameFiles.GameRandomEvents.Flood
{
    public class ValveInteractTerminal : EventTerminal
    {
        private const float openAngle = 0f;
        private const float closedAngle = 360f;
        private const float rotationTime = 1f; //s

        //refs
        [SerializeField] private FloodEvent floodEvent;

        //valve rotation
        [SerializeField] private Transform pivot;
        
        //view
        [SerializeField] private Outline outline;
        [SerializeField] private ParticleSystem impactParticles;
        [SerializeField] private SoundSource valveSound = default;

        private float _rotationProggress;

        public Outline Outline { get => outline; set => outline = value; }


        [Server]
        public override void TerminalAct(NetworkConnectionToClient conn)
        {
            if (IsTerminalBusy) return;
            if (IsFixed) return;
            

            if (ActivateMinigame(conn, floodEvent))
            {
                Debug.Log("[EVENTS] IsTerminalBusy = true");
                IsTerminalBusy = true;
                currentClient = conn;
            }

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
            Debug.Log("<color=yellow> [EVENT] IsTerminalBusy = false");
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

            if (floodEvent != null)
            {
                floodEvent.FixEvent();
            }
        }

        public override void OnFixedChanged (bool oldValue, bool newValue)
        {
            if (isServer)
            {
                Debug.Log("[EVENT] valve terminal OnFixedChanged");

                RpcRotateValve(newValue ? closedAngle : openAngle);
            }
        }


        //View
        [ClientRpc]
        private void RpcRotateValve(float newAngle)
        {
            StartCoroutine(RotateValve(newAngle));
        }

        private IEnumerator RotateValve(float newAngle)
        {
            _rotationProggress = 0f;

            var startRotation = pivot.rotation;
            var targetRotation = Quaternion.AngleAxis(newAngle, pivot.forward);

            while (_rotationProggress < rotationTime)
            {
                _rotationProggress += Time.deltaTime;
                var progressInPercantage = _rotationProggress / rotationTime;

                pivot.rotation = Quaternion.Slerp(startRotation, targetRotation, progressInPercantage);
                yield return null;
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
            if (valveSound && !valveSound.IsPlaying)
            {
                valveSound.Play();
            }
        }
    }
}