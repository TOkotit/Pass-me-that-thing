using System.Collections.Generic;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class LevelMacroBuilder
    {
        private int _defenseRoomsCount = 4;
        private int _sideRoomsCount = 10;
        private int _sideRoomDepth = 2;
        private int _budget = 100;
        
        private int _maxHubConnections = 5;
        private int _maxDefenseConnections = 6;
        private int _maxSideRoomConnections = 5;
        
        
        private List<EventRoomDefinition> _availableEvents;
        private List<EventRoomDefinition> _mandatoryEvents = new();

        public LevelMacroBuilder(List<EventRoomDefinition> pool) => _availableEvents = pool;
        
        public LevelMacroBuilder SetDefense(int count) { _defenseRoomsCount = count; return this; }
        public LevelMacroBuilder SetSideRooms(int count) { _sideRoomsCount = count; return this; }
        public LevelMacroBuilder SetDepth(int depth) { _sideRoomDepth = depth; return this; }
        public LevelMacroBuilder SetBudget(int budget) { _budget = budget; return this; }
        public LevelMacroBuilder AddMandatoryEvent(EventRoomDefinition ev) { _mandatoryEvents.Add(ev); return this; }
        
        public LevelMacroBuilder SetMaxHubConnections(int count) { _maxHubConnections = count; return this; }
        public LevelMacroBuilder SetMaxDefenseConnections(int count) { _maxDefenseConnections = count; return this; }
        public LevelMacroBuilder SetMaxSideRoomConnections(int count) { _maxSideRoomConnections = count; return this; }

        public LevelMacroData Build() => new LevelMacroData(
            _sideRoomsCount, 1, _defenseRoomsCount, _budget, 
            _availableEvents, null, null, _mandatoryEvents, _sideRoomDepth,
            _maxHubConnections, _maxDefenseConnections, _maxSideRoomConnections
        );
    }
}