using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Game.Scripts.GameFiles.LevelGeneration.ItemSpawn;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.LevelGeneration.Editor_Grid;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using Game.Scripts.GameFiles.LevelGeneration.ItemSpawn;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class PlacedRoomDataCluster
    {
        public LevelRoom RoomComponent;
        public GameObject Prefab;
        public Vector3Int Origin;
        public RoomRotation Rotation;
        public RoomCluster Cluster;
        public List<ConnectionPointNew> FreeConnections = new();
        public List<Vector3Int> OccupiedCells = new();
        public List<GameObject> AttachedTunnels = new();
    }
    
    public struct ConnectionPointNew
    {
        public Vector3Int GlobalPosition;
        public Vector3Int Direction;
    }
    
    public class LevelOrchestrator : MonoBehaviour
    {
        [SerializeField] private RoomDatabase roomDatabase;
        [SerializeField] public LevelGrid levelGrid;
        [SerializeField] public NetworkObjectsOrchestrator  networkObjectsOrchestrator;
        [SerializeField] public NetworkRarityItemsOrchestrator networkRarityItemsOrchestrator;
        [SerializeField] private Transform levelContainer;
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject wallWithPassagePrefab;

        public static Transform ActiveLevelContainer { get; private set; }

        private void Awake()
        {
            ActiveLevelContainer = levelContainer;
        }

        private const int MAX_CLUSTER_PLACEMENT_ATTEMPTS = 15; 
        
        private System.Random _random;
        private List<PlacedRoomDataCluster> _allPlacedRooms = new();
        private List<RoomCluster> _clusters = new();
        private readonly List<(PlacedRoomDataCluster Room, ConnectionPointNew Conn)> _usedConnections = new();
        private List<GameObject> _placedWalls = new();
        public List<NetworkObjectSpot> AllLevelSpots { get; private set; } = new();
        //TODO ЗАМЕНИТЬ МОНОБЕХ LEVELROOM НА ROOM ID
        public Dictionary<LevelRoom, List<NetworkRarityItemSpot>> AllLevelRarityItemSpots { get; private set; } = new();

        public IReadOnlyList<PlacedRoomDataCluster> AllPlacedRooms => _allPlacedRooms;
        public IReadOnlyList<RoomCluster> Clusters => _clusters;
        private static readonly Vector3Int[] SearchDirections = {
            new(1, 0, 0), new(-1, 0, 0),
            new(0, 0, 1), new(0, 0, -1)
        };
        
        private class PathNode
        {
            public Vector3Int Cell;
            public PathNode Parent;
            public int Depth;
        }

        [Inject]
        private void Construct(ItemRarityDatabase rarityDatabase,
            ItemPoolManager itemPoolManager,
            PhysicalItemRegistry physicalItemRegistry)
        {
            networkRarityItemsOrchestrator.Init(rarityDatabase,
                itemPoolManager,
                physicalItemRegistry);
        }
        
        public void GeneratePhysicalLevel(List<RoomCluster> clusters, int seed)
        {
            _random = new System.Random(seed);
            ClearLevel();

            if (clusters == null || clusters.Count == 0) return;

            _clusters = new List<RoomCluster>(clusters);

            var coreCluster = clusters[0];
            if (!PlaceCoreCluster(coreCluster))
            {
                Debug.LogError("[GENERATOR] Can't place the core after a few attempts.");
                return;
            }

            var nonCoreClusters = clusters.Skip(1).ToList();
            var dirs = GetShuffled(SearchDirections);

            for (var i = 0; i < nonCoreClusters.Count; i++)
            {
                var dir = dirs[i % dirs.Count];
                PlaceCluster(nonCoreClusters[i], dir);
            }
            ConnectAllFreeExits();
            PlaceRecoveryHangar(clusters);

            BlockUnusedExits();
            PlaceUsedExitPassages();
            networkObjectsOrchestrator.SpawnNetworkObjects(AllLevelSpots);
            BakeNavMeshes();
            networkRarityItemsOrchestrator.SpawnNetworkRarityItem(AllLevelRarityItemSpots);
            Debug.Log($"[GENERATOR] Total clusters: {clusters.Count}. Total placed rooms: {_allPlacedRooms.Count}.");
        }
        
        private void BakeNavMeshes()
        {
            foreach (var roomData in _allPlacedRooms)
            {
                var room = roomData.RoomComponent;
                if (room == null) continue;

                var surface = room.NavMeshSurface;
                if (surface == null)
                {
                    Debug.LogWarning($"[GENERATOR] У комнаты {room.name} не задан NavMeshSurface, навмеш не запечён.");
                    continue;
                }

                surface.BuildNavMesh();
            }
        }

        
        
        private void Shuffle<T>(IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = _random.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        private List<T> GetShuffled<T>(IEnumerable<T> source)
        {
            var list = source.ToList();
            Shuffle(list);
            return list;
        }
        
        
        private void ClearLevel()
        {
            levelGrid.ClearGrid();
            _allPlacedRooms.Clear();
            _clusters.Clear();
            _usedConnections.Clear();
            AllLevelSpots.Clear();
            AllLevelRarityItemSpots.Clear();
            _placedWalls.Clear();
            if (!levelContainer) return;
            for (var i = levelContainer.childCount - 1; i >= 0; i--)
            {
                var child = levelContainer.GetChild(i).gameObject;

                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
        }
        
        private bool PlaceCoreCluster(RoomCluster coreCluster)
        {
            for (var attempt = 0; attempt < MAX_CLUSTER_PLACEMENT_ATTEMPTS; attempt++)
            {
                var commandCenterNode = coreCluster.Rooms.FirstOrDefault(r => r.Type == RoomType.CommandCenter);
                if (commandCenterNode == null) return false;

                var candidates = roomDatabase.GetSuitableRooms(commandCenterNode.Type, 1, false).OrderBy(r => r.PrefabGameObject.name).ToList();
                if (candidates.Count == 0) return false;

                var entry = candidates[_random.Next(candidates.Count)];
                var commandCenterData = InstantiateAndRegisterRoom(entry, Vector3Int.zero, RoomRotation.Deg0, coreCluster);

                var remainingRooms = coreCluster.Rooms.Where(r => r != commandCenterNode).ToList();
                var placedCoreRooms = new List<PlacedRoomDataCluster> { commandCenterData };


                var success = true;
                foreach (var roomNode in remainingRooms)
                {
                    if (TryPlaceRoomInCluster(roomNode, placedCoreRooms, coreCluster)) continue;
                    success = false;
                    break;
                }

                if (success && ValidateClusterIntegrity(placedCoreRooms))
                {
                    return true;
                }

                UndoClusterPlacement(placedCoreRooms);
            }

            return false;
        }

        private void PlaceCluster(RoomCluster cluster, Vector3Int direction)
        {
            for (var attempt = 0; attempt < MAX_CLUSTER_PLACEMENT_ATTEMPTS; attempt++)
            {
                var startNode = cluster.Rooms[0];
                var candidates = roomDatabase.GetSuitableRooms(startNode.Type, 1, false);
                if (candidates.Count == 0) return;

                var origin = FindFreeSpaceAroundCore(candidates, direction);
                if (!origin.HasValue) continue;

                var prefab = candidates[_random.Next(candidates.Count)];
                var rotation = (RoomRotation)_random.Next(4);
                var startData = InstantiateAndRegisterRoom(prefab, origin.Value, rotation, cluster);

                var placedClusterRooms = new List<PlacedRoomDataCluster> { startData };
                var success = true;

                for (var i = 1; i < cluster.Rooms.Count; i++)
                {
                    if (!TryPlaceRoomInCluster(cluster.Rooms[i], placedClusterRooms, cluster))
                    {
                        success = false;
                        break;
                    }
                }

                if (success && ValidateClusterIntegrity(placedClusterRooms))
                {
                    return;
                }

                UndoClusterPlacement(placedClusterRooms);
            }
            
            Debug.LogWarning($"[GENERATOR] Failed to link cluster after {MAX_CLUSTER_PLACEMENT_ATTEMPTS} attempts.");
        }
        
        private void PlaceRecoveryHangar(List<RoomCluster> clusters)
        {
            if (clusters == null || clusters.Count < 2) return;

            var commandCenterRoom = _allPlacedRooms.FirstOrDefault(r => r.RoomComponent.RoomType == RoomType.CommandCenter);
            var originCell = commandCenterRoom?.Origin ?? Vector3Int.zero;

            RoomCluster farthestCluster = null;
            var farthestDistance = -1;
            
            for (var i = 1; i < clusters.Count; i++)
            {
                var cluster = clusters[i];
                var roomsInCluster = _allPlacedRooms.Where(r => r.Cluster == cluster).ToList();
                if (roomsInCluster.Count == 0) continue;

                var freeExitCount = roomsInCluster.Sum(r => r.FreeConnections.Count);
                if (freeExitCount < 2) continue;

                var clusterDistance = roomsInCluster.Max(r => GridDistance(r.Origin, originCell));
                if (clusterDistance > farthestDistance)
                {
                    farthestDistance = clusterDistance;
                    farthestCluster = cluster;
                }
            }

            if (farthestCluster == null)
            {
                for (var i = 1; i < clusters.Count; i++)
                {
                    var cluster = clusters[i];
                    var roomsInCluster = _allPlacedRooms.Where(r => r.Cluster == cluster).ToList();
                    if (roomsInCluster.Count == 0 || !roomsInCluster.Any(r => r.FreeConnections.Count > 0)) continue;

                    var clusterDistance = roomsInCluster.Max(r => GridDistance(r.Origin, originCell));
                    if (clusterDistance > farthestDistance)
                    {
                        farthestDistance = clusterDistance;
                        farthestCluster = cluster;
                    }
                }
            }

            if (farthestCluster == null)
            {
                Debug.LogWarning("[GENERATOR] No cluster with a free exit was found for the evacuation hangar.");
                return;
            }

            var hangarCandidates = roomDatabase.GetSuitableRooms(RoomType.RecoveryHangar, 1, false);
            if (hangarCandidates.Count == 0)
            {
                Debug.LogWarning("[GENERATOR] There are no RecoveryHangar prefabs in the database.");
                return;
            }

            var exitOptions = _allPlacedRooms
                .Where(r => r.Cluster == farthestCluster)
                .SelectMany(r => r.FreeConnections.Select(c => (Room: r, Conn: c)))
                .OrderBy(_ => _random.Next())
                .ToList();

            foreach (var exit in exitOptions)
            {
                foreach (var entry in hangarCandidates.OrderBy(_ => _random.Next()))
                {
                    for (var r = 0; r < 4; r++)
                    {
                        var rot = (RoomRotation)r;
                        var plates = RoomRotationHelper.GetRotatedPlates(entry, rot);
                        var match = FindMatchingConnection(plates, exit.Conn);
                        if (!match.HasValue) continue;

                        var targetCell = exit.Conn.GlobalPosition + exit.Conn.Direction;
                        var origin = targetCell - match.Value;

                        if (!RoomCollisionValidator.IsPlacementValid(levelGrid, entry, rot, origin)) continue;

                        var hangarData = InstantiateAndRegisterRoom(entry, origin, rot, farthestCluster);
                        return;
                    }
                }
            }

            Debug.LogWarning("[GENERATOR] It was not possible to dock the evacuation hangar with the free exit of the farthest cluster.");
        }

        private static int GridDistance(Vector3Int a, Vector3Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
        }

        private bool ValidateClusterIntegrity(List<PlacedRoomDataCluster> clusterRooms)
        {
            var allFreeConnections = clusterRooms.SelectMany(r => r.FreeConnections).ToList();

            if (allFreeConnections.Count < 2)
            {
                return false;
            }

            var validConnectionsCount = 0;

            foreach (var connection in allFreeConnections)
            {
                var targetCell = connection.GlobalPosition + connection.Direction;
                if (levelGrid.IsCellOccupied(targetCell)) continue;

                if (!IsCellAWell(targetCell))
                {
                    validConnectionsCount++;
                }
            }

            return validConnectionsCount >= 2;
        }

        private bool IsCellAWell(Vector3Int emptyCellPos)
        {
            var occupiedNeighbors = 0;
            var directions = new[] { Vector3Int.forward, Vector3Int.back, Vector3Int.left, Vector3Int.right };

            foreach (var dir in directions)
            {
                if (levelGrid.IsCellOccupied(emptyCellPos + dir))
                {
                    occupiedNeighbors++;
                }
            }

            return occupiedNeighbors >= 3;
        }

        private void UndoClusterPlacement(List<PlacedRoomDataCluster> placedRoomsToUndo)
        {
            foreach (var roomData in placedRoomsToUndo)
            {
                foreach (var cellPos in roomData.OccupiedCells)
                {
                    levelGrid.SetCellState(cellPos, false);
                }
                _usedConnections.RemoveAll(uc => uc.Room == roomData);

                _allPlacedRooms.Remove(roomData);

                if (!roomData.RoomComponent) continue;
                foreach (var tunnel in roomData.AttachedTunnels.Where(tunnel => tunnel))
                {
                    if (Application.isPlaying) Destroy(tunnel);
                    else DestroyImmediate(tunnel);
                }
    
                var go = roomData.RoomComponent.gameObject;
                if (Application.isPlaying) Destroy(go);
                else DestroyImmediate(go);
            }
            
            placedRoomsToUndo.Clear();
        }
        
        private bool TryPlaceRoomInCluster(RoomNode nodeToPlace, List<PlacedRoomDataCluster> clusterPlacedRooms, RoomCluster cluster)
        {
            var candidates = roomDatabase.GetSuitableRooms(nodeToPlace.Type, 1, false)
                .OrderBy(_ => _random.Next()).ToList();

            var otherClustersCells = _allPlacedRooms
                .Where(r => r.Cluster != cluster)
                .SelectMany(r => r.OccupiedCells)
                .ToHashSet();

            var validPlacements = new List<(RoomDataEntry entry, ConnectionPointNew parentConn, RoomRotation rot, Vector3Int origin)>();

            foreach (var parentData in clusterPlacedRooms)
            {
                foreach (var entry in candidates)
                {
                    foreach (var parentConn in parentData.FreeConnections.ToList())
                    {
                        for (var r = 0; r < 4; r++)
                        {
                            var rot = (RoomRotation)r;
                            var plates = RoomRotationHelper.GetRotatedPlates(entry, rot);
                            var match = FindMatchingConnection(plates, parentConn);

                            if (!match.HasValue) continue;
                            var targetCell = parentConn.GlobalPosition + parentConn.Direction;
                            var origin = targetCell - match.Value;

                            if (!RoomCollisionValidator.IsPlacementValid(levelGrid, entry, rot, origin)) continue;
                            if (IsSpaceIsolatedFromOtherClusters(origin, entry, rot, otherClustersCells, 1))
                            {
                                validPlacements.Add((entry, parentConn, rot, origin));
                            }
                        }
                    }
                }
            }

            if (validPlacements.Count == 0) return false;

            var selected = validPlacements.First();

            var newRoomData = InstantiateAndRegisterRoom(selected.entry, selected.origin, selected.rot, cluster);

            clusterPlacedRooms.Add(newRoomData);
            return true;
        }
        
                private Vector3Int? FindFreeSpaceAroundCore(List<RoomDataEntry> candidates, Vector3Int preferredDirection)
        {
            if (_allPlacedRooms.Count == 0) return Vector3Int.zero;

            var coreRoom = _allPlacedRooms.FirstOrDefault(r => r.RoomComponent.RoomType == RoomType.CommandCenter);
            if (coreRoom == null) return null;

            var coreCells = _allPlacedRooms
                .Where(r => r.Cluster == coreRoom.Cluster)
                .SelectMany(r => r.OccupiedCells)
                .ToList();

            var otherClustersCells = _allPlacedRooms
                .Where(r => r.Cluster != coreRoom.Cluster)
                .SelectMany(r => r.OccupiedCells)
                .ToHashSet();

            var directionsToTry = new List<Vector3Int> { preferredDirection };
            directionsToTry.AddRange(SearchDirections.Where(d => d != preferredDirection).OrderBy(_ => _random.Next()));

            var distances = new[] { 3, 4, 5 }.OrderBy(_ => _random.Next()).ToArray();

            foreach (var dir in directionsToTry)
            {
                var extremeValue = dir.x != 0 
                    ? (dir.x > 0 ? coreCells.Max(c => c.x) : coreCells.Min(c => c.x))
                    : (dir.z > 0 ? coreCells.Max(c => c.z) : coreCells.Min(c => c.z));

                var edgeCells = coreCells.Where(c => 
                    (dir.x != 0 && c.x == extremeValue) || 
                    (dir.z != 0 && c.z == extremeValue)
                ).OrderBy(_ => _random.Next()).ToList();

                foreach (var cell in edgeCells)
                {
                    foreach (var dist in distances)
                    {
                        var targetCell = cell + dir * dist;

                        foreach (var prefab in candidates.OrderBy(_ => _random.Next()))
                        {
                            for (var r = 0; r < 4; r++)
                            {
                                var rotation = (RoomRotation)r;
                                var plates = RoomRotationHelper.GetRotatedPlates(prefab, rotation);

                                foreach (var plate in plates)
                                {
                                    var potentialOrigin = targetCell - plate.LocalPosition;

                                    if (RoomCollisionValidator.IsPlacementValid(levelGrid, prefab, rotation, potentialOrigin))
                                    {
                                        if (IsSpaceIsolatedFromOtherClusters(potentialOrigin, prefab, rotation, otherClustersCells, 1))
                                        {
                                            return potentialOrigin;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
        
        private bool IsSpaceIsolatedFromOtherClusters(Vector3Int origin, RoomDataEntry entry, RoomRotation rotation, HashSet<Vector3Int> otherClustersCells, int minDistance = 1)
        {
            if (otherClustersCells.Count == 0) return true;

            var plates = RoomRotationHelper.GetRotatedPlates(entry, rotation);
            foreach (var plate in plates)
            {
                var globalPos = origin + plate.LocalPosition;
        
                for (var x = -minDistance; x <= minDistance; x++)
                {
                    for (var z = -minDistance; z <= minDistance; z++)
                    {
                        var checkPos = globalPos + new Vector3Int(x, 0, z);
                        if (otherClustersCells.Contains(checkPos))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        

        private PlacedRoomDataCluster InstantiateAndRegisterRoom(RoomDataEntry entry, Vector3Int origin, RoomRotation rotation, RoomCluster cluster)
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

            var instanceGo = Instantiate(entry.PrefabGameObject, worldPos, rotQuat, levelContainer);
            var spawnedRoomComponent = instanceGo.GetComponent<LevelRoom>();

            AllLevelSpots.AddRange(spawnedRoomComponent.NetworkObjects);
            //TODO ЗАМЕНИТЬ КОМПОНЕНТ НА ROOM ID
            AllLevelRarityItemSpots[spawnedRoomComponent] = spawnedRoomComponent.NetworkRarityItems;
            
            var data = new PlacedRoomDataCluster 
            { 
                RoomComponent = spawnedRoomComponent, 
                Prefab = instanceGo,
                Origin = origin,
                Rotation = rotation,
                Cluster = cluster
            };


            var virtualPlates = RoomRotationHelper.GetRotatedPlates(entry, rotation);

            foreach (var plate in virtualPlates)
            {
                var globalPos = origin + plate.LocalPosition;
                var doorDirs = plate.Doors.Select(d => d.GlobalDirection).ToList();
                levelGrid.SetCellState(globalPos, true, doorDirs, spawnedRoomComponent.GetInstanceID(), spawnedRoomComponent.RoomType);

                data.OccupiedCells.Add(globalPos);

                foreach (var door in plate.Doors)
                {
                    data.FreeConnections.Add(new ConnectionPointNew 
                    { 
                        GlobalPosition = globalPos, 
                        Direction = door.GlobalDirection, 
                    });
                }
            }

            _allPlacedRooms.Add(data);
            
            TryConnectAdjacentDoorsWithinCluster(data, cluster);
            
            return data;
        }

        private void TryConnectAdjacentDoorsWithinCluster(PlacedRoomDataCluster newRoomData, RoomCluster cluster)
        {
            var clusterRooms = _allPlacedRooms.Where(r => r.Cluster == cluster && r != newRoomData).ToList();

            foreach (var otherRoom in clusterRooms)
            {
                foreach (var connA in newRoomData.FreeConnections.ToList())
                {
                    foreach (var connB in otherRoom.FreeConnections.ToList())
                    {
                        if (connA.GlobalPosition + connA.Direction != connB.GlobalPosition ||
                            connB.Direction != -connA.Direction) continue;
                        newRoomData.FreeConnections.Remove(connA);
                        otherRoom.FreeConnections.Remove(connB);
                        _usedConnections.Add((newRoomData, connA));
                        _usedConnections.Add((otherRoom, connB));
                        break;
                    }
                }
            }
        }


        private Vector3Int ? FindMatchingConnection(VirtualPlateData[] plates, ConnectionPointNew parentConn)
        {
            var targetDirection = -parentConn.Direction;
            foreach (var plate in plates)
            {
                if (plate.Doors.Any(door => door.GlobalDirection == targetDirection))
                    return plate.LocalPosition;
            }
            return null;
        }

        
        private List<(PlacedRoomDataCluster Room, ConnectionPointNew Conn)> GetFreeExits(Func<RoomCluster, bool> clusterFilter = null)
        {
            return _allPlacedRooms
                .Where(r => r.RoomComponent != null &&
                            r.RoomComponent.RoomType is not (RoomType.CommandCenter or RoomType.RecoveryHangar))
                .Where(r => clusterFilter == null || clusterFilter(r.Cluster))
                .SelectMany(r => r.FreeConnections.Select(c => (Room: r, Conn: c)))
                .ToList();
        }

        private void ConnectAllFreeExits()
        {
            var clusterLinks = new Dictionary<RoomCluster, HashSet<RoomCluster>>();

            ConnectAdjacentDoorsGlobally(GetFreeExits(), clusterLinks);

            var tunnelPrefabs = roomDatabase.GetSuitableRooms(RoomType.TechnicalTunnels, 1, false);
            if (tunnelPrefabs == null || tunnelPrefabs.Count == 0) return;

            bool connectionsAdded;
            var currentMaxDepth = 2;
            var absoluteMaxDepth = 14;

            do
            {
                connectionsAdded = false;
                var currentExits = GetFreeExits();
                var freeExitsBefore = currentExits.Count;

                ConnectFreeExitPairs(currentExits, tunnelPrefabs, clusterLinks, currentMaxDepth);

                var freeExitsAfter = GetFreeExits().Count;
                if (freeExitsAfter < freeExitsBefore)
                {
                    connectionsAdded = true;
                    currentMaxDepth = 2; 
                }
                else
                {
                    currentMaxDepth++;
                    if (currentMaxDepth <= absoluteMaxDepth)
                    {
                        connectionsAdded = true;
                    }
                }
            }
            while (connectionsAdded);

            EnsureAllClustersConnected(tunnelPrefabs, clusterLinks);
        }

        private bool TryLinkExitToTargets(
            (PlacedRoomDataCluster Room, ConnectionPointNew Conn) startExit,
            List<(PlacedRoomDataCluster Room, ConnectionPointNew Conn)> candidateTargets,
            List<RoomDataEntry> tunnelPrefabs,
            Dictionary<RoomCluster, HashSet<RoomCluster>> clusterLinks,
            int maxPathLength = 14)
        {
            var targetDict = candidateTargets
                .Where(x => x.Room.Cluster != startExit.Room.Cluster)
                .GroupBy(x => x.Conn.GlobalPosition + x.Conn.Direction)
                .ToDictionary(g => g.Key, g => g.First());

            if (targetDict.Count == 0) return false;

            var startCell = startExit.Conn.GlobalPosition + startExit.Conn.Direction;
            if (levelGrid.IsCellOccupied(startCell) && !targetDict.ContainsKey(startCell)) return false;

            var queue = new Queue<PathNode>();
            var visited = new HashSet<Vector3Int>();
            queue.Enqueue(new PathNode { Cell = startCell, Parent = null, Depth = 1 });
            visited.Add(startCell);

            PathNode endNode = null;
            (PlacedRoomDataCluster Room, ConnectionPointNew Conn)? foundTarget = null;

            while (queue.Count > 0)
            {
                var curr = queue.Dequeue();

                if (targetDict.TryGetValue(curr.Cell, out var target))
                {
                    endNode = curr;
                    foundTarget = target;
                    break;
                }

                if (curr.Depth >= maxPathLength) continue;

                foreach (var dir in SearchDirections)
                {
                    var nextCell = curr.Cell + dir;
                    if (visited.Contains(nextCell)) continue;

                    if (levelGrid.IsCellOccupied(nextCell))
                    {
                        if (!targetDict.ContainsKey(nextCell))
                        {
                            continue;
                        }
                    }

                    visited.Add(nextCell);
                    queue.Enqueue(new PathNode { Cell = nextCell, Parent = curr, Depth = curr.Depth + 1 });
                }
            }

            if (endNode == null || !foundTarget.HasValue) return false;

            var path = new List<Vector3Int>();
            var node = endNode;
            while (node != null)
            {
                path.Add(node.Cell);
                node = node.Parent;
            }
            path.Reverse();

            if (path.Count > maxPathLength) return false;

            var targetRoom = foundTarget.Value.Room;
            var targetConn = foundTarget.Value.Conn;

            if (!TryPlaceTunnelsAlongPath(path, tunnelPrefabs, startExit.Room, startExit.Conn, targetConn))
                return false;

            startExit.Room.FreeConnections.Remove(startExit.Conn);
            targetRoom.FreeConnections.Remove(targetConn);
            _usedConnections.Add((startExit.Room, startExit.Conn));
            _usedConnections.Add((targetRoom, targetConn));

            RegisterClusterLink(clusterLinks, startExit.Room.Cluster, targetRoom.Cluster);

            return true;
        }

        private bool TryConnectFirstAvailable(
            List<(PlacedRoomDataCluster Room, ConnectionPointNew Conn)> ownExits,
            List<(PlacedRoomDataCluster Room, ConnectionPointNew Conn)> targets,
            List<RoomDataEntry> tunnelPrefabs,
            Dictionary<RoomCluster, HashSet<RoomCluster>> clusterLinks)
        {
            if (targets.Count == 0) return false;

            foreach (var startExit in ownExits)
            {
                if (TryLinkExitToTargets(startExit, targets, tunnelPrefabs, clusterLinks))
                    return true;
            }

            return false;
        }

        private void ConnectClustersToCore(
            RoomCluster coreCluster,
            List<RoomDataEntry> tunnelPrefabs,
            Dictionary<RoomCluster, HashSet<RoomCluster>> clusterLinks)
        {
            var otherClusters = _allPlacedRooms
                .Select(r => r.Cluster)
                .Distinct()
                .Where(c => c != coreCluster)
                .ToList();

            foreach (var cluster in otherClusters)
            {
                if (clusterLinks.TryGetValue(coreCluster, out var coreLinks) && coreLinks.Contains(cluster))
                    continue;

                var ownExits = GetFreeExits(c => c == cluster).OrderBy(_ => _random.Next()).ToList();
                var coreExits = GetFreeExits(c => c == coreCluster);

                if (!TryConnectFirstAvailable(ownExits, coreExits, tunnelPrefabs, clusterLinks))
                {
                    Debug.LogWarning("[GENERATOR] Не удалось напрямую соединить кластер с ядром (не хватило свободных выходов/пути) — попробуем через резервную связь.");
                }
            }
        }

        private void EnsureRedundantLinks(
            RoomCluster coreCluster,
            List<RoomDataEntry> tunnelPrefabs,
            Dictionary<RoomCluster, HashSet<RoomCluster>> clusterLinks)
        {
            var otherClusters = _allPlacedRooms
                .Select(r => r.Cluster)
                .Distinct()
                .Where(c => c != coreCluster)
                .ToList();

            foreach (var cluster in otherClusters)
            {
                var linkedTo = clusterLinks.TryGetValue(cluster, out var linked) ? linked : new HashSet<RoomCluster>();
                if (linkedTo.Count >= 2) continue;

                var ownExits = GetFreeExits(c => c == cluster).OrderBy(_ => _random.Next()).ToList();
                if (ownExits.Count == 0)
                {
                    Debug.LogWarning("[GENERATOR] У кластера не осталось свободных выходов для резервной связи.");
                    continue;
                }

                clusterLinks.TryGetValue(coreCluster, out var coreLinks);
                var otherSpokes = GetFreeExits(c =>
                    c != cluster && c != coreCluster &&
                    !linkedTo.Contains(c) &&
                    coreLinks != null && coreLinks.Contains(c));

                if (TryConnectFirstAvailable(ownExits, otherSpokes, tunnelPrefabs, clusterLinks))
                    continue;

                var anyOther = GetFreeExits(c => c != cluster && !linkedTo.Contains(c));
                if (!TryConnectFirstAvailable(ownExits, anyOther, tunnelPrefabs, clusterLinks))
                {
                    Debug.LogWarning("[GENERATOR] Не удалось дать кластеру вторую (резервную) связь — у него может остаться только один путь к ядру.");
                }
            }
        }
        
        private void ConnectAdjacentDoorsGlobally(
            List<(PlacedRoomDataCluster Room, ConnectionPointNew Conn)> allFreeConnections,
            Dictionary<RoomCluster, HashSet<RoomCluster>> clusterLinks)
        {
            for (var i = allFreeConnections.Count - 1; i >= 0; i--)
            {
                var connAData = allFreeConnections[i];
        
                for (var j = i - 1; j >= 0; j--)
                {
                    var connBData = allFreeConnections[j];
            
                    if (connAData.Conn.GlobalPosition + connAData.Conn.Direction == connBData.Conn.GlobalPosition &&
                        connBData.Conn.Direction == -connAData.Conn.Direction)
                    {
                        connAData.Room.FreeConnections.Remove(connAData.Conn);
                        connBData.Room.FreeConnections.Remove(connBData.Conn);
                
                        _usedConnections.Add((connAData.Room, connAData.Conn));
                        _usedConnections.Add((connBData.Room, connBData.Conn));

                        if (connAData.Room.Cluster != connBData.Room.Cluster)
                        {
                            RegisterClusterLink(clusterLinks, connAData.Room.Cluster, connBData.Room.Cluster);
                        }

                        allFreeConnections.RemoveAt(i);
                        allFreeConnections.RemoveAt(j);
                        break;
                    }
                }
            }
        }

        private void ConnectFreeExitPairs(
            List<(PlacedRoomDataCluster Room, ConnectionPointNew Conn)> allFreeConnections,
            List<RoomDataEntry> tunnelPrefabs,
            Dictionary<RoomCluster, HashSet<RoomCluster>> clusterLinks,
            int maxPathLength)
        {
            var i = 0;
            while (i < allFreeConnections.Count)
            {
                var startData = allFreeConnections[i];
                var startCluster = startData.Room.Cluster;

                var pool = allFreeConnections.Where(x => 
                    x.Room.Cluster != startCluster && 
                    !(clusterLinks.TryGetValue(startCluster, out var linked) && linked.Contains(x.Room.Cluster))
                );

                var targetDict = pool
                    .GroupBy(x => x.Conn.GlobalPosition + x.Conn.Direction)
                    .ToDictionary(g => g.Key, g => g.First());

                if (targetDict.Count == 0)
                {
                    i++;
                    continue;
                }

                var startCell = startData.Conn.GlobalPosition + startData.Conn.Direction;

                if (levelGrid.IsCellOccupied(startCell) && !targetDict.ContainsKey(startCell))
                {
                    i++;
                    continue;
                }

                var queue = new Queue<PathNode>();
                var visited = new HashSet<Vector3Int>();

                queue.Enqueue(new PathNode { Cell = startCell, Parent = null, Depth = 1 });
                visited.Add(startCell);

                PathNode endNode = null;
                (PlacedRoomDataCluster Room, ConnectionPointNew Conn)? foundTarget = null;

                while (queue.Count > 0)
                {
                    var curr = queue.Dequeue();

                    if (targetDict.TryGetValue(curr.Cell, out var target))
                    {
                        endNode = curr;
                        foundTarget = target;
                        break;
                    }

                    if (curr.Depth >= maxPathLength) continue;

                    foreach (var dir in SearchDirections)
                    {
                        var nextCell = curr.Cell + dir;
                        if (visited.Contains(nextCell)) continue;

                        if (levelGrid.IsCellOccupied(nextCell))
                        {
                            if (!targetDict.ContainsKey(nextCell))
                            {
                                continue;
                            }
                        }

                        visited.Add(nextCell);
                        queue.Enqueue(new PathNode { Cell = nextCell, Parent = curr, Depth = curr.Depth + 1 });
                    }
                }

                var connected = false;

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

                    if (path.Count <= maxPathLength)
                    {
                        var targetRoom = foundTarget.Value.Room;
                        var targetConn = foundTarget.Value.Conn;

                        if (TryPlaceTunnelsAlongPath(path, tunnelPrefabs, startData.Room, startData.Conn, targetConn))
                        {
                            RegisterClusterLink(clusterLinks, startCluster, targetRoom.Cluster);

                            allFreeConnections.Remove(foundTarget.Value);
                            allFreeConnections.Remove(startData);
                            
                            startData.Room.FreeConnections.Remove(startData.Conn);
                            targetRoom.FreeConnections.Remove(targetConn);
                            _usedConnections.Add((startData.Room, startData.Conn));
                            _usedConnections.Add((targetRoom, targetConn));

                            connected = true;
                        }
                    }
                }

                if (!connected) i++;
            }
        }
        
        private bool TryPlaceTunnelsAlongPath(List<Vector3Int> path, List<RoomDataEntry> tunnelPrefabs, PlacedRoomDataCluster ownerData, ConnectionPointNew startConn, ConnectionPointNew endConn)
        {
            var sortedPrefabs = tunnelPrefabs
                .OrderByDescending(p => RoomRotationHelper.GetRotatedPlates(p, RoomRotation.Deg0).Length)
                .ThenBy(p => p.PrefabGameObject.name)
                .ToList();

            var instantiatedTunnels = new List<GameObject>();
            var modifiedCells = new List<Vector3Int>();

            var i = 0;
            while (i < path.Count)
            {
                var cell = path[i];
                if (levelGrid.IsCellOccupied(cell)) 
                {
                    i++;
                    continue; 
                }

                var placed = false;
                var prevCell = i == 0 ? startConn.GlobalPosition : path[i - 1];

                foreach (var enrty in sortedPrefabs)
                {
                    for (var r = 0; r < 4; r++)
                    {
                        var rot = (RoomRotation)r;
                        var plates = RoomRotationHelper.GetRotatedPlates(enrty, rot);
                        var prefabSize = plates.Length;

                        if (i + prefabSize > path.Count) continue;

                        var matchPath = true;
                        var hasPrevDoor = false;
                        var hasNextDoor = false;
                        
                        var expectedPathCells = new HashSet<Vector3Int>();
                        for (var j = 0; j < prefabSize; j++) expectedPathCells.Add(path[i + j]);

                        var nextCellAfterPrefab = (i + prefabSize == path.Count) ? endConn.GlobalPosition : path[i + prefabSize];
                        var lastPrefabCell = path[i + prefabSize - 1];
                        var actualPrefabCells = new HashSet<Vector3Int>();

                        foreach (var p in plates)
                        {
                            var globalPos = cell + p.LocalPosition;
                            actualPrefabCells.Add(globalPos);

                            if (globalPos == cell)
                            {
                                var dirToPrev = prevCell - cell;
                                if (p.Doors.Any(d => d.GlobalDirection == dirToPrev)) hasPrevDoor = true;
                            }

                            if (globalPos == lastPrefabCell)
                            {
                                var dirToNext = nextCellAfterPrefab - lastPrefabCell;
                                if (p.Doors.Any(d => d.GlobalDirection == dirToNext)) hasNextDoor = true;
                            }
                        }

                        if (!expectedPathCells.SetEquals(actualPrefabCells)) matchPath = false;

                        if (matchPath && hasPrevDoor && hasNextDoor && RoomCollisionValidator.IsPlacementValid(levelGrid, enrty, rot, cell))
                        {
                            var instance = InstantiateTunnel(enrty, cell, rot);
                            instantiatedTunnels.Add(instance);

                            foreach (var p in plates)
                            {
                                var globalPos = cell + p.LocalPosition;
                                var doorDirs = p.Doors.Select(d => d.GlobalDirection).ToList();
                                levelGrid.SetCellState(globalPos, true, doorDirs, instance.GetInstanceID(), RoomType.TechnicalTunnels);
                                modifiedCells.Add(globalPos);
                            }
                            
                            i += prefabSize - 1; 
                            placed = true;
                            break;
                        }
                    }
                    if (placed) break;
                }

                if (!placed)
                {
                    foreach (var tunnel in instantiatedTunnels)
                    {
                        if (Application.isPlaying) Destroy(tunnel);
                        else DestroyImmediate(tunnel);
                    }
                    foreach (var modifiedCell in modifiedCells)
                    {
                        levelGrid.SetCellState(modifiedCell, false);
                    }
                    return false;
                }
                i++;
            }

            ownerData.AttachedTunnels.AddRange(instantiatedTunnels);
            return true;
        }

        private static void RegisterClusterLink(Dictionary<RoomCluster, HashSet<RoomCluster>> clusterLinks, RoomCluster a, RoomCluster b)
        {
            if (!clusterLinks.TryGetValue(a, out var setA)) clusterLinks[a] = setA = new HashSet<RoomCluster>();
            setA.Add(b);

            if (!clusterLinks.TryGetValue(b, out var setB)) clusterLinks[b] = setB = new HashSet<RoomCluster>();
            setB.Add(a);
        }

                private void EnsureAllClustersConnected(List<RoomDataEntry> tunnelPrefabs, Dictionary<RoomCluster, HashSet<RoomCluster>> clusterLinks)
        {
            var commandCenterRoom = _allPlacedRooms.FirstOrDefault(r => r.RoomComponent.RoomType == RoomType.CommandCenter);
            if (commandCenterRoom == null) return;

            var coreCluster = commandCenterRoom.Cluster;
            var allClusters = _allPlacedRooms.Select(r => r.Cluster).Distinct().ToList();

            var reachable = ComputeReachableClusters(coreCluster, clusterLinks);
            var isolatedClusters = allClusters.Where(c => c != coreCluster && !reachable.Contains(c)).ToList();

            foreach (var isolatedCluster in isolatedClusters)
            {
                if (reachable.Contains(isolatedCluster)) continue;

                var ownExits = GetFreeExits(c => c == isolatedCluster);
                if (ownExits.Count == 0)
                {
                    Debug.LogWarning("[GENERATOR] The isolated cluster had no free ports left\u2014connection to the network failed.");
                    continue;
                }

                var reachableExits = GetFreeExits(c => reachable.Contains(c));
                if (reachableExits.Count == 0)
                {
                    Debug.LogWarning("[GENERATOR] The connected part of the network has no free ports left\u2014there is nowhere to extend the tunnel to.");
                    continue;
                }

                if (TryConnectFirstAvailable(ownExits, reachableExits, tunnelPrefabs, clusterLinks))
                {
                    reachable = ComputeReachableClusters(coreCluster, clusterLinks);
                }
                else
                {
                    Debug.LogWarning("[GENERATOR] Failed to forcibly connect the isolated cluster to the network.");
                }
            }
        }

        private HashSet<RoomCluster> ComputeReachableClusters(RoomCluster coreCluster, Dictionary<RoomCluster, HashSet<RoomCluster>> clusterLinks)
        {
            var reachable = new HashSet<RoomCluster> { coreCluster };
            var bfsQueue = new Queue<RoomCluster>();
            bfsQueue.Enqueue(coreCluster);

            while (bfsQueue.Count > 0)
            {
                var current = bfsQueue.Dequeue();
                if (!clusterLinks.TryGetValue(current, out var neighbours)) continue;

                foreach (var neighbour in neighbours)
                {
                    if (reachable.Add(neighbour))
                    {
                        bfsQueue.Enqueue(neighbour);
                    }
                }
            }

            return reachable;
        }

        private GameObject InstantiateTunnel(RoomDataEntry entry, Vector3Int origin, RoomRotation rotation)
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

            var instanceGo = Instantiate(entry.PrefabGameObject, worldPos, rotQuat, levelContainer);
            return instanceGo;
        }
        
        private void InstantiateDoorwayInsert(PlacedRoomDataCluster roomData, ConnectionPointNew conn, GameObject prefab, string instanceName)
        {
            if (prefab == null || roomData?.RoomComponent == null) return;

            var centerWorldPos = levelGrid.UnityGrid.GetCellCenterWorld(conn.GlobalPosition);
            var baseWorldPos = levelGrid.UnityGrid.CellToWorld(conn.GlobalPosition);
            
            var facing = -conn.Direction;
            var wallRot = Quaternion.identity;
            if (facing == Vector3Int.right) wallRot = Quaternion.Euler(0, 90, 0);
            else if (facing == Vector3Int.back) wallRot = Quaternion.Euler(0, 180, 0);
            else if (facing == Vector3Int.left) wallRot = Quaternion.Euler(0, 270, 0);
            
            var left = new Vector3(-conn.Direction.z, 0f, conn.Direction.x);
            var wallPos = new Vector3(
                centerWorldPos.x + conn.Direction.x * 5f + left.x * 5f,
                baseWorldPos.y,
                centerWorldPos.z + conn.Direction.z * 5f + left.z * 5f
            );
            
            var instance = Instantiate(prefab, wallPos, wallRot, levelContainer);
            instance.name = instanceName;
            _placedWalls.Add(instance);
        }
        
        private void BlockUnusedExits()
        {
            if (wallPrefab == null)
            {
                Debug.LogError("[GENERATOR] Wall prefab not assigned.");
                return;
            }

            foreach (var roomData in _allPlacedRooms)
            {
                if (roomData.RoomComponent == null) continue;

                foreach (var conn in roomData.FreeConnections)
                {
                    InstantiateDoorwayInsert(roomData, conn, wallPrefab, "BlockedExitWall");
                }
            }
 
        }
        
        private void PlaceUsedExitPassages()
        {
            if (wallWithPassagePrefab == null)
            {
                Debug.LogError("[GENERATOR] Префаб стены с проходом не назначен.");
                return;
            }

            foreach (var (roomData, conn) in _usedConnections)
            {
                if (roomData.RoomComponent == null) continue;

                InstantiateDoorwayInsert(roomData, conn, wallWithPassagePrefab, "UsedExitPassageWall");
            }
        }
    }
}