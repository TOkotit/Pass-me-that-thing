using Assets.Game.Scripts.GameFiles.GameRandomEvents.GameTasks;
using Assets.Game.Scripts.GameFiles.GlobalStageManager;
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

        [SerializeField] private GameEventsType eventType;

        [SerializeField] private GameTaskHandler gameTaskHandler;

        [SerializeField] private float taskTimeLimit;
        [SerializeField] private int taskPoints;

        [SyncVar] 
        private int _eventId;

        [SyncVar]
        private bool _isEventActive;

        [SyncVar] 
        private int _roomNumber;

        [Inject] private GameRandomEventManager _gameRandomEventManager;

        [Inject] private GlobalStageDatabase _globalStageDatabase;

        private float _currentTriggerChance;

        private GameTask _currentTask;

        public int EventId => _eventId;
        public bool IsEventActive => _isEventActive;
        public int RoomNumber => _roomNumber;
        public float CurrentTriggerChance => _currentTriggerChance;
        public GameRandomEventManager GameRandomEventManager => _gameRandomEventManager;
        public GameEventsType EventType => eventType;


        public event Action<float> OnSyncTimerChanged;

        public void UpdateCurrentTriggerChance(float chanceToAdd)
        {
            _currentTriggerChance = Mathf.Clamp01(_baseTriggerChance + chanceToAdd);
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
                Debug.LogError("EventManager не заинжектился!");
        }
        
        [Server]
        public void StartEvent()
        {
            if (_isEventActive) return;
            
            _isEventActive = true;
            OnStartEvent();

            _currentTask = gameTaskHandler.CreateGameTask(GetEventTaskParams());
            _currentTask.OnTaskTimerEnd += OnTaskTimerEnd;

            Debug.Log($"[Server] Ивент ID:{_eventId} ({EventType}) ЗАПУЩЕН.");
        }
        
        [Server]
        public void StopEvent()
        {
            if (!_isEventActive) return;

            _isEventActive = false;
            OnStopEvent();

            _currentTask.OnTaskTimerEnd -= OnTaskTimerEnd;
            gameTaskHandler.CompleteAndDestroyGameTask(_currentTask.gameTaskId);


            Debug.Log($"[Server] Ивент ID:{_eventId} ({EventType}) ЗАВЕРШЕН.");
        }


        [Server]
        private GameTaskParameters GetEventTaskParams()
        {
            var testPars = new GameTaskParameters();
            testPars.gameTaskType = GameTaskType.GameEvent;
            testPars.timeLimit = taskTimeLimit;
            testPars.taskField = eventType.ToString();
            testPars.cost = taskPoints * _globalStageDatabase.BaseQuotaConst;

            return testPars;
        }

        [Server] protected virtual void OnStartEvent() { }
        [Server] protected virtual void OnStopEvent() { }

        [Server] protected virtual void OnTaskTimerEnd(int taskId) { }
    }
}