using FishNet.Object;
using FishNet.Object.Synchronizing;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Events;
using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.GameEvents.FloodEvent;
using Mirror;
using Mirror.SimpleWeb;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.GameEvents.FloodEvent
{
    public class ValveInteractTerminal : EventTerminal
    {
        [SerializeField] private ParticleSystem impactParticles;
        [SerializeField] private Transform pivot;
        
        [SerializeField] private Vector3 rotationAxis = Vector3.forward;
        [SerializeField] private float openAngle = 0f;
        [SerializeField] private float closedAngle = 360f;
        [SerializeField] private float moveSpeed = 100f;
        [SerializeField] private Events.FloodEvent.FloodEvent floodEvent;
        
        [SyncVar(hook = nameof(OnClosedStateChanged))]
        public bool _isClosed = true;

        private float _currentAngle;
        private float _targetAngle;
        private Quaternion _initialRotation;
        
        private void Awake()
        {
            if (pivot) _initialRotation = pivot.localRotation;
            
            
            _currentAngle = openAngle;
            _targetAngle = openAngle;
            
        }

        [Server]
        public override void TerminalAct(NetworkConnectionToClient conn)
        {
            if (IsTerminalBusy) return;
            
            if (_isClosed) return;
            
            RpcPlayImpactParticles();
            if (ActivateMinigame(conn, floodEvent))
            {
                Debug.Log("<color=yellow> [Server] IsTerminalBusy = true");
                IsTerminalBusy = true;
                currentClient = conn;
            }
        }

        [Command(requiresAuthority = false)]
        public override void CmdMinigameComplete()
        {
            floodEvent.StopEvent();
            _isClosed = true;
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


        [Command(requiresAuthority = false)]
        private void CmdCloseValve()
        {
            if (_isClosed) return;
            
            Close();
            // RpcPlayImpactParticles();
            // floodEvent.PlayerFinishedAction();
        }
        
        [ClientRpc]
        private void RpcPlayImpactParticles()
        {
            if (impactParticles && !impactParticles.isPlaying) 
            {
                impactParticles.Play();
            }
        }
        
        private void OnClosedStateChanged(bool oldValue, bool newValue)
        {
            _isClosed = newValue;
            _targetAngle = newValue ? closedAngle : openAngle;
            
            if (oldValue == newValue) 
            {
                _currentAngle = _targetAngle;
            }
        }
        
        
        private void Update()
        {
            if (!pivot) return;

            if (!(Mathf.Abs(_currentAngle - _targetAngle) > 0.01f)) return;
            _currentAngle = Mathf.MoveTowards(_currentAngle, _targetAngle, moveSpeed * Time.deltaTime);
            pivot.localRotation = _initialRotation * Quaternion.Euler(rotationAxis * _currentAngle);
        }
        
        [Server]
        public void Open()
        { 
            _isClosed = false;
            _targetAngle = openAngle;
        }

        [Server]
        public void Close()
        {
            _isClosed = true;
            _targetAngle = closedAngle;
        }
    }
}