using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class EventRoomDefinition
    {
        public string EventId;
        public GameEventsType EventType;
        public int Cost;

        public EventRoomDefinition(string eventId, GameEventsType eventType, int cost)
        {
            EventId = eventId;
            EventType = eventType;
            Cost = cost;
        }
    }
}