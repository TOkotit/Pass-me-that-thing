
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using Game.Scripts.GameFiles.LevelGeneration.Objects;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class LevelSpawnerTest_ : MonoBehaviour
    {
        
        private IObjectResolver _resolver;
        public float roomSizeMultiplier = 10f; 

        [Header("Префабы комнат")]
        public SerializedDictionary<RoomType, List<GameObject>> roomPrefabs;
        public SerializedDictionary<GameEventsType, List<GameObject>> eventPrefabs;
        
        public GameObject solidWallPrefab;
        public GameObject doorWallPrefab;
        
        [Inject]
        public void Construct(IObjectResolver resolver)
        {
            _resolver = resolver;
        }
        
        private void Start()
        {
            var eventsPool = new List<EventRoomDefinition>
            {
                new EventRoomDefinition("flood_1", GameEventsType.FloodBrokenPump, 35),
                new EventRoomDefinition("flood_2", GameEventsType.FloodPipeBreak, 30),
                new EventRoomDefinition("blackout_1", GameEventsType.BlackoutBlowFuse, 40),
                new EventRoomDefinition("blackout_2", GameEventsType.BlackoutCutWires, 45)
            };

            var randomDefenseRooms = Random.Range(3, 6); 
            var maxSideRoomsCapacity = randomDefenseRooms * 10;
            var randomSideRooms = Random.Range(3, maxSideRoomsCapacity + 1); 
            var randomBudget = Random.Range(100, 161);

            Debug.Log($"<color=orange><b>[Тест Generation]</b></color> Сгенерированы параметры: " +
                      $"Оборона = {randomDefenseRooms}, Боковые = {randomSideRooms}, Бюджет = {randomBudget}");

            var macroData = new LevelMacroData(
                totalRoomsWithoutHub: randomSideRooms, 
                exitsCount: 1, 
                defenseRoomsCount: randomDefenseRooms, 
                eventRoomsBudget: randomBudget, 
                availableEventsPool: eventsPool
            );

            var graphBuilder = new LevelGraphBuilder();
            var rootNode = graphBuilder.BuildGraph(macroData);

            Debug.Log(graphBuilder.GetGraphStructureString(rootNode));

            SpawnGraph(rootNode);
        }
        
        
        private void SpawnGraph(RoomNode startNode)
        {
            var visited = new HashSet<int>();
            var queue = new Queue<(RoomNode Node, RoomNode Parent)>();

            var allSpawnedNodes = new List<RoomNode>();
            queue.Enqueue((startNode, null));
            visited.Add(startNode.NodeId);

            while (queue.Count > 0)
            {
                var (node, parent) = queue.Dequeue();
            
                allSpawnedNodes.Add(node);    
                
                var spawnPosition = new Vector3(node.X * roomSizeMultiplier, 0, node.Y * roomSizeMultiplier);
                var spawnRotation = Quaternion.identity;

                if (parent != null)
                {
                    var parentPosition = new Vector3(parent.X * roomSizeMultiplier, 0, parent.Y * roomSizeMultiplier);
                    var directionToParent = parentPosition - spawnPosition;

                    spawnRotation = Quaternion.FromToRotation(Vector3.right, directionToParent.normalized);
                }

                GameObject prefabToSpawn = null;
                var roomLogName = node.Type.ToString();

                if (node.Type == RoomType.Event && node.EventData != null)
                {
                    var eventType = node.EventData.EventType;
                    roomLogName = $"Event ({eventType})";

                    prefabToSpawn = GetRandomPrefab(eventPrefabs, eventType);

                    if (prefabToSpawn == null)
                    {
                        Debug.LogWarning($"Префаб для ивента {eventType} не найден! Пробуем общий список RoomType.Event.");
                        prefabToSpawn = GetRandomPrefab(roomPrefabs, RoomType.Event);
                    }
                }
                else
                    prefabToSpawn = GetRandomPrefab(roomPrefabs, node.Type);

                if (prefabToSpawn != null)
                {
                    var spawnedRoom = _resolver.Instantiate(prefabToSpawn, spawnPosition, spawnRotation, this.transform);
                    spawnedRoom.name = $"Room [{node.NodeId}] {roomLogName} ({node.X}, {node.Y})";

                    // ProcessRoomMicroGeneration(spawnedRoom, node);
                }
                else
                {
                    Debug.LogError($"Не удалось найти префаб для комнаты [ID: {node.NodeId}] типа {node.Type}!");
                }

                foreach (var connectedNode in node.ConnectedNodes)
                {
                    if (!visited.Add(connectedNode.NodeId)) continue;
                    queue.Enqueue((connectedNode, node));
                }
            }
            GenerateWalls(allSpawnedNodes);
        }
        
        private GameObject GetRandomPrefab<T>(SerializedDictionary<T, List<GameObject>> dictionary, T key)
        {
            if (dictionary.TryGetValue(key, out var list) && list != null && list.Count > 0)
            {
                return list[Random.Range(0, list.Count)];
            }
            return null;
        }
        
        private void GenerateWalls(List<RoomNode> allNodes)
        {
            var grid = new Dictionary<Vector2Int, RoomNode>();
            foreach (var node in allNodes)
            {
                grid[new Vector2Int(node.X, node.Y)] = node;
            }

            var halfDistance = roomSizeMultiplier / 2f;

            foreach (var node in allNodes)
            {
                var pos = new Vector2Int(node.X, node.Y);
                var roomCenter = new Vector3(node.X * roomSizeMultiplier, 0, node.Y * roomSizeMultiplier);

                var directions = new (Vector2Int dir, Vector3 offset, Quaternion rot, bool isUpOrRight)[]
                {
                    (new Vector2Int(0, 1),  new Vector3(0, 0, halfDistance),  Quaternion.Euler(0, 0, 0),   true),  // Вверх
                    (new Vector2Int(1, 0),  new Vector3(halfDistance, 0, 0),  Quaternion.Euler(0, 90, 0),  true),  // Вправо
                    (new Vector2Int(0, -1), new Vector3(0, 0, -halfDistance), Quaternion.Euler(0, 0, 0),   false), // Вниз
                    (new Vector2Int(-1, 0), new Vector3(-halfDistance, 0, 0), Quaternion.Euler(0, 90, 0),  false)  // Влево
                };

                foreach (var d in directions)
                {
                    var neighborPos = pos + d.dir;
                    var hasNeighbor = grid.TryGetValue(neighborPos, out RoomNode neighborNode);

                    GameObject wallPrefabToSpawn = null;

                    if (!hasNeighbor)
                    {
                        wallPrefabToSpawn = solidWallPrefab;
                    }
                    else
                    {
                        if (d.isUpOrRight)
                        {
                            var isConnected = node.ConnectedNodes.Any(n => n.NodeId == neighborNode.NodeId);
                            wallPrefabToSpawn = isConnected ? doorWallPrefab : solidWallPrefab;
                        }
                    }

                    if (wallPrefabToSpawn != null)
                    {
                        var wallPosition = roomCenter + d.offset;
                        var spawnedWall = _resolver.Instantiate(wallPrefabToSpawn, wallPosition, d.rot, this.transform);
                        
                        var wallType = wallPrefabToSpawn == doorWallPrefab ? "Door" : "Solid";
                        spawnedWall.name = $"Wall [{wallType}] at ({pos.x}, {pos.y}) dir {d.dir}";
                    }
                }
            }
        }

        private void ProcessRoomMicroGeneration(GameObject spawnedRoom, RoomNode node)
        {
            //     
            //     var allSpots = spawnedRoom.GetComponentsInChildren<LevelPartSpot>();
            //     if (allSpots.Length == 0) return;
            //
            //     var spotsGroups = allSpots.GroupBy(spot => spot.spotType);
            //
            //     foreach (var group in spotsGroups)
            //     {
            //         var currentType = group.Key;
            //         var availableSpots = group.ToList();
            //
            //         var randomIndex = Random.Range(0, availableSpots.Count);
            //         var selectedSpot = availableSpots[randomIndex];
            //
            //         GameObject prefabToSpawn = null;
            //
            //         if (currentType == SpotType.EventTerminal)
            //         {
            //             if (node.Type == RoomType.Event && node.EventData != null)
            //                 eventTerminalPrefabs.TryGetValue(node.EventData.EventType, out prefabToSpawn);
            //         }
            //         else
            //             spotPrefabs.TryGetValue(currentType, out prefabToSpawn);
            //
            //         if (prefabToSpawn != null)
            //         {
            //             var faceRotation = selectedSpot.transform.rotation * prefabToSpawn.transform.rotation;
            //
            //             var spawnedPart = _resolver.Instantiate(
            //                 prefabToSpawn, 
            //                 selectedSpot.transform.position, 
            //                 faceRotation, 
            //                 spawnedRoom.transform
            //             );
            //             
            //             spawnedPart.name = $"[Micro] {currentType}";
            //         }
            //         else
            //         {
            //             if (currentType != SpotType.EventTerminal || node.Type == RoomType.Event)
            //             {
            //                 Debug.LogWarning($"[MicroGen] Пропущен спавн для {currentType} в комнате ID:{node.NodeId}. Нет префаба в словаре.");
            //             }
            //         }
            //
            //         foreach (var spot in availableSpots)
            //         {
            //             Destroy(spot.gameObject);
            //         }
            //     }
            // 
        }
    }
}