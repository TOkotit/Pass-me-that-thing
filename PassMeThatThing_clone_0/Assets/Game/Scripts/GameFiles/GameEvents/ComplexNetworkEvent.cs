using DI;
using Game.Scripts.Enums;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.Events
{
    public abstract class ComplexNetworkEvent : NetworkBehaviour
    {
        
        
        [Inject] private GameEventManager  gameEventManager;
        
        [SyncVar(hook = nameof(OnTimerChanged))]
        public float timeLeft;
        
        [SyncVar]
        public GameEventDurationType  gameEventDurationType;

        [SyncVar]
        public bool isEventActive;
        
        public bool isPersistentSceneObject; 
        public GameEventsType eventType;

        public abstract void OnTimerChanged(float oldTime, float newTime);
    
        
        public override void OnStartServer()
        {
            if (isPersistentSceneObject)
            {
                var scope = LifetimeScope.Find<GameplayScope>();
                if (scope && scope.Container != null)
                {
                    gameEventManager = scope.Container.Resolve<GameEventManager>();
                }
                else
                {
                    gameEventManager = FindObjectOfType<GameEventManager>();
                }
            }
            if (isPersistentSceneObject && gameEventManager)
            {
                gameEventManager.RegisterSceneEvent(this);
            }
        }
        
        [Server]
        public virtual void StartEvent()
        {
            if(!isEventActive)
                isEventActive = true;
        }
        
        
        [Server]
        public virtual void StopEvent()
        {
            isEventActive = false;
            OnStopEvent(); 
    
            if (isPersistentSceneObject)
            {
                RpcOnEventDisabled(); 
            }
            else
            {
                NetworkServer.Destroy(gameObject);
            }
        }
        
        [Server] protected virtual void OnStopEvent() { }
        
        [ClientRpc]
        protected virtual void RpcOnEventDisabled()
        {
            
        }
    }
}