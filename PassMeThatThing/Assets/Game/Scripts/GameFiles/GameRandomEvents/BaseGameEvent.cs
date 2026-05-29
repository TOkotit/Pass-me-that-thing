using DI;
using Game.Scripts.Enums;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.Events
{
    public class BaseGameEvent : NetworkBehaviour
    {
        [SyncVar] private int _eventId; 
        public int EventId => _eventId;
        
        [SyncVar] public GameEventsType eventType;
        
        [SyncVar]
        private bool _isEventActive;
        public bool IsEventActive => _isEventActive;

        [SyncVar] private int _roomNumber;
        public int RoomNumber => _roomNumber;
        
        [Inject] private GameRandomEventManager  _gameRandomEventManager;
        
        public GameRandomEventManager GameRandomEventManager => _gameRandomEventManager;
        private bool _isInjected;
        
        private void EnsureInjected()
        {
            if (_isInjected) return;

            var scope = LifetimeScope.Find<GameplayScope>();
            if (scope != null && scope.Container != null)
            {
                scope.Container.Inject(this);
                _isInjected = true;
            }
        }        
        
        [Server]
        public override void OnStartServer()
        {
            base.OnStartServer();
            EnsureInjected();

            if (_gameRandomEventManager)
            {
                _eventId = _gameRandomEventManager.RegisterSceneEvent(this);
                Debug.Log($"[BaseGameEvent] Ивент успешно зарегистрирован на сервере. ID: {_eventId}");
            }
            else
            {
                Debug.LogError($"[BaseGameEvent] Менеджер ивентов НЕ НАЙДЕН вообще нигде (ни DI, ни Find)! Ивент не зарегистрирован.");
            }
        }
        public override void OnStartClient()
        {
            base.OnStartClient();
            EnsureInjected();
        }
        
        
        public void Initialize(int uniqueId, GameEventsType assignedType, GameRandomEventManager manager)
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