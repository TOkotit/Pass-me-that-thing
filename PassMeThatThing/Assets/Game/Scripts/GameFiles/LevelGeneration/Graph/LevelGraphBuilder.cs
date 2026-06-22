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

            var hubNode = new RoomNode(_nodeIdCounter++, RoomType.Hub, 0, 0);
            occupiedPositions.Add((0, 0));
            
            var currentNode = hubNode;

            for (var i = 0; i < macroData.DefenseRoomsCount; i++)
            {
                var shuffledDirs = _directions.OrderBy(_ => _random.Next()).ToList();
                (int x, int y) chosenDir = (0, 0);
                var foundValidTile = false;

                foreach (var dir in shuffledDirs)
                {
                    var testX = currentNode.X + dir.x;
                    var testY = currentNode.Y + dir.y;

                    if (!occupiedPositions.Contains((testX, testY)))
                    {
                        // ПРОВЕРКА ИЗОЛЯЦИИ: Считаем, скольких УЖЕ ПОСТРОЕННЫХ комнат коснется новая.
                        // Должна касаться ТОЛЬКО одной (своей родительской currentNode).
                        int neighborsCount = _directions.Count(d => occupiedPositions.Contains((testX + d.x, testY + d.y)));
                        
                        if (neighborsCount == 1)
                        {
                            chosenDir = dir;
                            foundValidTile = true;
                            break;
                        }
                    }
                }
                
                // Фолбэк, если алгоритм загнал себя в угол (крайне редко, но нужно для страховки)
                if (!foundValidTile)
                {
                    foreach (var dir in shuffledDirs)
                    {
                        var tx = currentNode.X + dir.x;
                        var ty = currentNode.Y + dir.y;
                        if (!occupiedPositions.Contains((tx, ty)))
                        {
                            // Хотя бы защищаем Хаб от касания при фолбэке
                            bool touchesHub = Math.Abs(tx - hubNode.X) + Math.Abs(ty - hubNode.Y) == 1;
                            if (i > 0 && touchesHub) continue; 
                            
                            chosenDir = dir;
                            break;
                        }
                    }
                }

                var nextX = currentNode.X + chosenDir.x;
                var nextY = currentNode.Y + chosenDir.y;
                
                var defenseNode = new RoomNode(_nodeIdCounter++, RoomType.Defense, nextX, nextY);
                occupiedPositions.Add((nextX, nextY));
                
                currentNode.ConnectedNodes.Add(defenseNode);
                defenseNode.ConnectedNodes.Add(currentNode);
                
                currentNode = defenseNode;
            }

            // РАЗМЕЩЕНИЕ ВЫХОДА (с такой же проверкой изоляции)
            var exitDirs = _directions.OrderBy(_ => _random.Next()).ToList();
            var finalExitDir = _directions[_random.Next(_directions.Length)];
            var foundExit = false;

            foreach (var dir in exitDirs)
            {
                var testX = currentNode.X + dir.x;
                var testY = currentNode.Y + dir.y;
                if (!occupiedPositions.Contains((testX, testY)))
                {
                    int neighborsCount = _directions.Count(d => occupiedPositions.Contains((testX + d.x, testY + d.y)));
                    if (neighborsCount == 1)
                    {
                        finalExitDir = dir;
                        foundExit = true;
                        break;
                    }
                }
            }

            // Фолбэк для выхода
            if (!foundExit)
            {
                foreach (var dir in exitDirs)
                {
                    if (!occupiedPositions.Contains((currentNode.X + dir.x, currentNode.Y + dir.y)))
                    {
                        finalExitDir = dir;
                        break;
                    }
                }
            }
            
            var exitX = currentNode.X + finalExitDir.x;
            var exitY = currentNode.Y + finalExitDir.y;
            
            var exitNode = new RoomNode(_nodeIdCounter++, RoomType.Exit, exitX, exitY);
            occupiedPositions.Add((exitX, exitY));
            
            currentNode.ConnectedNodes.Add(exitNode);
            exitNode.ConnectedNodes.Add(currentNode);

            return hubNode;
        }
        
        public List<RoomNode> GenerateSideRoomsPool(LevelMacroData macroData)
        {
            var totalSideRooms = macroData.TotalRoomsWithoutHub;
            var targetEventCount = macroData.EventRoomsCount ?? 
                                   (macroData.NormalRoomsCount.HasValue ? totalSideRooms - macroData.NormalRoomsCount.Value : _random.Next(macroData.MandatoryEvents.Count, totalSideRooms + 1));
            targetEventCount = Math.Min(targetEventCount, totalSideRooms);

            var purchasedEvents = new List<EventRoomDefinition>(macroData.MandatoryEvents);
            var currentBudget = macroData.EventRoomsBudget;

            while (purchasedEvents.Count < targetEventCount)
            {
                var typeCounts = purchasedEvents.GroupBy(e => e.EventType).ToDictionary(g => g.Key, g => g.Count());
                var limit50Percent = Math.Max(1, targetEventCount / 2);
                var affordableEvents = macroData.AvailableEventsPool.Where(e => e.Cost <= currentBudget && (!typeCounts.ContainsKey(e.EventType) || typeCounts[e.EventType] < limit50Percent)).ToList();
                if (affordableEvents.Count == 0) break;
                var selectedEvent = affordableEvents[_random.Next(affordableEvents.Count)];
                purchasedEvents.Add(selectedEvent);
                currentBudget -= selectedEvent.Cost;
            }

            var sideRoomsPool = purchasedEvents.Select(ev => new RoomNode(_nodeIdCounter++, RoomType.Event, 0, 0, ev)).ToList();
            while (sideRoomsPool.Count < totalSideRooms) sideRoomsPool.Add(new RoomNode(_nodeIdCounter++, RoomType.Regular, 0, 0));
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
                if (node.Type == RoomType.Exit) exitNode = node; // Находим комнату выхода для проверок
                
                foreach (var conn in node.ConnectedNodes.Where(c => !visited.Contains(c))) queue.Enqueue(conn);
            }

            var shuffledPool = sideRoomsPool.OrderBy(x => _random.Next()).ToList();
            var attachmentPoints = defenseRooms.Select(d => (Node: d, Depth: 0)).ToList();

            foreach (var sideRoom in shuffledPool)
            {
                var validPoints = attachmentPoints.Where(p => 
                    ((p.Depth == 0 && p.Node.ConnectedNodes.Count < 4) || 
                     (p.Depth == 1 && p.Node.ConnectedNodes.Count < 2)) &&
                    _directions.Any(d => 
                    {
                        var tx = p.Node.X + d.x;
                        var ty = p.Node.Y + d.y;
                        
                        // Клетка должна быть свободна
                        if (occupiedPositions.Contains((tx, ty))) return false;
                        
                        // СТРОГАЯ ИЗОЛЯЦИЯ: не касаемся Хаба (Манхэттенское расстояние == 1)
                        if (Math.Abs(tx - hubNode.X) + Math.Abs(ty - hubNode.Y) == 1) return false;
                        
                        // СТРОГАЯ ИЗОЛЯЦИЯ: не касаемся Выхода
                        if (exitNode != null && Math.Abs(tx - exitNode.X) + Math.Abs(ty - exitNode.Y) == 1) return false;
                        
                        return true;
                    })
                ).ToList();

                if (validPoints.Count == 0) break; 

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

                string nodeInfo = $"[ID: {node.NodeId} | {node.Type}]";
                if (node.Type == RoomType.Event && node.EventData != null)
                {
                    nodeInfo = $"[ID: {node.NodeId} | {node.Type} ({node.EventData.EventType})]";
                }

                // Добавляем инфу о координатах комнаты на сетке
                string posInfo = $"Поз: ({node.X}, {node.Y})";

                var connectedIds = node.ConnectedNodes.Select(n => n.NodeId.ToString()).ToList();
                string connectionsInfo = connectedIds.Count > 0 ? string.Join(", ", connectedIds) : "Нет связей";

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