using System.Collections.Generic;
using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.LevelGeneration.Graph
{
    public class RoomNode
    {
        public int NodeId; 
        
        public RoomType Type; 
        
        public EventData EventData; 
        
        public List<RoomNode> ConnectedNodes = new List<RoomNode>();
        
        public RoomNode(int nodeId, RoomType type, EventData eventData = null)
        {
            NodeId = nodeId;
            Type = type;
            EventData = eventData;
        }
    }
}