using System.Collections.Generic;
using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.LevelGeneration.Graph
{
    
    /// <summary>
    /// Кластер комнат<br/>
    /// Пока хранит только список нод
    /// </summary>
    public class RoomCluster
    {
        public int Id { get; }
        public List<RoomNode> Rooms { get; set; } = new();
        
        public RoomCluster(int id)
        {
            Id = id;
        }
        public void AddRoom(RoomNode node)
        {
            node.ClusterId = Id;
            Rooms.Add(node);
        }
        public RoomNode GetNodeById(int nodeId) => Rooms.Find(r => r.Id == nodeId);
        public bool ContainsType(RoomType type) => Rooms.Exists(r => r.Type == type);
    }
}