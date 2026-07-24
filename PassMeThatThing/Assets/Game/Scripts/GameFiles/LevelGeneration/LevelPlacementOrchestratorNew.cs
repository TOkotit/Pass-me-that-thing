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
        public List<LevelRoomNew> AttachedTunnels = new();
    }
    
    
    public class LevelPlacementOrchestratorNew : MonoBehaviour
    {
        [SerializeField] private RoomDatabaseNew roomDatabase;
        [SerializeField] private LevelGrid levelGrid;
        [SerializeField] private Transform levelContainer;

        private Dictionary<RoomNodeNew, PlacedRoomDataNew> _placedRooms = new();
        private System.Random _random = new();
        
        private static readonly Vector3Int[] SearchDirections = {
            new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1)
        };
        
        private class PathNodeNew
        {
            public Vector3Int Cell;
            public PathNodeNew Parent;
            public int Depth;
        }
        

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
                    }
                }
                else
                {
                    Debug.LogWarning($"[ПРОПУСК] Не удалось разместить узел ID {current.NodeId}");
                }
            }
            ConnectAllFreeExits();;
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

            var candidates = GetPrioritizedCandidates(nodeToPlace, parentData);

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
        
        private List<LevelRoomNew> GetPrioritizedCandidates(RoomNodeNew nodeToPlace, PlacedRoomDataNew parentData)
        {
            var rawCandidates = roomDatabase.GetSuitableRooms(nodeToPlace.Type, 1, false);
            if (rawCandidates == null || rawCandidates.Count == 0) 
                return new List<LevelRoomNew>();

           
            var isEndOrPenultimate = nodeToPlace.Type == RoomTypeNew.RecoveryHangar || 
                                     nodeToPlace.ConnectedNodes.Count <= 1 ||
                                     nodeToPlace.ConnectedNodes.Any(neighbor => neighbor.Type == RoomTypeNew.RecoveryHangar || neighbor.ConnectedNodes.Count <= 1);

            if (!isEndOrPenultimate)
            {
                return rawCandidates.OrderBy(_ => _random.Next()).ToList();
            }

            return rawCandidates
                .Select(prefab => new
                {
                    Prefab = prefab,
                    FreeExitsScore = CalculateFreeExitsScore(prefab, parentData),
                    RandomKey = _random.Next()
                })
                .OrderByDescending(x => x.FreeExitsScore)
                .ThenBy(x => x.RandomKey)
                .Select(x => x.Prefab)
                .ToList();
        }
        
        private int CalculateFreeExitsScore(LevelRoomNew prefab, PlacedRoomDataNew parentData)
        {
            int maxValidFreeDoors = -1;

            foreach (var parentConn in parentData.FreeConnections)
            {
                for (var r = 0; r < 4; r++)
                {
                    var rot = (RoomRotation)r;
                    var plates = RoomRotationHelper.GetRotatedPlates(prefab, rot);
                    var match = FindMatchingConnection(plates, parentConn);

                    if (!match.HasValue) continue;

                    var targetCell = parentConn.GlobalPosition + parentConn.Direction;
                    var origin = targetCell - match.Value.LocalPosition;

                    if (!RoomCollisionValidator.IsPlacementValid(levelGrid, prefab, rot, origin)) 
                        continue;

                    int freeDoorsToUnoccupied = 0;
                    Vector3Int entranceCell = targetCell;
                    Vector3Int entranceDir = -parentConn.Direction;

                    foreach (var plate in plates)
                    {
                        var plateGlobalPos = origin + plate.LocalPosition;
                        foreach (var door in plate.Doors)
                        {
                            if (plateGlobalPos == entranceCell && door.GlobalDirection == entranceDir)
                                continue;

                            var neighborCell = plateGlobalPos + door.GlobalDirection;
                            if (!levelGrid.IsCellOccupied(neighborCell))
                            {
                                freeDoorsToUnoccupied++;
                            }
                        }
                    }

                    if (freeDoorsToUnoccupied > maxValidFreeDoors)
                    {
                        maxValidFreeDoors = freeDoorsToUnoccupied;
                    }
                }
            }

            return maxValidFreeDoors;
        }
        private void ConnectAllFreeExits()
        {
            var allFreeConnections = new List<(PlacedRoomDataNew Room, ConnectionPointNew Conn)>();
            foreach (var kvp in _placedRooms)
            {
                foreach (var conn in kvp.Value.FreeConnections)
                {
                    allFreeConnections.Add((kvp.Value, conn));
                }
            }

            var tunnelPrefabs = roomDatabase.GetSuitableRooms(RoomTypeNew.TechnicalTunnels, 1, false);
            if (tunnelPrefabs == null || tunnelPrefabs.Count == 0) return;

            while (allFreeConnections.Count >= 2)
            {
                var startData = allFreeConnections[0];
                allFreeConnections.RemoveAt(0);

                var startCell = startData.Conn.GlobalPosition + startData.Conn.Direction;

                var targetDict = allFreeConnections
                    .Where(x => x.Room != startData.Room)
                    .GroupBy(x => x.Conn.GlobalPosition + x.Conn.Direction)
                    .ToDictionary(g => g.Key, g => g.First());

                if (levelGrid.IsCellOccupied(startCell) && !targetDict.ContainsKey(startCell))
                {
                    continue; 
                }

                var queue = new Queue<PathNodeNew>();
                var visited = new HashSet<Vector3Int>();

                queue.Enqueue(new PathNodeNew { Cell = startCell, Parent = null, Depth = 1 });
                visited.Add(startCell);

                PathNodeNew endNode = null;
                (PlacedRoomDataNew Room, ConnectionPointNew Conn)? foundTarget = null;

                while (queue.Count > 0)
                {
                    var curr = queue.Dequeue();

                    if (targetDict.TryGetValue(curr.Cell, out var target))
                    {
                        endNode = curr;
                        foundTarget = target;
                        break;
                    }

                    if (curr.Depth >= 6) continue;

                    foreach (var dir in SearchDirections)
                    {
                        var nextCell = curr.Cell + dir;
                        if (!visited.Contains(nextCell))
                        {
                            if (!levelGrid.IsCellOccupied(nextCell) || targetDict.ContainsKey(nextCell))
                            {
                                visited.Add(nextCell);
                                queue.Enqueue(new PathNodeNew { Cell = nextCell, Parent = curr, Depth = curr.Depth + 1 });
                            }
                        }
                    }
                }

                if (endNode != null && foundTarget.HasValue)
                {
                    var path = new List<Vector3Int>();
                    var node = endNode;
                    while (node != null)
                    {
                        path.Add(node.Cell);
                        node = node.Parent;
                    }
                    path.Reverse();

                    if (path.Count <= 6)
                    {
                        PlaceTunnelsAlongPath(path, tunnelPrefabs, startData.Room, startData.Conn, foundTarget.Value.Conn);
                        
                        allFreeConnections.Remove(foundTarget.Value);
                        startData.Room.FreeConnections.Remove(startData.Conn);
                        foundTarget.Value.Room.FreeConnections.Remove(foundTarget.Value.Conn);
                    }
                }
            }
        }
        
        private void PlaceTunnelsAlongPath(List<Vector3Int> path, List<LevelRoomNew> tunnelPrefabs, PlacedRoomDataNew ownerData, ConnectionPointNew startConn, ConnectionPointNew endConn)
        {
            for (int i = 0; i < path.Count; i++)
            {
                var cell = path[i];
                if (levelGrid.IsCellOccupied(cell)) continue; 

                var prevCell = (i == 0) ? startConn.GlobalPosition : path[i - 1];
                var nextCell = (i == path.Count - 1) ? endConn.GlobalPosition : path[i + 1];

                var dirToPrev = prevCell - cell;
                var dirToNext = nextCell - cell;

                bool placed = false;
                foreach (var prefab in tunnelPrefabs.OrderBy(_ => _random.Next()))
                {
                    for (int r = 0; r < 4; r++)
                    {
                        var rot = (RoomRotation)r;
                        var plates = RoomRotationHelper.GetRotatedPlates(prefab, rot);

                        bool hasPrevDoor = false;
                        bool hasNextDoor = false;

                        foreach (var p in plates)
                        {
                            if (p.LocalPosition == Vector3Int.zero)
                            {
                                foreach (var door in p.Doors)
                                {
                                    if (door.GlobalDirection == dirToPrev) hasPrevDoor = true;
                                    if (door.GlobalDirection == dirToNext) hasNextDoor = true;
                                }
                            }
                        }

                        if (hasPrevDoor && hasNextDoor && RoomCollisionValidator.IsPlacementValid(levelGrid, prefab, rot, cell))
                        {
                            var instance = InstantiateRoom(prefab, cell, rot, "PathTunnel");
                            ownerData.AttachedTunnels.Add(instance);

                            foreach (var p in plates)
                            {
                                levelGrid.SetCellState(cell + p.LocalPosition, true);
                            }
                            
                            placed = true;
                            break;
                        }
                    }
                    if (placed) break;
                }
            }
        }
    }
}