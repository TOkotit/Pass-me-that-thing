using System.Collections.Generic;
using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.LevelGeneration.Graph
{
    public class RoomNodeNew
    {
        public int NodeId; 
        public int DepthFromHub;
        
        public RoomTypeNew Type; 
        
        public List<RoomNodeNew> ConnectedNodes = new();
        
        public RoomNodeNew(int nodeId, RoomTypeNew type)
        {
            NodeId = nodeId;
            Type = type;
        }
        
        public void Connect(RoomNodeNew other)
        {
            if (!ConnectedNodes.Contains(other))
            {
                ConnectedNodes.Add(other);
                other.ConnectedNodes.Add(this);
            }
        }
    }
}