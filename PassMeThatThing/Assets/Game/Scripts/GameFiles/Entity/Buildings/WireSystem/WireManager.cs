using System.Collections.Generic;
using Mirror;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class WireManager : NetworkBehaviour
    {
        private Dictionary<int, WireNode> allNodes = new ();
        
        private Dictionary<int, List<int>> nodeConnections = new ();
        
        private Dictionary<int, WireNetModel> wireNets = new ();
        
        private int _lastNodeIdCounter;
        private int _lastNetIdCounter;

        

        
        
        [Server]
        public void RegisterNode(WireNode wireNode)
        {
            _lastNodeIdCounter++;
            
            wireNode.NodeId = _lastNodeIdCounter;
            allNodes[_lastNodeIdCounter] = wireNode;
        }
        
        [Server]
        public void UnRegisterNode(int nodeId)
        {
            allNodes[nodeId] = null;
        }

        [Server]
        public int CreateWireNet()
        {
            _lastNetIdCounter++;
            
            wireNets.Add(_lastNetIdCounter, new WireNetModel(_lastNetIdCounter));

            return _lastNetIdCounter;
        }
        
        [Command(requiresAuthority = false)]
        public void CmdMakeConnection(int prevNodeId, int nextNodeId)
        {
            MakeConnection(prevNodeId, nextNodeId);
        }
        
        [Server]
        public void MakeConnection(int firstNodeId, int secondNodeId)
        {
            var firstNode =  allNodes[firstNodeId];
            var secondNode =  allNodes[secondNodeId];
            
            if (nodeConnections[firstNodeId] == null)
                nodeConnections[firstNodeId] = new List<int>();

            if (nodeConnections[secondNodeId] == null)
                nodeConnections[secondNodeId] = new List<int>();

            //проверка на лимит
            if (firstNode.IsSplitter)
            {
                if (nodeConnections[firstNodeId].Count > firstNode.SplitterConnLimit)
                    return;
            }
            else
            {
                if (nodeConnections[firstNodeId].Count > 2)
                    return;
            }
            
            if (secondNode.IsSplitter)
            {
                if (nodeConnections[secondNodeId].Count > secondNode.SplitterConnLimit)
                    return;
            }
            else
            {
                if (nodeConnections[secondNodeId].Count > 2)
                    return;
            }
            
            nodeConnections[firstNodeId].Add(secondNodeId);
            nodeConnections[secondNodeId].Add(firstNodeId);
            
            //
            if (firstNode.NetId == -1)
            {
                if (secondNode.NetId == -1)
                {
                    var netId = CreateWireNet();
                    
                    firstNode.NetId = netId;
                    secondNode.NetId = netId;
                    
                    wireNets[netId].AddWireNode(firstNodeId);
                    wireNets[netId].AddWireNode(secondNodeId);
                }
                else
                {
                    //1 to 2.netId
                    wireNets[firstNode.NetId].RemoveWireNode(firstNodeId);
                    wireNets[secondNode.NetId].AddWireNode(firstNodeId);
                    
                    firstNode.NetId = secondNode.NetId;
                }
            }
            else //firstNode.NetId != -1
            {
                if (secondNode.NetId == -1)
                {
                    //2 to 1.netId
                    secondNode.NetId = firstNode.NetId;
                }
                else //secondNode.NetId != -1
                {
                    //recalculate 1.net to 2.net
                }
            }
        }

        [Server]
        public void ClearConnectionsOfNode(int nodeId)
        {
            var node = allNodes[nodeId];

            //clear self from others
            foreach (var connection in nodeConnections[nodeId])
            {
                nodeConnections[connection].Remove(nodeId);
            }
            
            //clear others
            nodeConnections[nodeId].Clear();
            
            node.NetId = -1;
        }
    }
}