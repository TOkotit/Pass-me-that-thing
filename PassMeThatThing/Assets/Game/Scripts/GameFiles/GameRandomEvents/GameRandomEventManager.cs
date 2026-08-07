using System;
using System.Collections.Generic;
using Game.Scripts.Enums;
using Mirror;
using UnityEditor;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace Game.Scripts.GameFiles.GameRandomEvents
{
    public class GameRandomEventManager : NetworkBehaviour
    {
        private int _idGenerator = 1;
        private readonly SyncDictionary<int, BaseGameEvent> _sceneEvents = new();
        
        //ивенты которые запущены
        private readonly SyncDictionary<int, BaseGameEvent> _startedEvents = new();

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
                StartedEvents.Remove(eventId);
            }
            else
            {
                Debug.LogWarning($"[GameEventManager] Невозможно остановить: ивент с ID:{eventId} не найден на карте.");
            }
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
    }
}