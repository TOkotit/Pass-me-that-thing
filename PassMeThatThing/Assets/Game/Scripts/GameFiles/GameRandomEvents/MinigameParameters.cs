using System;
using Game.Scripts.Enums;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.GameRandomEvents
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

        public Vector3 position;
        public Quaternion rotation;
    }
}