using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.LevelGeneration.Editor_Grid;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public struct ConnectionPoint
    {
        public Vector3Int GlobalPosition;
        public Vector3Int Direction;
        public RoomsConnectionTypes Type;
    }

    public class PlacedRoomData
    {
        public LevelRoom Instance;
        public LevelRoom Prefab;
        public List<ConnectionPoint> FreeConnections = new();
        public List<Vector3Int> OccupiedCells = new();
    }

    public class LevelPlacementOrchestrator : MonoBehaviour
    {
        [SerializeField] private RoomDatabase roomDatabase;
        [SerializeField] private LevelGrid levelGrid;
        [SerializeField] private Transform levelContainer;

        private Dictionary<RoomNode, PlacedRoomData> _placedRooms = new();
        private Dictionary<LevelRoom, int> _prefabUsage = new();
        private System.Random _random = new();


        public void GeneratePhysicalLevel(RoomNode hubNode)
        {
            LogLogicalGraphStatistics(hubNode);
            
            levelGrid.ClearGrid();
            _placedRooms.Clear();
            _prefabUsage.Clear();

            if (levelContainer != null)
            {
                for (var i = levelContainer.childCount - 1; i >= 0; i--)
                {
                    var child = levelContainer.GetChild(i).gameObject;
                    if (Application.isPlaying) Destroy(child);
                    else DestroyImmediate(child);
                }
            }

            Debug.Log($"================ СТАРТ ГЕНЕРАЦИИ ================");

            var spineOrder = GetSpineBuildOrder(hubNode);
            Debug.Log($"[BuildOrder] Узлов хребта: {spineOrder.Count}");
    
            foreach (var step in spineOrder)
            {
                if (step.Node.Type == RoomType.Hub)
                {
                    if (!PlaceHub(step.Node)) 
                    {
                        Debug.LogError("[КРИТИЧЕСКИЙ СБОЙ] Не удалось разместить Hub.");
                        return;
                    }
                    continue;
                }

                if (!ExecuteSpinePlacementPipeline(step.Node, step.Parent))
                {
                    Debug.LogError($"[КРИТИЧЕСКИЙ СБОЙ] Хребет разорван на узле ID {step.Node.NodeId}. Генерация прервана.");
                    return; 
                }
            }

            GenerateSideRooms(hubNode);

            Debug.Log("<color=green>succses</color>");
        }

        private void LogLogicalGraphStatistics(RoomNode hubNode)
        {
            var allNodes = new List<RoomNode>();
            var queue = new Queue<RoomNode>();
            var visited = new HashSet<RoomNode>();

            queue.Enqueue(hubNode);
            visited.Add(hubNode);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                allNodes.Add(current);
                foreach (var conn in current.ConnectedNodes)
                {
                    if (visited.Add(conn)) queue.Enqueue(conn);
                }
            }
        }


        private List<(RoomNode Node, RoomNode Parent)> GetSpineBuildOrder(RoomNode hubNode)
        {
            var spineSequence = new List<(RoomNode Node, RoomNode Parent)>
            {
                (hubNode, null)
            };
            
            var current = hubNode;

            while (true)
            {
                var nextSpineNode = current.ConnectedNodes.FirstOrDefault(n => 
                    (n.Type == RoomType.Defense || n.Type == RoomType.Exit) && 
                    !spineSequence.Any(x => x.Node == n));

                if (nextSpineNode == null) 
                    break; 

                spineSequence.Add((nextSpineNode, current));
                current = nextSpineNode;
            }

            return spineSequence;
        }

        private bool PlaceHub(RoomNode hubNode)
        {
            var requiredDoors = hubNode.ConnectedNodes.Count;
            var candidates = roomDatabase.GetSuitableRooms(hubNode.Type, requiredDoors, null, false);
            
            if (candidates == null || candidates.Count == 0)
            {
                Debug.LogError($"[КРИТИЧЕСКАЯ ОШИБКА] Не найдено подходящей комнаты для типа Hub. Требуется дверей: {requiredDoors}.");
                return false;
            }

            var prefab = candidates[_random.Next(candidates.Count)];
            InstantiateAndRegisterRoom(hubNode, prefab, Vector3Int.zero, RoomRotation.Deg0);
            return true;
        }

        private bool ExecuteSpinePlacementPipeline(RoomNode nodeToPlace, RoomNode designatedParent)
        {
            if (!_placedRooms.ContainsKey(designatedParent)) return false;

            var parentData = _placedRooms[designatedParent];
            return TryPlaceRoom(nodeToPlace, parentData, false, true);
        }


        private void GenerateSideRooms(RoomNode hubNode)
        {
            var sideRoomsOrder = GetPrioritizedSideRoomsOrder(hubNode);
            var postponedEvents = new List<RoomNode>();

            foreach (var step in sideRoomsOrder)
            {
                var node = step.Node;
                var parent = step.Parent;
                bool isEvent = node.Type == RoomType.Event;
                
                if (!_placedRooms.ContainsKey(parent)) continue;

                bool success = TryPlaceRoom(node, _placedRooms[parent], false, false);
                
                if (!success)
                {
                    if (isEvent)
                    {
                        Debug.LogWarning($"[SideRooms] Ивент ID {node.NodeId} отложен на конец.");
                        postponedEvents.Add(node);
                    }
                    else
                    {
                        success = TryPlaceGlobal(node, false, false);
                        if (!success) Debug.LogWarning($"[ПРОПУСК] Боковая комната ID {node.NodeId} отброшена.");
                    }
                }
            }

            foreach (var eventNode in postponedEvents)
            {
                if (TryPlaceGlobal(eventNode, true, false)) continue;

                if (TryUpgradeRoomConnectionsAndPlace(eventNode)) continue;

                Debug.LogError($"[КРИТИЧЕСКИЙ СБОЙ] Ивент ID {eventNode.NodeId} не удалось разместить даже после апгрейда комнат.");
            }
        }
        
        private List<(RoomNode Node, RoomNode Parent)> GetPrioritizedSideRoomsOrder(RoomNode hubNode)
        {
            var order = new List<(RoomNode Node, RoomNode Parent)>();
            var visited = new HashSet<RoomNode>();
            
            var spine = GetSpineBuildOrder(hubNode);
            foreach (var s in spine) visited.Add(s.Node);

            var queue = new List<(RoomNode Node, RoomNode Parent)>();

            foreach (var spineNode in spine.Select(s => s.Node))
            {
                foreach (var child in spineNode.ConnectedNodes)
                {
                    if (!visited.Contains(child)) queue.Add((child, spineNode));
                }
            }

            while (queue.Count > 0)
            {
                queue = queue.OrderByDescending(q => HasEventInSubtree(q.Node, q.Parent)).ToList();

                var current = queue[0];
                queue.RemoveAt(0);

                if (visited.Add(current.Node))
                {
                    order.Add(current);
                    foreach (var child in current.Node.ConnectedNodes)
                    {
                        if (!visited.Contains(child)) queue.Add((child, current.Node));
                    }
                }
            }

            return order;
        }
        
        private bool HasEventInSubtree(RoomNode startNode, RoomNode parent)
        {
            if (startNode.Type == RoomType.Event) return true;
            foreach (var child in startNode.ConnectedNodes)
            {
                if (child == parent) continue;
                if (HasEventInSubtree(child, startNode)) return true;
            }
            return false;
        }

        private bool TryUpgradeRoomConnectionsAndPlace(RoomNode eventNode)
        {
            var sacrificeCandidates = _placedRooms
                .Where(kvp => kvp.Key.Type == RoomType.Regular || kvp.Key.Type == RoomType.Event)
                .OrderBy(kvp => kvp.Value.FreeConnections.Count)
                .ToList();

            foreach (var sacrifice in sacrificeCandidates)
            {
                var sacrificedNode = sacrifice.Key;
                int neededDoors = sacrifice.Value.Prefab.TotalDoors + 1; 

                DestroyAndUnregisterRoom(sacrificedNode);

                if (TryPlaceGlobal(sacrificedNode, false, false, neededDoors))
                {
                    if (TryPlaceGlobal(eventNode, true, false))
                    {
                        Debug.Log($"[Upgrade] Успешно: Комната ID {sacrificedNode.NodeId} заменена на вариант с {neededDoors}+ дверьми. Ивент ID {eventNode.NodeId} размещен.");
                        return true;
                    }
                }

                if (!_placedRooms.ContainsKey(sacrificedNode))
                {
                    TryPlaceGlobal(sacrificedNode, true, false);
                }
            }
            return false;
        }
        

        private bool TryPlaceRoom(RoomNode node, PlacedRoomData parentData, bool ignoreDoorCount, bool requireGate, int overrideRequiredDoors = -1)
        {
            var eventType = node.Type == RoomType.Event ? node.EventData?.EventType : null;
            
            var requiredDoors = overrideRequiredDoors > 0 ? overrideRequiredDoors : (ignoreDoorCount ? 1 : node.ConnectedNodes.Count);
            var candidates = roomDatabase.GetSuitableRooms(node.Type, requiredDoors, eventType, false);            
            
            if (candidates == null || candidates.Count == 0) return false;

            if (node.Type == RoomType.Defense)
            {
                candidates = candidates.Where(p => !_prefabUsage.TryGetValue(p, out var count) || count < 2).ToList();
                if (candidates.Count == 0) return false;
            }

            candidates = candidates.OrderBy(x => (x.TotalDoors + x.TotalGates)).ToList();

            int poolSize = candidates.Count < 4 ? candidates.Count : 4;
            if (poolSize > 1)
            {
                var topPool = candidates.Take(poolSize).OrderBy(x => _random.Next()).ToList();
                var remaining = candidates.Skip(poolSize);
                candidates = topPool.Concat(remaining).ToList();
            }

            var availableConnections = parentData.FreeConnections;
            if (requireGate)
            {
                availableConnections = availableConnections.Where(c => c.Type == RoomsConnectionTypes.Gate).ToList();
            }

            if (availableConnections.Count == 0) return false;

            foreach (var prefab in candidates)
            {
                foreach (var parentConn in availableConnections)
                {
                    for (var r = 0; r < 4; r++)
                    {
                        var currentRotation = (RoomRotation)r;
                        var virtualPlates = RoomRotationHelper.GetRotatedPlates(prefab, currentRotation);
                        var matchingConn = FindMatchingConnection(virtualPlates, parentConn, requireGate);

                        if (matchingConn.HasValue)
                        {
                            var targetGlobalCell = parentConn.GlobalPosition + parentConn.Direction;
                            var calculatedOrigin = targetGlobalCell - matchingConn.Value.LocalPosition;

                            if (RoomCollisionValidator.IsPlacementValid(levelGrid, prefab, currentRotation, calculatedOrigin))
                            {
                                var newRoomData = InstantiateAndRegisterRoom(node, prefab, calculatedOrigin, currentRotation);
                                parentData.FreeConnections.Remove(parentConn);
                                RemoveUsedConnection(newRoomData, targetGlobalCell, -parentConn.Direction);
                                return true;
                            }
                        }
                    }
                }
            }
            
            return false;
        }

        private bool TryPlaceGlobal(RoomNode nodeToPlace, bool ignoreDoorCount, bool requireGate, int overrideRequiredDoors = -1)
        {
            var allPlacedNodes = _placedRooms.Keys.OrderBy(x => _random.Next()).ToList();
            
            foreach (var potentialParent in allPlacedNodes)
            {
                var parentData = _placedRooms[potentialParent];
                if (parentData.FreeConnections.Count == 0) continue;

                if (TryPlaceRoom(nodeToPlace, parentData, ignoreDoorCount, requireGate, overrideRequiredDoors))
                {
                    return true;
                }
            }
            return false;
        }

        private void DestroyAndUnregisterRoom(RoomNode node)
        {
            if (!_placedRooms.TryGetValue(node, out var data)) return;

            foreach (var cell in data.OccupiedCells)
            {
                levelGrid.SetCellState(cell, false);
            }

            if (data.Instance != null)
            {
                Destroy(data.Instance.gameObject);
            }

            if (data.Prefab != null && _prefabUsage.ContainsKey(data.Prefab))
            {
                _prefabUsage[data.Prefab]--;
                if (_prefabUsage[data.Prefab] <= 0)
                {
                    _prefabUsage.Remove(data.Prefab);
                }
            }

            _placedRooms.Remove(node);
        }

        private PlacedRoomData InstantiateAndRegisterRoom(RoomNode node, LevelRoom prefab, Vector3Int origin, RoomRotation rotation)
        {
            var rotationQuaternion = GetRotationQuaternion(rotation);
            var centerWorldPos = levelGrid.UnityGrid.GetCellCenterWorld(origin);
            var baseWorldPos = levelGrid.UnityGrid.CellToWorld(origin);
            var worldPos = new Vector3(centerWorldPos.x, baseWorldPos.y, centerWorldPos.z);

            var instance = Instantiate(prefab, worldPos, rotationQuaternion, levelContainer);
            instance.name = $"Room_{node.NodeId}_{node.Type}";
            instance.DepthFromHub = node.DepthFromHub;
            
            var placedData = new PlacedRoomData { Instance = instance, Prefab = prefab };
            var virtualPlates = RoomRotationHelper.GetRotatedPlates(prefab, rotation);

            foreach (var plate in virtualPlates)
            {
                var globalPos = origin + plate.LocalPosition;
                
                levelGrid.SetCellState(globalPos, true);
                placedData.OccupiedCells.Add(globalPos);

                ExtractConnections(placedData.FreeConnections, plate, globalPos);
            }

            if (!_prefabUsage.ContainsKey(prefab))
            {
                _prefabUsage[prefab] = 0;
            }
            _prefabUsage[prefab]++;

            _placedRooms.Add(node, placedData);
            return placedData;
        }

        private (Vector3Int LocalPosition, RoomsConnectionTypes Type)? FindMatchingConnection(VirtualPlateData[] plates, ConnectionPoint parentConn, bool requireGate)
        {
            var targetDirection = -parentConn.Direction;
            
            foreach (var plate in plates)
            {
                foreach (var door in plate.Doors)
                {
                    if (door.GlobalDirection == targetDirection && door.Type == parentConn.Type)
                    {
                        if (requireGate && door.Type != RoomsConnectionTypes.Gate)
                        {
                            continue;
                        }
                        return (plate.LocalPosition, door.Type);
                    }
                }
            }
            return null;
        }

        private void ExtractConnections(List<ConnectionPoint> list, VirtualPlateData plate, Vector3Int globalPos)
        {
            foreach (var door in plate.Doors)
            {
                list.Add(new ConnectionPoint 
                { 
                    GlobalPosition = globalPos, 
                    Direction = door.GlobalDirection, 
                    Type = door.Type 
                });
            }
        }

        private void RemoveUsedConnection(PlacedRoomData roomData, Vector3Int globalPos, Vector3Int direction)
        {
            for (var i = 0; i < roomData.FreeConnections.Count; i++)
            {
                var conn = roomData.FreeConnections[i];
                if (conn.GlobalPosition == globalPos && conn.Direction == direction)
                {
                    roomData.FreeConnections.RemoveAt(i);
                    break;
                }
            }
        }

        private Quaternion GetRotationQuaternion(RoomRotation rotation)
        {
            return rotation switch
            {
                RoomRotation.Deg90 => Quaternion.Euler(0, 90, 0),
                RoomRotation.Deg180 => Quaternion.Euler(0, 180, 0),
                RoomRotation.Deg270 => Quaternion.Euler(0, 270, 0),
                _ => Quaternion.identity
            };
        }
    }
}