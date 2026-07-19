using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.LevelGeneration.Graph
{
    public class LevelGraphBuilder
    {
        private int _nodeIdCounter = 0;
        private Random _random = new();
        public int MaxDepth { get; private set; }

        public RoomNode BuildGraph(LevelMacroData macroData)
        {
            macroData.CalculateRuntimeValues(_random);
            var hubNode = BuildLevelSpine(macroData);
            var sideRooms = GenerateSideRoomsPool(macroData);
            
            AttachSideRooms(hubNode, sideRooms, macroData);
            var totalNodes = CountNodes(hubNode);

            if (totalNodes < macroData.TotalRoomsWithoutHub + 2)
            {
                UnityEngine.Debug.LogWarning(
                    $"Построено только {totalNodes} узлов из ожидаемых {macroData.TotalRoomsWithoutHub + 2}");
            }
            return hubNode;
        }
        private int CountNodes(RoomNode start)
        {
            var visited = new HashSet<RoomNode>();
            var queue = new Queue<RoomNode>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                foreach (var next in node.ConnectedNodes)
                {
                    if (visited.Add(next))
                        queue.Enqueue(next);
                }
            }

            return visited.Count;
        }
        
        public RoomNode BuildLevelSpine(LevelMacroData macroData)
        {
            _nodeIdCounter = 0;

            var levelSpine = new RoomNode(_nodeIdCounter++, RoomType.Hub);
            var currentNode = levelSpine;

            for (var i = 0; i < macroData.DefenseRoomsCount; i++)
            {
                var defenseNode = new RoomNode(_nodeIdCounter++, RoomType.Defense);
                currentNode.ConnectedNodes.Add(defenseNode);
                defenseNode.ConnectedNodes.Add(currentNode);
                currentNode = defenseNode;
            }

            for (var i = 0; i < macroData.ExitsCount; i++)
            {
                var exitNode = new RoomNode(_nodeIdCounter++, RoomType.Exit);
                currentNode.ConnectedNodes.Add(exitNode);
                exitNode.ConnectedNodes.Add(currentNode);
            }

            return levelSpine;
        }
        
       public List<RoomNode> GenerateSideRoomsPool(LevelMacroData macroData)
        {
            var sideRoomsPool = new List<RoomNode>();
            var purchasedEvents = new List<EventData>();
            var currentBudget = macroData.EventRoomsBudget;

            foreach (var mandatoryType in macroData.MandatoryEvents)
            {
                var cost = LevelMacroData.EventCosts.TryGetValue(mandatoryType, out var c) ? c : 0;
                purchasedEvents.Add(new EventData { EventType = mandatoryType, Cost = cost });
                currentBudget -= cost;
            }

            if (currentBudget > 0)
            {
                var availableForPurchase = LevelMacroData.EventCosts
                    .Where(kvp => kvp.Value > 0 && kvp.Key != GameEventsType.None)
                    .ToList();

                var safetyCounter = 1000; 
                while (currentBudget > 0 && purchasedEvents.Count < macroData.TargetEventRoomsCount && safetyCounter-- > 0)
                {
                    var affordable = availableForPurchase.Where(kvp => kvp.Value <= currentBudget).ToList();
                    if (affordable.Count == 0) break; 

                    var prospectiveTotalEvents = purchasedEvents.Count + 1;
                    var maxAllowedOfOneType = (int)Math.Ceiling(prospectiveTotalEvents / 2.0);

                    var validCandidates = affordable.Where(kvp => 
                    {
                        var currentCount = purchasedEvents.Count(p => p.EventType == kvp.Key);
                        return (currentCount + 1) <= maxAllowedOfOneType;
                    }).ToList();

                    if (validCandidates.Count == 0) break; 

                    var selected = validCandidates[_random.Next(validCandidates.Count)];

                    purchasedEvents.Add(new EventData { EventType = selected.Key, Cost = selected.Value });
                    currentBudget -= selected.Value;
                }
            }

            foreach (var ev in purchasedEvents)
            {
                sideRoomsPool.Add(new RoomNode(_nodeIdCounter++, RoomType.Event, ev));
            }

            while (sideRoomsPool.Count < macroData.TargetSideRoomsCount) 
            {
                sideRoomsPool.Add(new RoomNode(_nodeIdCounter++, RoomType.Regular));
            }

            sideRoomsPool = sideRoomsPool.OrderBy(x => _random.Next()).ToList();
            return sideRoomsPool;
        }
        
       private void AttachSideRooms(RoomNode hubNode, List<RoomNode> sideRoomsPool, LevelMacroData macroData)
        {
            var queue = new Queue<RoomNode>();
            var visited = new HashSet<RoomNode>();
            var attachmentPoints = new List<(RoomNode Node, int Depth)>();
            
            queue.Enqueue(hubNode);
            var depthMap = new Dictionary<RoomNode, int> { [hubNode] = 0 };
            visited.Add(hubNode);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                var depth = depthMap[node];

                if (node.Type != RoomType.Exit && node.Type != RoomType.Hub)
                    attachmentPoints.Add((node, depth));

                foreach (var conn in node.ConnectedNodes)
                {
                    if (visited.Add(conn))
                    {
                        depthMap[conn] = depth + 1;
                        if (depth + 1 > MaxDepth) { MaxDepth = depth + 1;}
                        queue.Enqueue(conn);
                    }
                }
            }
            
            foreach (var kvp in depthMap)
            {
                kvp.Key.DepthFromHub = kvp.Value;
            }

            foreach (var sideRoom in sideRoomsPool)
            {
                var validPoints = attachmentPoints.Where(p => 
                    p.Node.ConnectedNodes.Count < GetMaxConnections(p.Node, macroData) && 
                    p.Depth < macroData.SideRoomDepth
                ).ToList();

                if (validPoints.Count == 0) break; 

                var targetPoint = validPoints[_random.Next(validPoints.Count)];
                sideRoom.DepthFromHub = targetPoint.Depth + 1;
                if (sideRoom.DepthFromHub > MaxDepth)
                    MaxDepth = sideRoom.DepthFromHub;
                targetPoint.Node.ConnectedNodes.Add(sideRoom);
                sideRoom.ConnectedNodes.Add(targetPoint.Node);
                attachmentPoints.Add((sideRoom, targetPoint.Depth + 1));
            }
        }
        
        private int GetMaxConnections(RoomNode node, LevelMacroData macroData)
        {
            return node.Type switch
            {
                RoomType.Hub => 1,
                RoomType.Defense => macroData.MaxDefenseConnections,
                RoomType.Exit => 1, 
                RoomType.Regular => macroData.MaxSideRoomConnections,
                RoomType.Event => macroData.MaxSideRoomConnections,
                _ => 1
            };
        }
        
        public string GetGraphStructureString(RoomNode startNode)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== СТРУКТУРА ГРАФА УРОВНЯ (ЧИСТАЯ ТОПОЛОГИЯ) ===");
            
            var visited = new HashSet<RoomNode>();
            var queue = new Queue<RoomNode>();

            queue.Enqueue(startNode);
            visited.Add(startNode);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                var nodeInfo = $"[ID: {node.NodeId} | {node.Type}]";
                if (node.Type == RoomType.Event && node.EventData != null)
                {
                    nodeInfo = $"[ID: {node.NodeId} | {node.Type} ({node.EventData.EventType})]";
                }

                var connectedIds = node.ConnectedNodes.Select(n => n.NodeId.ToString()).ToList();
                var connectionsInfo = connectedIds.Count > 0 ? string.Join(", ", connectedIds) : "Нет связей";

                sb.AppendLine($"{nodeInfo,-40} --> Связи (ID): {connectionsInfo}");

                foreach (var connectedNode in node.ConnectedNodes)
                {
                    if (!visited.Contains(connectedNode))
                    {
                        visited.Add(connectedNode);
                        queue.Enqueue(connectedNode);
                    }
                }
            }
            sb.AppendLine("==============================");
            return sb.ToString();
        }
    }
}