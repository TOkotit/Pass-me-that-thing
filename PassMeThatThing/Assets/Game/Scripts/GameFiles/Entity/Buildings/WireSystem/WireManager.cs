using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Entity.Buildings.WireSystem
{
    public class WireManager : NetworkBehaviour
    {
        [SerializeField] private WireVisualizer wireVisualizer;
        [SerializeField] private LayerMask obstacleLayer;
        //[SerializeField] private float maxDistance;

        
        private SyncDictionary<int, WireNode> allNodes = new ();

        private Dictionary<int, WireNodePort> portNodes = new ();
        
        private Dictionary<int, List<int>> nodeConnections = new ();

        private Dictionary<int, WireNetModel> wireNets = new ();
        private SyncDictionary<int, WireNetNetworkData> _wireNetsData = new();

        private Dictionary<int, WireNodeEntry> _entryObjects = new();

        //firstNode/entryid, secondNode/entryId
        private List<(int, int)> _usedEntries = new ();

        private int _lastNodeIdCounter;
        private int _lastNetIdCounter;
        private int _lastEntryIdCounter;

        public SyncDictionary<int, WireNode> AllNodes => allNodes;
        public Dictionary<int, WireNodePort> PortNodes => portNodes;
        public Dictionary<int, List<int>> NodeConnections => nodeConnections;
        public Dictionary<int, WireNetModel> WireNets => wireNets;
        public SyncDictionary<int, WireNetNetworkData> WireNetsData => _wireNetsData;
        public Dictionary<int, WireNodeEntry> EntryObjects => _entryObjects;

        

        [Server]
        public void RegisterNode(WireNode wireNode)
        {
            _lastNodeIdCounter++;
            
            wireNode.NodeId = _lastNodeIdCounter;
            
            AllNodes[_lastNodeIdCounter] = wireNode;
            
            if (wireNode is WireNodePort port)
            {
                PortNodes[_lastNodeIdCounter] = port;
            }
            
            NodeConnections.Add(_lastNodeIdCounter, null);
            
            Debug.Log($"[W] registered {wireNode.NodeId}");
        }
        
        [Server]
        public void UnRegisterNode(int nodeId)
        {
            AllNodes[nodeId] = null;
            if (PortNodes.ContainsKey(nodeId))
            {
                PortNodes[nodeId] = null;
            }
        }

        [Server]
        public void RegisterEntry(WireNodeEntry entry)
        {
            _lastEntryIdCounter++;

            entry.EntryId = _lastEntryIdCounter;

            _entryObjects[_lastEntryIdCounter] = entry;
        }

        [Server]
        public void UnregisterEntry(int entryId)
        {
            _entryObjects.Remove(entryId);
        }

        [Server]
        public int CreateWireNet()
        {
            _lastNetIdCounter++;
            
            WireNets.Add(_lastNetIdCounter, new WireNetModel(_lastNetIdCounter, this));
            _wireNetsData.Add(_lastNetIdCounter, new WireNetNetworkData());

            return _lastNetIdCounter;
        }

        
        [Command(requiresAuthority = false)]
        public void CmdMakeConnection(int prevNodeId, int nextNodeId, int firstEntryId, int secondEntryId)
        {
            MakeConnection(prevNodeId, nextNodeId, firstEntryId, secondEntryId);
        }
        
        [Server]
        public void MakeConnection(int firstNodeId, int secondNodeId,
            int firstEntryId, int secondEntryId)
        {
            var firstNode =  AllNodes[firstNodeId];
            var secondNode =  AllNodes[secondNodeId];
            
            if (firstNode.WireType != secondNode.WireType) return;

            //// проверка  физики
            //var direction = (firstNode.transform.position - secondNode.transform.position).normalized;
            //if (Physics.Raycast(secondNode.transform.position, direction, 
            //        Vector3.Distance(firstNode.transform.position, secondNode.transform.position), obstacleLayer))
            //{
            //    Debug.Log($"[W] WALLS");
            //    return;
            //}
            
            if (NodeConnections[firstNodeId] == null)
                NodeConnections[firstNodeId] = new List<int>();

            if (NodeConnections[secondNodeId] == null)
                NodeConnections[secondNodeId] = new List<int>();

            //проверка на лимит

            if (NodeConnections[firstNodeId].Count >= firstNode.ConnLimit
                || NodeConnections[secondNodeId].Count >= secondNode.ConnLimit)
            {
                Debug.Log($"[W] LIMIT");
                return;
            }

            if (_usedEntries.Any(x => x.Item1 == firstEntryId 
            || x.Item2 == firstEntryId
            || x.Item1 == secondEntryId
            || x.Item2 == secondEntryId))
            {
                Debug.Log($"[W] Entries already in use");
                return;
            }


            if (firstNode.NetId == secondNode.NetId && secondNode.NetId != -1)
            {
                Debug.Log($"[W] NET LOOP / ALREADY CONNECTED  NetId-{firstNode.NetId}");
                return;
            }
            

            NodeConnections[firstNodeId].Add(secondNodeId);
            NodeConnections[secondNodeId].Add(firstNodeId);

            _usedEntries.Add((firstEntryId, secondEntryId));

            if (firstNode.NetId == -1)
            {
                if (secondNode.NetId == -1)
                {
                    var wireNetId = CreateWireNet();
                    
                    firstNode.NetId = wireNetId;
                    secondNode.NetId = wireNetId;
                    
                    WireNets[wireNetId].AddWireNode(firstNodeId);
                    WireNets[wireNetId].AddWireNode(secondNodeId);
                }
                else
                {
                    //1 to 2.netId
                    firstNode.NetId = secondNode.NetId;
                    WireNets[secondNode.NetId].AddWireNode(firstNodeId);
                }
            }
            else //firstNode.NetId != -1
            {
                if (secondNode.NetId == -1)
                {
                    //2 to 1.netId
                    secondNode.NetId = firstNode.NetId;
                    WireNets[firstNode.NetId].AddWireNode(secondNodeId);
                }
                else //secondNode.NetId != -1
                {
                    //recalculate 1.net to 2.net
                    var copy = WireNets[firstNode.NetId].nodesId.ToList();
                    
                    foreach (var nodeId in copy)
                    {
                        WireNets[firstNode.NetId].RemoveWireNode(nodeId);
                        AllNodes[nodeId].NetId = secondNode.NetId;
                        WireNets[secondNode.NetId].AddWireNode(nodeId);
                    }
                }
            }

            RpcDrawNodeLines(firstNode, secondNode,
                EntryObjects[firstEntryId],
                EntryObjects[secondEntryId]);

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
            var node = AllNodes[nodeId];
            
            if (node.NetId == -1) return;
            
            //clear self from others
            foreach (var connection in NodeConnections[nodeId])
            {
                NodeConnections[connection].Remove(nodeId);
            }
            
            //перезапись образовавшихся веток кроме первой
            for (int i=0; i < NodeConnections[nodeId].Count; i++)
            {
                if (NodeConnections[NodeConnections[nodeId][i]].Count == 0)
                {
                    WireNets[AllNodes[NodeConnections[nodeId][i]].NetId]
                        .RemoveWireNode(NodeConnections[nodeId][i]);
                    AllNodes[NodeConnections[nodeId][i]].NetId = -1;
                    
                    continue;
                }
                
                var wireNetId = CreateWireNet();
                AttachConnectedToNewWireNet(wireNetId, NodeConnections[nodeId][i], nodeId);
            }
            
            //clear self
            NodeConnections[nodeId].Clear();

            foreach (var entry in node.Entries)
            {
                _usedEntries.RemoveAll(x => x.Item1 == entry.EntryId || x.Item2 == entry.EntryId);
            }

            WireNets[AllNodes[nodeId].NetId].RemoveWireNode(nodeId);
            node.NetId = -1;
            
            RpcClearNodeLines(node);
            
            PrintDebugInfo();
        }

        private void AttachConnectedToNewWireNet(int newWireNetId, int nodeId, int prevNodeId)
        {
            WireNets[AllNodes[nodeId].NetId].RemoveWireNode(nodeId);
            
            AllNodes[nodeId].NetId = newWireNetId;
            
            WireNets[newWireNetId].AddWireNode(nodeId);
            
            foreach (var connected in NodeConnections[nodeId])
            {
                if (connected != prevNodeId)
                    AttachConnectedToNewWireNet(newWireNetId, connected, nodeId);
            }
        }

        [ClientRpc]
        public void RpcDrawNodeLines(WireNode firstNode, WireNode secondNode,
            WireNodeEntry firstEntry, WireNodeEntry secondEntry)
        {
            wireVisualizer.DrawNodeLines(firstNode,
                secondNode,
                firstEntry,
                secondEntry);
        }
        
        [ClientRpc]
        public void RpcClearNodeLines(WireNode node)
        {
            wireVisualizer.ClearNodeLines(node);
        }

        

        private void PrintDebugInfo()
        {
            Debug.Log("[W] AllNodes" + string.Join("\n", AllNodes
                .Select(n => $"{n.Value.NodeId} : {n.Value.NetId}")));
            
            Debug.Log("[W] WireNets" + string.Join("\n", WireNets
                .Select(n => $"{n.Key} : {string.Join(", ", n.Value.nodesId)}")));

            Debug.Log("[W] usedEntries" + string.Join("\n", _usedEntries
                .Select(n => $"{n.Item1} - {n.Item2}")));
        }
    }
}