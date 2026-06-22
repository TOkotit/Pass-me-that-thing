using System.Collections.Generic;
using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.LevelGeneration.Graph
{
    public class RoomNode
    {
        public int NodeId; 
        
        public RoomType Type; 
        
        public EventRoomDefinition EventData; 
        
        public List<RoomNode> ConnectedNodes = new List<RoomNode>();
        
        public int X;
        public int Y;
        
        public RoomNode(int nodeId, RoomType type, int x = 0, int y = 0, EventRoomDefinition eventData = null)
        {
            NodeId = nodeId;
            Type = type;
            X = x;
            Y = y;
            EventData = eventData;
        }
    }
}