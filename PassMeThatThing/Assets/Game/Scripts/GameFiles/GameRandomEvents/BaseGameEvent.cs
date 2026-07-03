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

        public virtual int timeLimit { get; }
        public virtual int difficulty { get; }
        public virtual string description { get; }
        
        [SyncVar]
        private bool _isEventActive;
        public bool IsEventActive => _isEventActive;

        [SyncVar] private int _roomNumber;
        public int RoomNumber => _roomNumber;
        
        [Inject] private GameRandomEventManager  _gameRandomEventManager;
        
        [SerializeField, Range(0f, 1f)] 
        private float _baseTriggerChance = 0.2f;
        private float _currentTriggerChance;
        public float CurrentTriggerChance
        {
            get => _currentTriggerChance;
            set => _currentTriggerChance = Mathf.Clamp01(value);
        }
        
        public GameRandomEventManager GameRandomEventManager => _gameRandomEventManager;
        private bool _isInjected;
        
        
        [Server]
        public override void OnStartServer()
        {
            base.OnStartServer();
            _currentTriggerChance = _baseTriggerChance;
            RegisterEvent();
        }
        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!isServer && !_isInjected)
            {
                
                GameplayScope.Resolver?.Inject(this);
                _isInjected = true;
            }
        }
        
        private void RegisterEvent()
        {
            if (_gameRandomEventManager != null)
            {
                _eventId = _gameRandomEventManager.RegisterSceneEvent(this);
            }
            else
            {
                Debug.LogError("EventManager не заинжектился!");
            }
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