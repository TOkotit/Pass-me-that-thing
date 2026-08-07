using DI;
using Game.Scripts.Enums;
using Mirror;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.GameRandomEvents
{
    public class BaseGameEvent : NetworkBehaviour
    {
        [SerializeField, Range(0f, 1f)]
        private float _baseTriggerChance = 0.2f;

        [SyncVar] 
        private int _eventId;

        [SyncVar]
        [SerializeField]
        private GameEventsType eventType;

        [SyncVar]
        private bool _isEventActive;

        [SyncVar] 
        private int _roomNumber;

        [Inject] 
        private GameRandomEventManager  _gameRandomEventManager;

        private float _currentTriggerChance;

        public virtual int timeLimit { get; }
        public virtual int difficulty { get; }
        public virtual string description { get; }

        public int EventId => _eventId;
        public bool IsEventActive => _isEventActive;
        public int RoomNumber => _roomNumber;

        public float CurrentTriggerChance
        {
            get => _currentTriggerChance;
            set => _currentTriggerChance = Mathf.Clamp01(value);
        }
        
        public GameRandomEventManager GameRandomEventManager => _gameRandomEventManager;

        public GameEventsType EventType => eventType;

        public void UpdateCurrentTriggerChance(float chanceToAdd)
        {
            CurrentTriggerChance = _baseTriggerChance + chanceToAdd;
            Debug.Log($"[EVENT] UpdateCurrentTriggerChance {EventId} - {CurrentTriggerChance}");
        }
        
        [Server]
        public override void OnStartServer()
        {
            base.OnStartServer();
            _currentTriggerChance = _baseTriggerChance;
            RegisterEvent();
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
            Debug.Log($"[Server] Ивент ID:{_eventId} ({EventType}) ЗАПУЩЕН.");
        }
        
        [Server]
        public void StopEvent()
        {
            if (!_isEventActive) return;

            _isEventActive = false;
            OnStopEvent();
            Debug.Log($"[Server] Ивент ID:{_eventId} ({EventType}) ЗАВЕРШЕН.");
        }
        
        [Server] protected virtual void OnStartEvent() { }
        [Server] protected virtual void OnStopEvent() { }

    }
}