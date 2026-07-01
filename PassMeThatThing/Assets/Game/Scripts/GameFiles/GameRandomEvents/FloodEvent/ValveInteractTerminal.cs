using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Events;
using Game.Scripts.GameFiles.InteractableObjects;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.GameEvents.FloodEvent;

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
        
        // [SyncVar(OnChange = nameof(OnClosedStateChanged))]
        public readonly SyncVar<bool> _isClosed = new(true);

        private float _currentAngle;
        private float _targetAngle;
        private Quaternion _initialRotation;
        
        private void Awake()
        {
            _isClosed.OnChange += OnClosedStateChanged;
            if (pivot) _initialRotation = pivot.localRotation;
            
            
            _currentAngle = openAngle;
            _targetAngle = openAngle;
            
        }

        protected override void OnDestroy()
        {
            _isClosed.OnChange -= OnClosedStateChanged;
        }

        [Server]
        public override void TerminalAct(NetworkConnection conn)
        {
            if (IsTerminalBusy) return;
            
            if (_isClosed.Value) return;
            
            RpcPlayImpactParticles();
            if (ActivateMinigame(conn, floodEvent))
            {
                Debug.Log("<color=yellow> [Server] IsTerminalBusy = true");
                IsTerminalBusy = true;
                currentClient = conn;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public override void CmdMinigameComplete()
        {
            floodEvent.StopEvent();
            _isClosed.Value = true;
        }

        [ServerRpc(RequireOwnership = false)]
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


        [ServerRpc(RequireOwnership = false)]
        private void CmdCloseValve()
        {
            if (_isClosed.Value) return;
            
            Close();
            // RpcPlayImpactParticles();
            // floodEvent.PlayerFinishedAction();
        }
        
        [ObserversRpc]
        private void RpcPlayImpactParticles()
        {
            if (impactParticles && !impactParticles.isPlaying) 
            {
                impactParticles.Play();
            }
        }
        
        private void OnClosedStateChanged(bool oldValue, bool newValue, bool asServer)
        {
            _isClosed.Value = newValue;
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
            _isClosed.Value = false;
            _targetAngle = openAngle;
        }

        [Server]
        public void Close()
        {
            _isClosed.Value = true;
            _targetAngle = closedAngle;
        }
    }
}