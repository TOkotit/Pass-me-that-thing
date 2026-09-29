using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Game.Scripts.GameFiles.GameRandomEvents.GameTasks;
using Game.Scripts.Enums;
using Mirror;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace Game.Scripts.GameFiles.GameRandomEvents
{
    public class GameRandomEventManager : NetworkBehaviour
    {
        private WaitForSeconds _waitTasksTick = new WaitForSeconds(1f);

        private int _idGenerator = 1;
        private readonly SyncDictionary<int, BaseGameEvent> _sceneEvents = new();
        
        //ивенты которые запущены
        private readonly SyncDictionary<int, BaseGameEvent> _startedEvents = new();

        private readonly SyncDictionary<GameEventsType, int> _fixedEvents = new();

        private float _pipebreakChanceBoost;


        //TODO вынести в отдельный класс
        //починка станций, сдача предметов и тд
        private Coroutine _timeTickCoroutine;
        private int _idTaskHandlerGenerator = 1;
        private SyncDictionary<int, GameTaskHandler> _gameTaskHandlers = new();
        private int _idTaskGenerator = 1;
        private SyncDictionary<int, GameTask> _gameTasks = new();
        private SyncDictionary<int, GameTaskData> _gameTasksData = new();


        public SyncDictionary<int, BaseGameEvent> StartedEvents => _startedEvents;
        public SyncDictionary<int, GameTask> GameTasks => _gameTasks;
        public SyncDictionary<int, GameTaskData> GameTasksData => _gameTasksData;
        public float PipebreakChanceBoost 
        { 
            get => _pipebreakChanceBoost; 
            set
            {
                if (value != _pipebreakChanceBoost) 
                    OnPipeBreakChanceBoostChanged?.Invoke(value);
                _pipebreakChanceBoost = value;
            } 
        }

        public IReadOnlyDictionary<GameEventsType, int> FixedEvents => _fixedEvents;

        

        public IEnumerable<BaseGameEvent> GetAllEvents() => _sceneEvents.Values;

        public event Action<int> OnQuotaValueAdded;
        
        
        public event Action<SyncDictionary<int, BaseGameEvent>> OnEventReceived;

        public event Action<float> OnPipeBreakChanceBoostChanged;

        public override void OnStartClient()
        {
            OnEventReceived?.Invoke(StartedEvents);
            _timeTickCoroutine = StartCoroutine(CalcGameTasksTime());
        }

        public override void OnStopClient()
        {
            StopCoroutine(_timeTickCoroutine);
        }

        [Server]
        public int RegisterSceneEvent(BaseGameEvent gameEvent)
        {
            var assignedId = _idGenerator;
            
            _idGenerator++; 
            Debug.Log($"<color=green>RegisterSceneEvent, id: {assignedId}</color>");
            _sceneEvents.Add(assignedId, gameEvent);
            
            return assignedId;
        }

        [Server]
        public void UnregisterEvent(int id)
        {
            if (_sceneEvents.ContainsKey(id))
            {
                _sceneEvents.Remove(id);
            }
        }

        [Server]
        public BaseGameEvent GetEventById(int id)
        {
            if (_sceneEvents.TryGetValue(id, out var foundEvent))
            {
                return foundEvent;
            }
            
            Debug.LogWarning($"[GameEventManager] Ивент с ID {id} не найден!");
            return null;
        }
        
        [Server]
        public void ActivateEvent(int eventId)
        {
            if (_sceneEvents.TryGetValue(eventId, out var gameEvent))
            {
                gameEvent.StartEvent();
                StartedEvents.Add(eventId, gameEvent);
            }
            else
            {
                Debug.LogWarning($"[GameEventManager] Невозможно запустить: ивент с ID:{eventId} не найден на карте.");
            }
        }

        [Server]
        public void DeactivateEvent(int eventId)
        {
            if (_sceneEvents.TryGetValue(eventId, out var gameEvent))
            {
                gameEvent.StopEvent();
                AddFixedEvent(gameEvent.EventType);
                StartedEvents.Remove(eventId);
            }
            else
            {
                Debug.LogWarning($"[GameEventManager] Невозможно остановить: ивент с ID:{eventId} не найден на карте.");
            }
        }

        [Server]
        public void AddFixedEvent(GameEventsType type)
        {
            if (_fixedEvents.ContainsKey(type))
            {
                _fixedEvents[type]++;
            }
            else
            {
                _fixedEvents.Add(type, 1);
            }
        }

        //в начале дня после Rest
        [Server]
        public void ClearFixedEvent()
        {
            _fixedEvents.Clear();
        }


        [Command(requiresAuthority = false)]
        public void CmdStopEventById(int id)
        {
            Debug.Log($"<color=yellow> CmdStopEventById {id}");
            GetEventById(id).StopEvent();
        }
        
        [Server]
        public void TryTriggerRandomEvents()
        {
            foreach (var kvp in _sceneEvents)
            {
                var gameEvent = kvp.Value;
                
                if (gameEvent.IsEventActive) continue;

                if (Random.value <= gameEvent.CurrentTriggerChance)
                {
                    ActivateEvent(gameEvent.EventId);
                }
            }
        }

        //GAME TASKS
        [Server]
        public int RegisterSceneTaskHandler(GameTaskHandler taskHandler)
        {
            var assignedId = _idTaskHandlerGenerator;

            _idTaskHandlerGenerator++;
            Debug.Log($"RegisterSceneTaskHandler, id: {assignedId}");

            _gameTaskHandlers.Add(assignedId, taskHandler);

            return assignedId;
        }

        [Server]
        public void UnregisterTaskHandler(int id)
        {
            if (_gameTaskHandlers.ContainsKey(id))
            {
                _gameTaskHandlers.Remove(id);
            }
        }

        [Server]
        public GameTask CreateGameTask(GameTaskParameters parameters)
        {
            _idTaskGenerator++;

            var newTask = new GameTask();

            newTask.gameTaskId = _idTaskGenerator;
            newTask.InitTask(parameters.timeLimit);
            newTask.OnTaskTimerEnd += OverdueGameTask;

            var newTaskData = new GameTaskData();

            newTaskData.gameTaskId = _idTaskGenerator;
            newTaskData.gameTaskType = parameters.gameTaskType;
            newTaskData.taskField = parameters.taskField;
            newTaskData.cost = parameters.cost;

            _gameTasks.Add(_idTaskGenerator, newTask);
            _gameTasksData.Add(_idTaskGenerator, newTaskData);

            return newTask;
        }

        [Server]
        public void OverdueGameTask(int taskId)
        {
            if (_gameTasksData.TryGetValue(taskId, out var taskData))
            {
                taskData.isTaskOverdue = true;

                _gameTasksData[taskId] = taskData;
            }
        }

        [Server]
        public void DestroyGameTask(int taskId)
        {
            if (_gameTasks.TryGetValue(taskId, out var task))
            {
                task.OnTaskTimerEnd -= OverdueGameTask;
                task.DestroyTask();
                _gameTasks.Remove(taskId);
            }

            if (_gameTasksData.TryGetValue(taskId, out var taskData))
            {
                _gameTasksData.Remove(taskId);
            }
        }

        [Server]
        public void CompleteAndDestroyGameTask(int taskId)
        {
            if (_gameTasksData.TryGetValue(taskId, out var taskData))
            {
                if (!taskData.isTaskOverdue)
                {
                    AddTaskCostQuota(taskData.cost);
                }
                else
                {
                    AddTaskCostQuota(taskData.cost / 2);
                }
            }

            DestroyGameTask(taskId);
        }

        [Server]
        public void AddTaskCostQuota(int value)
        {
            OnQuotaValueAdded?.Invoke(value);
        }

        //просчет таймеров происходит и на сервере и на клиенте
        public IEnumerator CalcGameTasksTime()
        {
            while (true)
            {
                var taskIds = _gameTasks.Keys.ToArray();

                foreach (var taskId in taskIds)
                {
                    if (_gameTasks.TryGetValue(taskId, out var task) &&
                        _gameTasksData.TryGetValue(taskId, out var taskData))
                    {
                        if (!taskData.isTaskOverdue)
                        {
                            task.TickTimer(); 
                        }
                    }
                }
                yield return _waitTasksTick;
            }
        }
    }
}