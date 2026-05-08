using System.Collections.Generic;
using Game.Scripts.Enums;
using Mirror;
using UnityEditor;
using UnityEngine;

namespace Game.Scripts.GameFiles.Events
{
    public class GameEventManager : NetworkBehaviour
    {
        [SerializeField] private GameEventsDatabase _database;
        
        private readonly Dictionary<GameEventsType, ComplexNetworkEvent> _sceneEvents = new();
        
        public GameEventData GetGameEventData(GameEventsType type) => _database.GetEventByType(type);
        
        public void RegisterSceneEvent(ComplexNetworkEvent ev)
        {
            if (!_sceneEvents.ContainsKey(ev.eventType))
                _sceneEvents.Add(ev.eventType, ev);
        }
        
        [Server]
        public void ActivateEvent(GameEventsType type)
        {
            
            Debug.Log($"ActivateEvent: type={type}");
            
            if (_sceneEvents.TryGetValue(type, out var sceneEvent))
            {
                Debug.Log($"Find event on scene, starting...");
                sceneEvent.StartEvent();
                return;
            }
            
            Debug.Log($"Didn't find event on scene, starting...");

            var data = _database.GetEventByType(type);
            if (data != null && data.eventPrefab != null)
            {
                var go = Instantiate(data.eventPrefab);
                NetworkServer.Spawn(go);
            
                if (go.TryGetComponent(out ComplexNetworkEvent ev))
                    ev.StartEvent();
            }
        }

        [Server]
        public void DisableEvent(GameEventsType gameEventType)
        {
            if (_sceneEvents.TryGetValue(gameEventType, out var sceneEvent))
            {
                Debug.Log($"Stopping scene event: {gameEventType}");
                sceneEvent.StopEvent();
            }
            else
            {
              
                var activeEvent = FindObjectOfType<ComplexNetworkEvent>();
                if (activeEvent != null && activeEvent.eventType == gameEventType)
                {
                    activeEvent.StopEvent();
                }
            }
        }
    }
}