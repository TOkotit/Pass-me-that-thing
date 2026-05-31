using System;
using Game.Scripts.Enums;
using Mirror;

namespace Game.Scripts.GameFiles.Events
{
    [Serializable]
    public struct MinigameParameters
    {
        public int eventId;
        public GameEventsType eventType;
        public string description;
        public int difficulty;
        public float timeLimit;
        public EventTerminal eventTerminal;
    }
}