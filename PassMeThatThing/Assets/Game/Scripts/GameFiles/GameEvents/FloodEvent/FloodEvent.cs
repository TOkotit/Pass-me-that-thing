using Game.Scripts.Utils;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Events.FloodEvent
{
    
    public class FloodEvent : ComplexNetworkEvent
    {

        public GameObject waterMesh;
        private GameObject waterMeshInstance;

        public float MaxWaterwidth;
        
        private NetworkTimer _preparationTimer;
        
        [SyncVar]
        private bool _isFloodingActive = false;
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            
            
            _preparationTimer = new NetworkTimer(this, (time) => timeLeft = time);
            
            _preparationTimer.Set(2f);
            
            _preparationTimer.TimeIsOver += StartFlooding;
            
            
            
        }
        
        public override void OnTimerChanged(float oldTime, float newTime)
        {
            Debug.Log($"Осталось: {Mathf.CeilToInt(newTime)} сек.");
        }
        
        [Server]
        private void StartFlooding()
        {
            _isFloodingActive = true;
            
            timeLeft = 0;
        }
        
        private void FixedUpdate()
        {
            if (isServer && _isFloodingActive && waterMeshInstance)
            {
                ExecuteFloodLogic();
            }
        }
        
        [Server]
        private void ExecuteFloodLogic()
        {
            
            if (!waterMeshInstance) return;
            
            if (waterMeshInstance.transform.localScale.x < MaxWaterwidth)
                waterMeshInstance.transform.localScale += new Vector3(1f, 0f, 1f) * Time.fixedDeltaTime;
        }
        
        [Server]
        public void PlayerFinishedAction()
        {
            _preparationTimer?.Stop();
            StopEvent();
        }

        [Server]
        public override void StartEvent()
        {
            _preparationTimer.Start();
            isEventActive = true; 
            _isFloodingActive = false;
            
            waterMeshInstance = Instantiate(waterMesh);
            waterMeshInstance.transform.position = transform.position;
            waterMeshInstance.transform.Translate(Vector3.down * 0.2f);
            
            NetworkServer.Spawn(waterMeshInstance);
        }
        
        [Server]
        protected override void OnStopEvent()
        {
            
            _isFloodingActive = false;
            
            
            if (waterMeshInstance != null)
            {
                NetworkServer.Destroy(waterMeshInstance);
            }
        }
    }
}