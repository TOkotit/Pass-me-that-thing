using System.Collections.Generic;
using Game.Scripts.Enums;
using Mirror;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Events
{
    public class GameEventManager : NetworkBehaviour
    {
        private int _idGenerator = 1;
        private readonly SyncDictionary<int, BaseGameEvent> _activeEvents = new();
        
        public SyncDictionary<int, BaseGameEvent> ActiveEvents => _activeEvents;
        
        
        [Server]
        public int RegisterSceneEvent(BaseGameEvent gameEvent)
        {
            var assignedId = _idGenerator;
            
            _idGenerator++; 
            
            _activeEvents.Add(assignedId, gameEvent);
            
            return assignedId;
        }
        
        [Server]
        public BaseGameEvent GetEventById(int id)
        {
            if (_activeEvents.TryGetValue(id, out var foundEvent))
            {
                return foundEvent;
            }
            
            Debug.LogWarning($"[GameEventManager] Ивент с ID {id} не найден!");
            return null;
        }
        
        [Server]
        public void ActivateEvent(int eventId)
        {
            if (_activeEvents.TryGetValue(eventId, out var gameEvent))
            {
                gameEvent.StartEvent();
            }
            else
            {
                Debug.LogWarning($"[GameEventManager] Невозможно запустить: ивент с ID:{eventId} не найден на карте.");
            }
        }
        [Server]
        public void UnregisterEvent(int id)
        {
            if (_activeEvents.ContainsKey(id))
            {
                _activeEvents.Remove(id);
            }
        }

        [Server]
        public void DisableEvent(int eventId)
        {
            if (_activeEvents.TryGetValue(eventId, out var gameEvent))
            {
                gameEvent.StopEvent();
            }
            else
            {
                Debug.LogWarning($"[GameEventManager] Невозможно остановить: ивент с ID:{eventId} не найден на карте.");
            }
        }
    }
}