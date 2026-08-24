using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class WireNetModel
    {
        private WireManager _wireManager;
        
        public int id;
        public List<int> nodesId = new();
        public List<int> portsId = new();

        private float availableQuantity;
        private float requiredQuantity;

        public bool IsNetWorking => availableQuantity >= requiredQuantity &&  availableQuantity != 0;

        public float AvailableQuantity => availableQuantity;
        public float RequiredQuantity => requiredQuantity;

        public WireNetModel(int id, WireManager wireManager)
        {
            this.id = id;
            _wireManager = wireManager;
        }

        public void AddWireNode(int wireNodeId)
        {
            nodesId.Add(wireNodeId);
            if (_wireManager.PortNodes.ContainsKey(wireNodeId))
            {
                portsId.Add(wireNodeId);
                Recalculate();
            }
        }

        public void RemoveWireNode(int wireNodeId)
        {
            nodesId.Remove(wireNodeId);
            if (_wireManager.PortNodes.ContainsKey(wireNodeId))
            {
                portsId.Remove(wireNodeId);
                Recalculate();
            }
        }

        public void Recalculate()
        {
            availableQuantity = 0f;
            requiredQuantity = 0f;
            
            foreach (var wireNode in portsId)
            {
                var port = _wireManager.PortNodes[wireNode];
                if (port.IsOn)
                {
                    availableQuantity += port.AvailableValue;
                    requiredQuantity += port.RequiredValue;
                }
            }

            foreach (var wireNode in portsId)
            {
               _wireManager.PortNodes[wireNode].OnWireNetWorkingStateChanged(IsNetWorking);
            }

            _wireManager.WireNetsData[id] = new WireNetNetworkData(availableQuantity, requiredQuantity);


            Debug.Log($"[W] Net {id} Value {availableQuantity}/{requiredQuantity}");
        }


    }
}