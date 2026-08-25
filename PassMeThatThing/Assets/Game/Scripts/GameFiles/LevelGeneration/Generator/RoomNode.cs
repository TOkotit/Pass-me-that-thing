using System.Collections.Generic;
using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.LevelGeneration.Graph
{
    public class RoomNode
    {
        public int NodeId; 
        public int DepthFromHub;
        
        public RoomTypeNew Type; 
        
        public List<RoomNode> ConnectedNodes = new();
        
        public RoomNode(int nodeId, RoomTypeNew type)
        {
            NodeId = nodeId;
            Type = type;
        }
        
        public void Connect(RoomNode other)
        {
            if (!ConnectedNodes.Contains(other))
            {
                ConnectedNodes.Add(other);
                other.ConnectedNodes.Add(this);
            }
        }
    }
}