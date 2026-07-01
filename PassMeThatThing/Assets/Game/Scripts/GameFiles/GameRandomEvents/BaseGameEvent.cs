using DI;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Game.Scripts.Enums;

using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.Events
{
    public class BaseGameEvent : NetworkBehaviour
    {
        // [SyncVar] 
        private readonly SyncVar<int> _eventId = new(); 
        public int EventId => _eventId.Value;
        
        // [SyncVar] 
        public readonly SyncVar<GameEventsType> eventType = new();

        public virtual int timeLimit { get; }
        public virtual int difficulty { get; }
        public virtual string description { get; }
        
        // [SyncVar]
        private readonly SyncVar<bool> _isEventActive = new();
        public bool IsEventActive => _isEventActive.Value;

        //[SyncVar] 
        private readonly SyncVar<int> _roomNumber = new();
        public int RoomNumber => _roomNumber.Value;
        
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
            _currentTriggerChance = _baseTriggerChance;
            if (_gameRandomEventManager)
            {
                _eventId.Value = _gameRandomEventManager.RegisterSceneEvent(this);
                Debug.Log($"[BaseGameEvent] Ивент успешно зарегистрирован на сервере. ID: {_eventId.Value}");
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
            _eventId.Value = uniqueId;
            eventType.Value = assignedType;
            
            manager.RegisterSceneEvent(this);
        }
        
        [Server]
        public void StartEvent()
        {
            if (_isEventActive.Value) return;
            
            _isEventActive.Value = true;
            OnStartEvent();
            Debug.Log($"[Server] Ивент ID:{_eventId} ({eventType}) ЗАПУЩЕН.");
        }
        
        [Server]
        public void StopEvent()
        {
            if (!_isEventActive.Value) return;

            _isEventActive.Value = false;
            OnStopEvent();
            Debug.Log($"[Server] Ивент ID:{_eventId} ({eventType}) ЗАВЕРШЕН.");
        }
        
        [Server] protected virtual void OnStartEvent() { }
        [Server] protected virtual void OnStopEvent() { }

    }
}