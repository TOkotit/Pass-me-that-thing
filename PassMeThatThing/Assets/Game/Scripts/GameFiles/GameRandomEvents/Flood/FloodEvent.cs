using Mirror;
using UnityEngine;


namespace Game.Scripts.GameFiles.GameRandomEvents.Flood
{
    public class FloodEvent : BaseGameEvent
    {
        [SerializeField] private ValveInteractTerminal terminal;

        [SerializeField] private GameObject waterMeshPrefab;
        [SerializeField] private Transform waterMeshTransform;
        
        [SerializeField] private float maxWaterWidth;

        private bool _isFloodingActive = false;

        private GameObject _waterMeshInstance;


        
        protected override void OnStartEvent()
        {
            _isFloodingActive = true;

            _waterMeshInstance = Instantiate(waterMeshPrefab);
            _waterMeshInstance.transform.position = transform.position;
            _waterMeshInstance.transform.Translate(Vector3.down * 0.2f);
            
            NetworkServer.Spawn(_waterMeshInstance);

            if (terminal)
                terminal.IsFixed = false;

            RpcEnableOutline();
        }

        [Server]
        public void FixEvent()
        {
            GameRandomEventManager.DeactivateEvent(EventId);
        }

        [Server]
        protected override void OnStopEvent()
        {
            _isFloodingActive = false;
            if (_waterMeshInstance != null)
            {
                NetworkServer.Destroy(_waterMeshInstance);
            }

            RpcDisableOutline();
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

        //View
        [ClientRpc]
        private void RpcEnableOutline()
        {
            terminal.Outline.enabled = true;
        }

        [ClientRpc]
        private void RpcDisableOutline()
        {
            terminal.Outline.enabled = false;
        }
    }
}