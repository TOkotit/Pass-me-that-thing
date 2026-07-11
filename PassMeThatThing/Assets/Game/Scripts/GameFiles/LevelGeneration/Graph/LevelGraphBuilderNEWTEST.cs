using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Game.Scripts.Enums;

namespace Game.Scripts.GameFiles.LevelGeneration.Graph
{
    public class LevelGraphBuilderNEWTEST
    {
        private int _nodeIdCounter = 0;
        private Random _random = new();

        public RoomNode BuildGraph(LevelMacroData macroData)
        {
            var hubNode = BuildLevelSpine(macroData);
            var sideRooms = GenerateSideRoomsPool(macroData);
            
            AttachSideRooms(hubNode, sideRooms, macroData);

            return hubNode;
        }
        
        public RoomNode BuildLevelSpine(LevelMacroData macroData)
        {
            _nodeIdCounter = 0;

            var levelSpine = new RoomNode(_nodeIdCounter++, RoomType.Hub);
            var currentNode = levelSpine;

            var defenseCount = macroData.DefenseRoomsCount;

            for (var i = 0; i < defenseCount; i++)
            {
                var defenseNode = new RoomNode(_nodeIdCounter++, RoomType.Defense);

                currentNode.ConnectedNodes.Add(defenseNode);
                defenseNode.ConnectedNodes.Add(currentNode);

                currentNode = defenseNode;
            }

            var exitNode = new RoomNode(_nodeIdCounter++, RoomType.Exit);

            currentNode.ConnectedNodes.Add(exitNode);
            exitNode.ConnectedNodes.Add(currentNode);

            return levelSpine;
        }
        
        public List<RoomNode> GenerateSideRoomsPool(LevelMacroData macroData)
        {
            var totalSideRooms = macroData.TotalRoomsWithoutHub;
            var purchasedEvents = new List<EventRoomDefinition>();
            
            if (macroData.MandatoryEvents != null)
            {
                purchasedEvents.AddRange(macroData.MandatoryEvents);
            }
            
            var minEvents = (int)Math.Ceiling(totalSideRooms * (1.0 / 6.0));
            var maxEvents = (int)Math.Floor(totalSideRooms * (1.0 / 5.0));
            
            var targetEventCount = Math.Max(1, _random.Next(minEvents, maxEvents + 1));
            var currentBudget = macroData.EventRoomsBudget;
            
            while (purchasedEvents.Count < targetEventCount)
            {
                var affordableEvents = macroData.AvailableEventsPool
                    .Where(e => e.Cost <= currentBudget)
                    .ToList();

                if (affordableEvents.Count == 0) break;

                var selectedEvent = affordableEvents[_random.Next(affordableEvents.Count)];
                purchasedEvents.Add(selectedEvent);
                currentBudget -= selectedEvent.Cost;
            }

            var sideRoomsPool = purchasedEvents.Select(ev => new RoomNode(_nodeIdCounter++, RoomType.Event, ev)).ToList();
    
            while (sideRoomsPool.Count < totalSideRooms) 
            {
                sideRoomsPool.Add(new RoomNode(_nodeIdCounter++, RoomType.Regular));
            }
    
            return sideRoomsPool;
        }
        
        private void AttachSideRooms(RoomNode hubNode, List<RoomNode> sideRoomsPool, LevelMacroData macroData)
        {
            var queue = new Queue<RoomNode>();
            var visited = new HashSet<RoomNode>();
            
            var attachmentPoints = new List<(RoomNode Node, int Depth)>();
            
            queue.Enqueue(hubNode);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                visited.Add(node);
                
                if (node.Type != RoomType.Exit) 
                {
                    attachmentPoints.Add((Node: node, Depth: 0));
                }
                
                foreach (var conn in node.ConnectedNodes.Where(c => !visited.Contains(c))) 
                {
                    queue.Enqueue(conn);
                }
            }

            var eventRooms = sideRoomsPool.Where(r => r.Type == RoomType.Event).ToList();
            var regularRooms = sideRoomsPool.Where(r => r.Type == RoomType.Regular).ToList();
            var combinedPool = new List<RoomNode>();
            
            var total = sideRoomsPool.Count;
            var eventCount = eventRooms.Count;
            var step = Math.Max(1, total / Math.Max(1, eventCount));
            
            
            for (var i = 0; i < total; i++)
            {
                if ((i % step == 0 || regularRooms.Count == 0) && eventRooms.Count > 0)
                {
                    combinedPool.Add(eventRooms[0]);
                    eventRooms.RemoveAt(0);
                }
                else if (regularRooms.Count > 0)
                {
                    combinedPool.Add(regularRooms[0]);
                    regularRooms.RemoveAt(0);
                }
            }
            
            foreach (var sideRoom in combinedPool)
            {
                var validPoints = attachmentPoints.Where(p => 
                    p.Node.ConnectedNodes.Count < GetMaxConnections(p.Node, macroData) && 
                    p.Depth < macroData.SideRoomDepth
                ).ToList();

                if (validPoints.Count == 0) break; 

                var targetPoint = validPoints[_random.Next(validPoints.Count)];

                targetPoint.Node.ConnectedNodes.Add(sideRoom);
                sideRoom.ConnectedNodes.Add(targetPoint.Node);

                attachmentPoints.Add((sideRoom, targetPoint.Depth + 1));
            }
        }
        
        private int GetMaxConnections(RoomNode node, LevelMacroData macroData)
        {
            return node.Type switch
            {
                RoomType.Hub => macroData.MaxHubConnections,
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