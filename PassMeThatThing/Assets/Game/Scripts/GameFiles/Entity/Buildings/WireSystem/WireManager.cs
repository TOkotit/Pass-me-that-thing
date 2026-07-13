using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

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
            
            nodeConnections.Add(_lastNodeIdCounter, null);
            
            Debug.Log($"[W] registered {wireNode.NodeId}");
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
                {
                    Debug.Log($"[W] LIMIT");
                    return;
                }
            }
            else
            {
                if (nodeConnections[firstNodeId].Count > 2)
                {
                    Debug.Log($"[W] LIMIT");
                    return;
                }
            }
            
            if (secondNode.IsSplitter)
            {
                if (nodeConnections[secondNodeId].Count > secondNode.SplitterConnLimit)
                {
                    Debug.Log($"[W] LIMIT");
                    return;
                }
            }
            else
            {
                if (nodeConnections[secondNodeId].Count > 2)
                {
                    Debug.Log($"[W] LIMIT");
                    return;
                }
            }

            if (firstNode.NetId == secondNode.NetId && secondNode.NetId != -1)
            {
                Debug.Log($"[W] NET LOOP / CONNECTED  NetId-{firstNode.NetId}");
                return;
            }
            
            nodeConnections[firstNodeId].Add(secondNodeId);
            nodeConnections[secondNodeId].Add(firstNodeId);
            
            if (firstNode.NetId == -1)
            {
                if (secondNode.NetId == -1)
                {
                    var wireNetId = CreateWireNet();
                    
                    firstNode.NetId = wireNetId;
                    secondNode.NetId = wireNetId;
                    
                    wireNets[wireNetId].AddWireNode(firstNodeId);
                    wireNets[wireNetId].AddWireNode(secondNodeId);
                }
                else
                {
                    //1 to 2.netId
                    
                    wireNets[secondNode.NetId].AddWireNode(firstNodeId);
                    
                    firstNode.NetId = secondNode.NetId;
                }
            }
            else //firstNode.NetId != -1
            {
                if (secondNode.NetId == -1)
                {
                    //2 to 1.netId
                    
                    wireNets[firstNode.NetId].AddWireNode(secondNodeId);
                    
                    secondNode.NetId = firstNode.NetId;
                }
                else //secondNode.NetId != -1
                {
                    //recalculate 1.net to 2.net
                    foreach (var nodeId in wireNets[firstNode.NetId].nodesId)
                    {
                        allNodes[nodeId].NetId = secondNode.NetId;
                    }
                }
            }

            PrintDebugInfo();
        }

        [Command(requiresAuthority = false)]
        public void CmdClearConnectionsOfNode(int nodeId)
        {
            ClearConnectionsOfNode(nodeId);
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
            
            //перезапись образовавшихся веток кроме первой
            for (int i=0; i < nodeConnections[nodeId].Count; i++)
            {
                if (i==0) continue;
                
                var wireNetId = CreateWireNet();
                AttachConnectedToNewWireNet(wireNetId, nodeConnections[nodeId][i], nodeId);
            }
            
            //clear others
            nodeConnections[nodeId].Clear();
            
            node.NetId = -1;
            
            PrintDebugInfo();
        }

        private void AttachConnectedToNewWireNet(int newWireNetId, int nodeId, int prevNodeId)
        {
            wireNets[allNodes[nodeId].NetId].RemoveWireNode(nodeId);
            
            allNodes[nodeId].NetId = newWireNetId;
            
            wireNets[newWireNetId].AddWireNode(nodeId);
            
            foreach (var connected in nodeConnections[nodeId])
            {
                if (connected != prevNodeId)
                    AttachConnectedToNewWireNet(newWireNetId, connected, nodeId);
            }
        }

        private void PrintDebugInfo()
        {
            Debug.Log("[W] " + string.Join(", ", allNodes
                .Select(n => $"{n.Value.NodeId} : {n.Value.NetId}")));
        }
    }
}