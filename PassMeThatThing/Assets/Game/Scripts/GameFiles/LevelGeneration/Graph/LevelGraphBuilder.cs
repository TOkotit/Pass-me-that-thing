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

        private readonly (int x, int y)[] _directions = 
        {
            (0, 1),   // Вверх
            (0, -1),  // Вниз
            (1, 0),   // Вправо
            (-1, 0)   // Влево
        };
        
        
        public RoomNode BuildGraph(LevelMacroData macroData)
        {
            var occupiedPositions = new HashSet<(int x, int y)>();
            var hubNode = BuildLevelSpine(macroData, occupiedPositions);
            
            var sideRooms = GenerateSideRoomsPool(macroData);
            
            AttachSideRooms(hubNode, sideRooms, occupiedPositions);

            return hubNode;
        }
        
        public RoomNode BuildLevelSpine(LevelMacroData macroData, HashSet<(int x, int y)> occupiedPositions)
        {
            _nodeIdCounter = 0;

            // 1. Создаем Hub
            var hubNode = new RoomNode(_nodeIdCounter++, RoomType.Hub, 0, 0);
            occupiedPositions.Add((0, 0));

            // Направления для хребта
            var primaryDir = _directions[_random.Next(_directions.Length)];
            (int x, int y) bendDir = (primaryDir.y, primaryDir.x);
            if (_random.Next(2) == 0) bendDir = (-bendDir.x, -bendDir.y);

            var currentNode = hubNode;

            // 2. Строим Defense комнаты
            // Изгиб происходит, если комнат защиты >= 4
            int defenseCount = macroData.DefenseRoomsCount;
            int bendAt = defenseCount >= 4 ? defenseCount / 2 : -1;

            for (int i = 0; i < defenseCount; i++)
            {
                // Поворачиваем после середины списка Defense
                var currentDir = (i >= bendAt && bendAt != -1) ? bendDir : primaryDir;
        
                var nextX = currentNode.X + currentDir.x;
                var nextY = currentNode.Y + currentDir.y;

                var defenseNode = new RoomNode(_nodeIdCounter++, RoomType.Defense, nextX, nextY);
                occupiedPositions.Add((nextX, nextY));

                currentNode.ConnectedNodes.Add(defenseNode);
                defenseNode.ConnectedNodes.Add(currentNode);

                currentNode = defenseNode;
            }

            var lastDir = (defenseCount > 0 && defenseCount >= bendAt && bendAt != -1) ? bendDir : primaryDir;
    
            var exitX = currentNode.X + lastDir.x;
            var exitY = currentNode.Y + lastDir.y;

            var exitNode = new RoomNode(_nodeIdCounter++, RoomType.Exit, exitX, exitY);
            occupiedPositions.Add((exitX, exitY));

            currentNode.ConnectedNodes.Add(exitNode);
            exitNode.ConnectedNodes.Add(currentNode);

            return hubNode;
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

            var sideRoomsPool = purchasedEvents.Select(ev => new RoomNode(_nodeIdCounter++, RoomType.Event, 0, 0, ev)).ToList();
    
            while (sideRoomsPool.Count < totalSideRooms) 
            {
                sideRoomsPool.Add(new RoomNode(_nodeIdCounter++, RoomType.Regular, 0, 0));
            }
    
            return sideRoomsPool;
        }
        
        private void AttachSideRooms(RoomNode hubNode, List<RoomNode> sideRoomsPool, HashSet<(int x, int y)> occupiedPositions)
        {
            var defenseRooms = new List<RoomNode>();
            var queue = new Queue<RoomNode>();
            var visited = new HashSet<RoomNode>();
            RoomNode exitNode = null;
            
            queue.Enqueue(hubNode);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                visited.Add(node);
                
                if (node.Type == RoomType.Defense) defenseRooms.Add(node);
                if (node.Type == RoomType.Exit) exitNode = node; 
                
                foreach (var conn in node.ConnectedNodes.Where(c => !visited.Contains(c))) queue.Enqueue(conn);
            }

            var attachmentPoints = defenseRooms.Select(d => (Node: d, Depth: 0)).ToList();
            
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
                    ((p.Depth == 0 && p.Node.ConnectedNodes.Count < 4) || 
                     (p.Depth == 1 && p.Node.ConnectedNodes.Count < 2)) &&
                    _directions.Any(d => 
                    {
                        var tx = p.Node.X + d.x;
                        var ty = p.Node.Y + d.y;
                        
                        if (occupiedPositions.Contains((tx, ty))) return false;
                        if (Math.Abs(tx - hubNode.X) + Math.Abs(ty - hubNode.Y) == 1) return false;
                        if (exitNode != null && Math.Abs(tx - exitNode.X) + Math.Abs(ty - exitNode.Y) == 1) return false;
                        
                        return true;
                    })
                ).ToList();

                if (validPoints.Count == 0) continue; 

                var targetPoint = validPoints[_random.Next(validPoints.Count)];

                var freeDirections = _directions
                    .Where(d => 
                    {
                        var tx = targetPoint.Node.X + d.x;
                        var ty = targetPoint.Node.Y + d.y;
                        
                        if (occupiedPositions.Contains((tx, ty))) return false;
                        if (Math.Abs(tx - hubNode.X) + Math.Abs(ty - hubNode.Y) == 1) return false;
                        if (exitNode != null && Math.Abs(tx - exitNode.X) + Math.Abs(ty - exitNode.Y) == 1) return false;
                        
                        return true;
                    })
                    .ToList();
                
                var chosenDir = freeDirections[_random.Next(freeDirections.Count)];
                
                sideRoom.X = targetPoint.Node.X + chosenDir.x;
                sideRoom.Y = targetPoint.Node.Y + chosenDir.y;
                
                occupiedPositions.Add((sideRoom.X, sideRoom.Y));

                targetPoint.Node.ConnectedNodes.Add(sideRoom);
                sideRoom.ConnectedNodes.Add(targetPoint.Node);

                if (targetPoint.Depth == 0)
                {
                    attachmentPoints.Add((sideRoom, 1));
                }
            }
        }
        public string GetGraphStructureString(RoomNode startNode)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== СТРУКТУРА ГРАФА УРОВНЯ (С КООРДИНАТАМИ) ===");
            
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

                var posInfo = $"Поз: ({node.X}, {node.Y})";

                var connectedIds = node.ConnectedNodes.Select(n => n.NodeId.ToString()).ToList();
                var connectionsInfo = connectedIds.Count > 0 ? string.Join(", ", connectedIds) : "Нет связей";

                sb.AppendLine($"{nodeInfo,-40} {posInfo,-15} --> Связи (ID): {connectionsInfo}");

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