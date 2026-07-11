using System.Collections.Generic;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class LevelMacroData
    {
        public int TotalRoomsWithoutHub { get; private set; }
        public int ExitsCount { get; private set; }
        public int DefenseRoomsCount { get; private set; }
        public int? NormalRoomsCount { get; private set; }
        public int? EventRoomsCount { get; private set; }
        public int EventRoomsBudget { get; private set; }
        public int SideRoomDepth { get; private set; } = 2;
        public int MaxHubConnections { get; private set; } = 5;
        public int MaxDefenseConnections { get; private set; } = 6;
        public int MaxSideRoomConnections { get; private set; } = 5;
            
        public List<EventRoomDefinition> AvailableEventsPool { get; private set; }
        public List<EventRoomDefinition> MandatoryEvents { get; private set; }

        public LevelMacroData(
            int totalRoomsWithoutHub,
            int exitsCount,
            int defenseRoomsCount,
            int eventRoomsBudget,
            List<EventRoomDefinition> availableEventsPool,
            int? normalRoomsCount = null,
            int? eventRoomsCount = null,
            List<EventRoomDefinition> mandatoryEvents = null,
            int sideRoomDepth = 2,
            int maxHubConnections = 5,
            int maxDefenseConnections = 6,
            int maxSideRoomConnections = 5)
        {
            TotalRoomsWithoutHub = totalRoomsWithoutHub;
            ExitsCount = exitsCount;
            DefenseRoomsCount = defenseRoomsCount;
            EventRoomsBudget = eventRoomsBudget;
            AvailableEventsPool = availableEventsPool ?? new List<EventRoomDefinition>();
            NormalRoomsCount = normalRoomsCount;
            EventRoomsCount = eventRoomsCount;
            MandatoryEvents = mandatoryEvents ?? new List<EventRoomDefinition>();
            SideRoomDepth = sideRoomDepth;
            
            MaxHubConnections = maxHubConnections;
            MaxDefenseConnections = maxDefenseConnections;
            MaxSideRoomConnections = maxSideRoomConnections;
            
        }
    }
}