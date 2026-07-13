using System.Collections.Generic;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class WireNetModel
    {
        public int id;
        public List<int> nodesId = new();
        
        public float availableQuantity;
        public float requiredQuantity;

        public WireNetModel(int id)
        {
            this.id = id;
        }

        public void AddWireNode(int wireNodeId)
        {
            nodesId.Add(wireNodeId);
        }

        public void RemoveWireNode(int wireNodeId)
        {
            nodesId.Remove(wireNodeId);
        }
    }
}