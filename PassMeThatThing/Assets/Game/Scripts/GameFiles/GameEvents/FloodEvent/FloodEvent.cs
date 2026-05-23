using Game.Scripts.Utils;
using Mirror;
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
        
        [SyncVar]
        private bool _isFloodingActive = false;
        
        protected override void OnStartEvent()
        {
            _isFloodingActive = true;
            
            _waterMeshInstance = Instantiate(waterMesh);
            _waterMeshInstance.transform.position = transform.position;
            _waterMeshInstance.transform.Translate(Vector3.down * 0.2f);
            
            NetworkServer.Spawn(_waterMeshInstance);
        }
        
        private void FixedUpdate()
        {
            if (isServer && _isFloodingActive && _waterMeshInstance != null)
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
            _isFloodingActive = false;
            
            if (_waterMeshInstance != null)
            {
                NetworkServer.Destroy(_waterMeshInstance);
            }
        }
    }
}