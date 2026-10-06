using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Game.Scripts.GameFiles.GameRandomEvents.GameTasks;
using Assets.Game.Scripts.GameFiles.GameRandomEvents.ItemTasks;
using Game.Scripts.Enums;
using kcp2k;
using Mirror;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace Game.Scripts.GameFiles.GameRandomEvents
{
    public class GameRandomEventManager : NetworkBehaviour
    {
        //все объекты ивентов
        private int _idGenerator = 1;
        private readonly SyncDictionary<int, BaseGameEvent> _sceneEvents = new();
        
        //ивенты которые запущены
        private readonly SyncDictionary<int, BaseGameEvent> _startedEvents = new();

        private readonly SyncDictionary<GameEventsType, int> _fixedEvents = new();

        private float _pipebreakChanceBoost;

        public SyncDictionary<int, BaseGameEvent> StartedEvents => _startedEvents;

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


        
        
        public event Action<SyncDictionary<int, BaseGameEvent>> OnEventReceived;

        public event Action<float> OnPipeBreakChanceBoostChanged;

        public override void OnStartClient()
        {
            OnEventReceived?.Invoke(StartedEvents);
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

        [Server]
        public void TriggerManyGameEvents(int eventCount = 1)
        {
            for (var i = 0; i < eventCount; i++)
            {
                var scIds = _sceneEvents.Keys;
                var randId = scIds.OrderBy(x => Random.value >= 0.5f).First();

                var gameEvent = _sceneEvents[randId];

                if (gameEvent.IsEventActive) continue;

                ActivateEvent(gameEvent.EventId);
            }
        }

        [Server]
        public void TriggerEventByType(GameEventsType eventType)
        {
            var gameEvent = _sceneEvents.Values.First(x => x.EventType == eventType);

            if (gameEvent == null || gameEvent.IsEventActive) return;

            ActivateEvent(gameEvent.EventId);
        }
    }
}