
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class LevelSpawnerTest_ : MonoBehaviour
    {
        public float roomSizeMultiplier = 10f; 

        [Header("Префабы комнат")]
        [Tooltip("Твой сериализуемый словарь из аддона")]
        public SerializedDictionary<RoomType, GameObject> roomPrefabs;
        public SerializedDictionary<GameEventsType, GameObject> eventPrefabs;
        private void Start()
        {
            var eventsPool = new List<EventRoomDefinition>
            {
                new EventRoomDefinition("flood_1", GameEventsType.FloodBrokenPump, 10),
                new EventRoomDefinition("flood_2", GameEventsType.FloodPipeBreak, 10),
                new EventRoomDefinition("blackout_1", GameEventsType.BlackoutBlowFuse, 15),
                new EventRoomDefinition("other_1", GameEventsType.OtherEvent, 5)
            };

            // 2. РАНДОМИЗИРУЕМ входные параметры уровня
            int randomDefenseRooms = Random.Range(3, 5); 
            int maxSideRoomsCapacity = randomDefenseRooms * 10;
            int randomSideRooms = Random.Range(3, maxSideRoomsCapacity + 1); 
            int randomBudget = Random.Range(70, 130); 

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

                Vector3 spawnPosition = new Vector3(node.X * roomSizeMultiplier, 0, node.Y * roomSizeMultiplier);
                
                Quaternion spawnRotation = Quaternion.identity;

                if (parent != null)
                {
                    Vector3 parentPosition = new Vector3(parent.X * roomSizeMultiplier, 0, parent.Y * roomSizeMultiplier);
                    Vector3 directionToParent = parentPosition - spawnPosition;

                    spawnRotation = Quaternion.FromToRotation(Vector3.right, directionToParent.normalized);
                }

                GameObject prefabToSpawn = null;
                string roomLogName = node.Type.ToString();

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
                    GameObject spawnedRoom = Instantiate(prefabToSpawn, spawnPosition, spawnRotation, this.transform);
                    spawnedRoom.name = $"Room [{node.NodeId}] {roomLogName} ({node.X}, {node.Y})";
                }
                else
                {
                    Debug.LogError($"Не удалось найти префаб для комнаты [ID: {node.NodeId}] типа {node.Type}!");
                }

                foreach (var connectedNode in node.ConnectedNodes)
                {
                    if (!visited.Contains(connectedNode.NodeId))
                    {
                        visited.Add(connectedNode.NodeId);
                        queue.Enqueue((connectedNode, node));
                    }
                }
            }
        }
    }
}