using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.Events
{
    
    [CreateAssetMenu(fileName = "GameEventDatabase", menuName = "GameEvents/Database")]
    public class GameEventsDatabase : ScriptableObject
    {
        public List<GameEventData> allEvents;
            
            public GameEventData GetEventByType(GameEventsType type) 
                => allEvents.FirstOrDefault(ev => ev.type == type);
            
            public GameEventData GetEventById(string id) 
                => allEvents.FirstOrDefault(ev => ev.eventName == id);
    }
}