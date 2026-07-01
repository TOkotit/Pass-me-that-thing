using FishNet.Object;
using FishNet.Object.Synchronizing;
using Game.Scripts.GameFiles.GameEvents.FloodEvent;
using Game.Scripts.Utils;

using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.GameFiles.Events.FloodEvent
{
    
    public class FloodEvent : BaseGameEvent
    {

        public GameObject waterMesh;
        private GameObject _waterMeshInstance;
        [SerializeField] Transform _waterMeshTransform;
        
        [SerializeField] private float maxWaterWidth;
        [SerializeField] private ValveInteractTerminal valve;
        
        // [SyncVar]
        private readonly SyncVar<bool> _isFloodingActive = new(false);
        public bool IsFloodingActive => _isFloodingActive.Value;
        
        protected override void OnStartEvent()
        {
            _isFloodingActive.Value = true;
            valve.Open();
            _waterMeshInstance = Instantiate(waterMesh);
            _waterMeshInstance.transform.position = transform.position;
            _waterMeshInstance.transform.Translate(Vector3.down * 0.2f);
            
            ServerManager.Spawn(_waterMeshInstance);
        }
        
        private void FixedUpdate()
        {
            if (IsServerStarted && _isFloodingActive.Value && _waterMeshInstance != null)
            {
                ExecuteFloodLogic();
            }
        }
        
        [Server]
        private void ExecuteFloodLogic()
        {
            if (_waterMeshInstance.transform.localScale.x < maxWaterWidth)
            {
                _waterMeshInstance.transform.localScale += new Vector3(1f, 0f, 1f) * Time.fixedDeltaTime;
            }
        }
        
        [Server]
        public void PlayerFinishedAction()
        {
            StopEvent();
        }

        [Server]
        protected override void OnStopEvent()
        {
            _isFloodingActive.Value = false;
            if (_waterMeshInstance != null)
            {
                ServerManager.Despawn(_waterMeshInstance);
            }
            GameRandomEventManager.DisableEvent(EventId);
        }
    }
}