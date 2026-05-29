using System;
using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.Events
{
    [Serializable]
    public struct MinigameParameters
    {
        public GameEventsType eventType;
        public string description;
        public int difficulty;
        public float timeLimit;
    }
}