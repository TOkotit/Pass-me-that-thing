using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.LevelGeneration.Editor_Grid;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public struct ConnectionPointNew
    {
        public Vector3Int GlobalPosition;
        public Vector3Int Direction;
        public RoomsConnectionTypes Type;
    }

    public class PlacedRoomDataNew
    {
        public LevelRoomNew Instance;
        public LevelRoomNew Prefab;
        public List<ConnectionPointNew> FreeConnections = new();
        public List<Vector3Int> OccupiedCells = new();
        public List<LevelRoomNew> AttachedTunnels = new(); // Добавлено
    }
    
    public class LevelPlacementOrchestratorNew : MonoBehaviour
    {
        [SerializeField] private RoomDatabaseNew roomDatabase;
        [SerializeField] private LevelGrid levelGrid;
        [SerializeField] private Transform levelContainer;

        private Dictionary<RoomNodeNew, PlacedRoomDataNew> _placedRooms = new();
        private System.Random _random = new();

        public void GeneratePhysicalLevel(RoomNodeNew hubNode)
        {
            levelGrid.ClearGrid();
            _placedRooms.Clear();

            if (levelContainer != null)
            {
                for (var i = levelContainer.childCount - 1; i >= 0; i--)
                {
                    var child = levelContainer.GetChild(i).gameObject;
                    if (Application.isPlaying) Destroy(child);
                    else DestroyImmediate(child);
                }
            }

            if (!PlaceHub(hubNode)) 
            {
                Debug.LogError("[СБОЙ] Не удалось разместить Hub.");
                return;
            }

            var queue = new Queue<(RoomNodeNew Node, RoomNodeNew Parent)>();
            var visited = new HashSet<RoomNodeNew> { hubNode };
            var cycleEdges = new List<(RoomNodeNew from, RoomNodeNew to)>();
            foreach (var child in hubNode.ConnectedNodes)
            {
                queue.Enqueue((child, hubNode));
                visited.Add(child);
            }

            while (queue.Count > 0)
            {
                var (current, parent) = queue.Dequeue();

                if (TryPlaceNode(current, parent))
                {
                    foreach (var child in current.ConnectedNodes)
                    {
                        if (child == parent) continue;

                        if (visited.Add(child))
                        {
                            queue.Enqueue((child, current));
                        }
                        else
                        {
                            cycleEdges.Add((current, child));
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[ПРОПУСК] Не удалось разместить узел ID {current.NodeId}");
                }
            }
            foreach (var edge in cycleEdges)
            {
                TryClosePhysicalCycle(edge.from, edge.to);
            }
        }

        private bool PlaceHub(RoomNodeNew hubNode)
        {
            var candidates = roomDatabase.GetSuitableRooms(hubNode.Type, 1, false);
            if (candidates == null || candidates.Count == 0) return false;

            var prefab = candidates[_random.Next(candidates.Count)];
            InstantiateAndRegisterRoom(hubNode, prefab, Vector3Int.zero, RoomRotation.Deg0, null);
            return true;
        }

        private bool TryPlaceNode(RoomNodeNew nodeToPlace, RoomNodeNew parentNode)
        {
            if (!_placedRooms.TryGetValue(parentNode, out var parentData)) return false;

            var candidates = roomDatabase.GetSuitableRooms(nodeToPlace.Type, 1, false)
                .OrderBy(x => _random.Next()).ToList();

            foreach (var prefab in candidates)
            {
                foreach (var parentConn in parentData.FreeConnections.ToList())
                {
                    for (var r = 0; r < 4; r++)
                    {
                        var rot = (RoomRotation)r;
                        var plates = RoomRotationHelper.GetRotatedPlates(prefab, rot);
                        var match = FindMatchingConnection(plates, parentConn);

                        if (match.HasValue)
                        {
                            var targetCell = parentConn.GlobalPosition + parentConn.Direction;
                            var origin = targetCell - match.Value.LocalPosition;

                            if (RoomCollisionValidator.IsPlacementValid(levelGrid, prefab, rot, origin))
                            {
                                InstantiateAndRegisterRoom(nodeToPlace, prefab, origin, rot, parentConn);
                                parentData.FreeConnections.Remove(parentConn);
                                return true;
                            }
                        }
                    }
                }
            }

            return TryPlaceGlobal(nodeToPlace);
        }

        private bool TryPlaceGlobal(RoomNodeNew nodeToPlace)
        {
            var candidates = roomDatabase.GetSuitableRooms((RoomTypeNew)nodeToPlace.Type, 1, false)
                .OrderBy(x => _random.Next()).ToList();

            var allPlaced = _placedRooms.Values.OrderBy(x => _random.Next()).ToList();

            foreach (var parentData in allPlaced)
            {
                foreach (var prefab in candidates)
                {
                    foreach (var parentConn in parentData.FreeConnections.ToList())
                    {
                        for (var r = 0; r < 4; r++)
                        {
                            var rot = (RoomRotation)r;
                            var plates = RoomRotationHelper.GetRotatedPlates(prefab, rot);
                            var match = FindMatchingConnection(plates, parentConn);

                            if (match.HasValue)
                            {
                                var targetCell = parentConn.GlobalPosition + parentConn.Direction;
                                var origin = targetCell - match.Value.LocalPosition;

                                if (RoomCollisionValidator.IsPlacementValid(levelGrid, prefab, rot, origin))
                                {
                                    InstantiateAndRegisterRoom(nodeToPlace, prefab, origin, rot, parentConn);
                                    parentData.FreeConnections.Remove(parentConn);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private PlacedRoomDataNew InstantiateAndRegisterRoom(RoomNodeNew node, LevelRoomNew prefab, Vector3Int origin, RoomRotation rotation, ConnectionPointNew? usedConnection)
        {
            var instance = InstantiateRoom(prefab, origin, rotation, $"Room_{node.NodeId}_{node.Type}");
            if (node != null) instance.DepthFromHub = node.DepthFromHub;

            var data = new PlacedRoomDataNew { Instance = instance, Prefab = prefab };
            var virtualPlates = RoomRotationHelper.GetRotatedPlates(prefab, rotation);

            Vector3Int? parentDoorCell = null;
            Vector3Int? parentDoorDir = null;

            if (usedConnection.HasValue)
            {
                parentDoorCell = usedConnection.Value.GlobalPosition + usedConnection.Value.Direction;
                parentDoorDir = -usedConnection.Value.Direction;
            }

            foreach (var plate in virtualPlates)
            {
                var globalPos = origin + plate.LocalPosition;
                levelGrid.SetCellState(globalPos, true);
                data.OccupiedCells.Add(globalPos);
            }

            foreach (var plate in virtualPlates)
            {
                var globalPos = origin + plate.LocalPosition;
                foreach (var door in plate.Doors)
                {
                    if (parentDoorCell.HasValue && globalPos == parentDoorCell.Value && door.GlobalDirection == parentDoorDir.Value)
                    {
                        continue;
                    }

                    data.FreeConnections.Add(new ConnectionPointNew 
                    { 
                        GlobalPosition = globalPos, 
                        Direction = door.GlobalDirection, 
                        Type = door.Type 
                    });
                }
            }

            if (node != null)
            {
                _placedRooms.Add(node, data);
            }
            TryConnectAdjacentDoors(data);
            return data;
        }
        
        private void TryConnectAdjacentDoors(PlacedRoomDataNew newRoomData)
        {
            foreach (var otherRoom in _placedRooms.Values)
            {
                if (otherRoom == newRoomData) continue;

                foreach (var connA in newRoomData.FreeConnections.ToList())
                {
                    foreach (var connB in otherRoom.FreeConnections.ToList())
                    {
                        if (connA.GlobalPosition + connA.Direction == connB.GlobalPosition &&
                            connB.Direction == -connA.Direction)
                        {
                            newRoomData.FreeConnections.Remove(connA);
                            otherRoom.FreeConnections.Remove(connB);
                            break;
                        }
                    }
                }
            }
        }

        private void TryClosePhysicalCycle(RoomNodeNew fromNode, RoomNodeNew toNode)
        {
            if (!_placedRooms.TryGetValue(fromNode, out var fromData)) return;
            if (!_placedRooms.TryGetValue(toNode, out var toData)) return;

            var tunnelPrefabs = roomDatabase.GetSuitableRooms(RoomTypeNew.TechnicalTunnels, 1, false)
                .OrderBy(x => _random.Next()).ToList();

            if (tunnelPrefabs.Count == 0) return;

            foreach (var fromConn in fromData.FreeConnections.ToList())
            {
                foreach (var toConn in toData.FreeConnections.ToList())
                {
                    foreach (var prefab in tunnelPrefabs)
                    {
                        for (int r = 0; r < 4; r++)
                        {
                            var rot = (RoomRotation)r;
                            var plates = RoomRotationHelper.GetRotatedPlates(prefab, rot);
                            
                            var matchFrom = FindMatchingConnection(plates, fromConn);
                            if (matchFrom.HasValue)
                            {
                                var targetCell = fromConn.GlobalPosition + fromConn.Direction;
                                var origin = targetCell - matchFrom.Value.LocalPosition;

                                // Проверяем: совпадает ли ДРУГОЙ конец этого туннеля с дверью целевой комнаты?
                                bool bridgesGap = false;
                                foreach (var p in plates)
                                {
                                    var globalPos = origin + p.LocalPosition;
                                    foreach (var d in p.Doors)
                                    {
                                        if (globalPos == toConn.GlobalPosition + toConn.Direction && 
                                            d.GlobalDirection == -toConn.Direction)
                                        {
                                            bridgesGap = true;
                                            break;
                                        }
                                    }
                                    if (bridgesGap) break;
                                }

                                if (bridgesGap && RoomCollisionValidator.IsPlacementValid(levelGrid, prefab, rot, origin))
                                {
                                    // Бинго! Туннель идеально встает между двумя комнатами
                                    var tunnelInstance = InstantiateRoom(prefab, origin, rot, "CycleRingTunnel");
                                    fromData.AttachedTunnels.Add(tunnelInstance);
                                    
                                    foreach (var p in plates)
                                    {
                                        levelGrid.SetCellState(origin + p.LocalPosition, true);
                                    }

                                    // Удаляем занятые двери из пула свободных выходов
                                    fromData.FreeConnections.Remove(fromConn);
                                    toData.FreeConnections.Remove(toConn);
                                    return; 
                                }
                            }
                        }
                    }
                }
            }
        }

        private LevelRoomNew InstantiateRoom(LevelRoomNew prefab, Vector3Int origin, RoomRotation rotation, string roomName)
        {
            var centerWorldPos = levelGrid.UnityGrid.GetCellCenterWorld(origin);
            var baseWorldPos = levelGrid.UnityGrid.CellToWorld(origin);
            var worldPos = new Vector3(centerWorldPos.x, baseWorldPos.y, centerWorldPos.z);
            
            var rotQuat = rotation switch
            {
                RoomRotation.Deg90 => Quaternion.Euler(0, 90, 0),
                RoomRotation.Deg180 => Quaternion.Euler(0, 180, 0),
                RoomRotation.Deg270 => Quaternion.Euler(0, 270, 0),
                _ => Quaternion.identity
            };

            var instance = Instantiate(prefab, worldPos, rotQuat, levelContainer);
            instance.name = roomName;
            return instance;
        }
        
        private (Vector3Int LocalPosition, RoomsConnectionTypes Type)? FindMatchingConnection(VirtualPlateData[] plates, ConnectionPointNew parentConn)
        {
            var targetDirection = -parentConn.Direction;
            foreach (var plate in plates)
            {
                foreach (var door in plate.Doors)
                {
                    if (door.GlobalDirection == targetDirection && door.Type == parentConn.Type)
                    {
                        return (plate.LocalPosition, door.Type);
                    }
                }
            }
            return null;
        }
    }
}