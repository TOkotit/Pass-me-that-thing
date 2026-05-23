using Game.Scripts.Enums;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Events
{
    public class BaseGameEvent : NetworkBehaviour
    {
        [SyncVar] private int _eventId; 
        public int EventId => _eventId;
        [SyncVar] public GameEventsType eventType;
        
        [SyncVar] private bool _isEventActive;
        public bool IsEventActive => _isEventActive;
        
        [Inject] GameEventManager  gameEventManager;
        
        [Server]
        public override void OnStartServer()
        {
            base.OnStartServer();
    
            if (gameEventManager)
            {
                _eventId = gameEventManager.RegisterSceneEvent(this);
                
            }
            else
            {
                Debug.LogError($"[BaseGameEvent] Менеджер ивентов не найден на сцене! Ивент ID:{_eventId} не зарегистрирован.");
            }
        }
        
        
        public void Initialize(int uniqueId, GameEventsType assignedType, GameEventManager manager)
        {
            _eventId = uniqueId;
            eventType = assignedType;
            
            manager.RegisterSceneEvent(this);
        }
        
        [Server]
        public void StartEvent()
        {
            if (_isEventActive) return;
            
            _isEventActive = true;
            OnStartEvent();
            Debug.Log($"[Server] Ивент ID:{_eventId} ({eventType}) ЗАПУЩЕН.");
        }
        
        [Server]
        public void StopEvent()
        {
            if (!_isEventActive) return;

            _isEventActive = false;
            OnStopEvent();
            Debug.Log($"[Server] Ивент ID:{_eventId} ({eventType}) ЗАВЕРШЕН.");
        }
        
        [Server] protected virtual void OnStartEvent() { }
        [Server] protected virtual void OnStopEvent() { }
    }
}