
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
        [Tooltip("Твой сериализуемый словарь из аддона")]
        public SerializedDictionary<RoomType, GameObject> roomPrefabs;
        public SerializedDictionary<GameEventsType, GameObject> eventPrefabs;
        
        public SerializedDictionary<SpotType, GameObject> spotPrefabs;
        public SerializedDictionary<GameEventsType, GameObject> eventTerminalPrefabs;
        
        
        [Inject]
        public void Construct(IObjectResolver resolver)
        {
            _resolver = resolver;
        }
        
        private void Start()
        {
            var eventsPool = new List<EventRoomDefinition>
            {
                new EventRoomDefinition("flood_1", GameEventsType.FloodBrokenPump, 10),
                new EventRoomDefinition("flood_2", GameEventsType.FloodPipeBreak, 10),
                new EventRoomDefinition("blackout_1", GameEventsType.BlackoutBlowFuse, 15),
                new EventRoomDefinition("other_1", GameEventsType.OtherEvent, 5)
            };

            var randomDefenseRooms = Random.Range(3, 5); 
            var maxSideRoomsCapacity = randomDefenseRooms * 10;
            var randomSideRooms = Random.Range(3, maxSideRoomsCapacity + 1); 
            var randomBudget = Random.Range(70, 130); 

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

            queue.Enqueue((startNode, null));
            visited.Add(startNode.NodeId);

            while (queue.Count > 0)
            {
                var (node, parent) = queue.Dequeue();

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

                    if (!eventPrefabs.TryGetValue(eventType, out prefabToSpawn))
                    {
                        Debug.LogWarning($"Префаб для ивента {eventType} не найден в словаре eventPrefabs! Пробуем общий префаб.");
                        roomPrefabs.TryGetValue(RoomType.Event, out prefabToSpawn);
                    }
                }
                else
                {
                    roomPrefabs.TryGetValue(node.Type, out prefabToSpawn);
                }

                if (prefabToSpawn != null)
                {
                    var spawnedRoom = _resolver.Instantiate(prefabToSpawn, spawnPosition, spawnRotation, this.transform);
                    spawnedRoom.name = $"Room [{node.NodeId}] {roomLogName} ({node.X}, {node.Y})";

                    ProcessRoomMicroGeneration(spawnedRoom, node);
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
        }
        
        private void ProcessRoomMicroGeneration(GameObject spawnedRoom, RoomNode node)
        {
            var allSpots = spawnedRoom.GetComponentsInChildren<LevelPartSpot>();
            if (allSpots.Length == 0) return;

            var spotsGroups = allSpots.GroupBy(spot => spot.spotType);

            foreach (var group in spotsGroups)
            {
                var currentType = group.Key;
                var availableSpots = group.ToList();

                var randomIndex = Random.Range(0, availableSpots.Count);
                var selectedSpot = availableSpots[randomIndex];

                GameObject prefabToSpawn = null;

                if (currentType == SpotType.EventTerminal)
                {
                    if (node.Type == RoomType.Event && node.EventData != null)
                        eventTerminalPrefabs.TryGetValue(node.EventData.EventType, out prefabToSpawn);
                }
                else
                    spotPrefabs.TryGetValue(currentType, out prefabToSpawn);

                if (prefabToSpawn != null)
                {
                    var faceRotation = selectedSpot.transform.rotation * prefabToSpawn.transform.rotation;

                    var spawnedPart = _resolver.Instantiate(
                        prefabToSpawn, 
                        selectedSpot.transform.position, 
                        faceRotation, 
                        spawnedRoom.transform
                    );
                    
                    spawnedPart.name = $"[Micro] {currentType}";
                }
                else
                {
                    if (currentType != SpotType.EventTerminal || node.Type == RoomType.Event)
                    {
                        Debug.LogWarning($"[MicroGen] Пропущен спавн для {currentType} в комнате ID:{node.NodeId}. Нет префаба в словаре.");
                    }
                }

                foreach (var spot in availableSpots)
                {
                    Destroy(spot.gameObject);
                }
            }
        }
    }
}