using System;
using System.Collections.Generic;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class EventData
    {
        public GameEventsType EventType;
        public int Cost;
    }
    
    public class LevelMacroData
    {
        public int MinTotalRoomsWithoutHub = 15;
        public int MaxTotalRoomsWithoutHub = 25;

        public int MinExitsCount = 1;
        public int MaxExitsCount = 2;
        
        public int MinDefenseRoomsCount = 2;
        public int MaxDefenseRoomsCount = 5;

        public int SideRoomDepth = 4;
        public int MaxHubConnections = 3;
        public int MaxDefenseConnections = 4;
        public int MaxSideRoomConnections = 5;
        
        public int MinEventRoomsBudget = 100;
        public int MaxEventRoomsBudget = 200;
        
        public int MinEventRoomsCount = 2;
        
        [Range(0f, 1f)]
        public float EventRoomsPercentage = 0.3f;
        
        public List<GameEventsType> MandatoryEvents = new();
        
        public static readonly Dictionary<GameEventsType, int> EventCosts = new()
        {
            { GameEventsType.FloodBrokenPump, 30 },
            { GameEventsType.FloodPipeBreak, 40 },
            { GameEventsType.BlackoutBlowFuse, 35 },
            { GameEventsType.BlackoutCutWires, 50 }
        };
        
        [HideInInspector] public int TotalRoomsWithoutHub;
        [HideInInspector] public int ExitsCount;
        [HideInInspector] public int DefenseRoomsCount;
        [HideInInspector] public int EventRoomsBudget;
        [HideInInspector] public int TargetSideRoomsCount;
        [HideInInspector] public int TargetEventRoomsCount;

        public void CalculateRuntimeValues(System.Random random)
        {
            TotalRoomsWithoutHub = random.Next(MinTotalRoomsWithoutHub, MaxTotalRoomsWithoutHub + 1);
            ExitsCount = random.Next(MinExitsCount, MaxExitsCount + 1);
            DefenseRoomsCount = random.Next(MinDefenseRoomsCount, MaxDefenseRoomsCount + 1);
            EventRoomsBudget = random.Next(MinEventRoomsBudget, MaxEventRoomsBudget + 1);

            TargetEventRoomsCount = (int)Math.Round(TotalRoomsWithoutHub * EventRoomsPercentage);
            if (TargetEventRoomsCount < MinEventRoomsCount)
            {
                TargetEventRoomsCount = MinEventRoomsCount;
            }

            var minSideRooms = TotalRoomsWithoutHub / 2;
            TargetSideRoomsCount = random.Next(minSideRooms, TotalRoomsWithoutHub + 1);

            if (TargetEventRoomsCount > TargetSideRoomsCount)
            {
                TargetEventRoomsCount = TargetSideRoomsCount;
            }
        }
    }
}