using DI;
using Game.Scripts.Enums;
using Game.Scripts.Utils;
using Mirror;
using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.GameRandomEvents
{
    public class BaseGameEvent : NetworkBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _baseTriggerChance = 0.2f;
        [SerializeField] private int _timeLimit;
        [SerializeField] private GameEventsType eventType;

        [SyncVar] 
        private int _eventId;

        [SyncVar]
        private bool _isEventActive;

        [SyncVar] 
        private int _roomNumber;

        [SyncVar(hook = nameof(OnTimeChanged))]
        private float _syncRemainingTime;

        [Inject] private GameRandomEventManager _gameRandomEventManager;

        private NetworkTimer _timer;
        private float _currentTriggerChance;


        public int EventId => _eventId;
        public bool IsEventActive => _isEventActive;
        public int RoomNumber => _roomNumber;
        public float CurrentTriggerChance => _currentTriggerChance;
        public GameRandomEventManager GameRandomEventManager => _gameRandomEventManager;
        public GameEventsType EventType => eventType;
        public int TimeLimit => _timeLimit;

        public event Action<float> OnSyncTimerChanged;

        public void UpdateCurrentTriggerChance(float chanceToAdd)
        {
            _currentTriggerChance = Mathf.Clamp01(_baseTriggerChance + chanceToAdd);
            Debug.Log($"[EVENT] UpdateCurrentTriggerChance {EventId} - {CurrentTriggerChance}");
        }

        private void Awake()
        {
            _timer = new NetworkTimer(this, OnTimerTick);
            _timer.TimeIsOver += OnTimerFinished;
        }

        [Server]
        public override void OnStartServer()
        {
            base.OnStartServer();
            _currentTriggerChance = _baseTriggerChance;
            RegisterEvent();
        }

        private void OnDestroy()
        {
            if (_timer != null)
            {
                _timer.TimeIsOver -= OnTimerFinished;
                _timer.Stop();
            }
        }

        private void RegisterEvent()
        {
            if (_gameRandomEventManager != null)
            {
                _eventId = _gameRandomEventManager.RegisterSceneEvent(this);
            }
            else
                Debug.LogError("EventManager не заинжектился!");
        }
        
        [Server]
        public void StartEvent()
        {
            if (_isEventActive) return;
            
            _isEventActive = true;
            OnStartEvent();

            StartTimer(_timeLimit);


            Debug.Log($"[Server] Ивент ID:{_eventId} ({EventType}) ЗАПУЩЕН.");
        }
        
        [Server]
        public void StopEvent()
        {
            if (!_isEventActive) return;

            _isEventActive = false;
            OnStopEvent();

            _timer.Stop();

            Debug.Log($"[Server] Ивент ID:{_eventId} ({EventType}) ЗАВЕРШЕН.");
        }

        [Server]
        public void StartTimer(float duration)
        {
            _timer.Set(duration);
            _timer.Start();
        }

        private void OnTimerTick(float remainingTime)
        {
            _syncRemainingTime = remainingTime;
        }

        private void OnTimeChanged(float oldTime, float newTime)
        {
            OnSyncTimerChanged?.Invoke(newTime / _timeLimit);
        }

        private void OnTimerFinished()
        {
            OnTimerEnd();
        }

        [Server] protected virtual void OnStartEvent() { }
        [Server] protected virtual void OnStopEvent() { }
        [Server] protected virtual void OnTimerEnd() { }
    }
}