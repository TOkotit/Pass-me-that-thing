using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.Events
{
    public class GameEventData : ScriptableObject
    {
        public string eventName;
        public Sprite icon;
        public GameEventsType type;
        
        public GameObject eventPrefab;
    }
}