using Ami.BroAudio;
using Game.Scripts.GameFiles.Entity.Buildings.WireSystem;
using Mirror;
using System.Collections;
using UnityEngine;


namespace Game.Scripts.GameFiles.GameRandomEvents.Flood
{
    public class ValveInteractTerminal : EventTerminal
    {
        private const float openAngle = 270f;
        private const float closedAngle = 0f;
        private const float rotationTime = 3f; //s

        //refs
        [SerializeField] private FloodEvent floodEvent;
        [SerializeField] private WireNodePort port;
        
        //view
        [SerializeField] private Outline outline;
        [SerializeField] private ParticleSystem impactParticles;
        [SerializeField] private SoundSource valveSound = default;

        private Vector3 _initEulerAngles;
        private float _currentYOffset;
        private float _rotationProggress;

        public Outline Outline { get => outline; set => outline = value; }

        private void Awake()
        {
            _initEulerAngles = transform.localEulerAngles;
            _currentYOffset = 0f;
        }

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

        public override void OnFixedChanged(bool oldValue, bool newValue)
        {
            if (isServer)
            {
                Debug.Log($"[EVENT] valve terminal OnFixedChanged {newValue}");

                UpdatePort();

                RpcRotateValve(newValue ? closedAngle : openAngle);
            }
        }

        [Server]
        public void UpdatePort()
        {
            port.IsOn = IsFixed;
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

            float startAngle = _currentYOffset;

            while (_rotationProggress < rotationTime)
            {
                _rotationProggress += Time.deltaTime;
                var progressInPercantage = _rotationProggress / rotationTime;

                _currentYOffset = Mathf.Lerp(startAngle, newAngle, progressInPercantage);
                transform.localEulerAngles = new Vector3(_initEulerAngles.x, _initEulerAngles.y + _currentYOffset, _initEulerAngles.z);
                
                yield return null;
            }

            _currentYOffset = newAngle;
            transform.localEulerAngles = new Vector3(_initEulerAngles.x, _initEulerAngles.y + _currentYOffset, _initEulerAngles.z);
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