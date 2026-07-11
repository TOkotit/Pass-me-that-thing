using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.LevelGeneration.Editor_Grid;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    /// <summary>
    /// Структура для хранения информации о свободной двери уже размещенной комнаты.
    /// </summary>
    public struct ConnectionPoint
    {
        public Vector3Int GlobalPosition;
        public Vector3Int Direction;
        public RoomsConnectionTypes Type;
    }

    /// <summary>
    /// Данные о физически размещенной комнате на уровне.
    /// </summary>
    public class PlacedRoomData
    {
        public LevelRoom Instance;
        public List<ConnectionPoint> FreeConnections = new List<ConnectionPoint>();
    }

    public class LevelPlacementOrchestrator : MonoBehaviour
    {
        [Header("Ссылки на компоненты")]
        [SerializeField] private RoomDatabase roomDatabase;
        [SerializeField] private LevelGrid levelGrid;
        
        [Header("Родительский объект для комнат")]
        [SerializeField] private Transform levelContainer;

        private Dictionary<RoomNode, PlacedRoomData> _placedRooms = new Dictionary<RoomNode, PlacedRoomData>();
        private System.Random _random = new System.Random();

        /// <summary>
        /// Основной метод запуска генерации.
        /// </summary>
        public void GeneratePhysicalLevel(RoomNode hubNode)
        {
            levelGrid.ClearGrid();
            _placedRooms.Clear();

            if (levelContainer != null)
            {
                foreach (Transform child in levelContainer)
                    Destroy(child.gameObject);
            }

            // 1. Размещение Хаба
            PlaceHub(hubNode);

            // 2. Обход графа и размещение остальных комнат
            var queue = new Queue<RoomNode>();
            var visited = new HashSet<RoomNode>();
            
            queue.Enqueue(hubNode);
            visited.Add(hubNode);

            while (queue.Count > 0)
            {
                var parentNode = queue.Dequeue();
                var parentPlacedData = _placedRooms[parentNode];

                foreach (var childNode in parentNode.ConnectedNodes)
                {
                    if (visited.Contains(childNode)) 
                        continue;

                    bool success = TryPlaceRoom(childNode, parentPlacedData);
                    
                    if (success)
                    {
                        visited.Add(childNode);
                        queue.Enqueue(childNode);
                    }
                    else
                    {
                        Debug.LogWarning($"Не удалось разместить узел ID: {childNode.NodeId} ({childNode.Type}). Тупик или нет места.");
                    }
                }
            }
        }

        private void PlaceHub(RoomNode hubNode)
        {
            var candidates = roomDatabase.GetSuitableRooms(hubNode.Type, hubNode.ConnectedNodes.Count, null, false);
            if (candidates.Count == 0) return;

            var prefab = candidates[_random.Next(candidates.Count)];
            
            // Хаб всегда ставится в (0,0,0) без поворота
            var origin = Vector3Int.zero;
            var rotation = RoomRotation.Deg0;

            InstantiateAndRegisterRoom(hubNode, prefab, origin, rotation);
        }

        private bool TryPlaceRoom(RoomNode childNode, PlacedRoomData parentData)
        {
            GameEventsType? eventType = childNode.Type == RoomType.Event ? childNode.EventData?.EventType : null;
            
            // Ищем подходящие префабы (exactMatch = false, чтобы брать комнаты с запасом дверей)
            var candidates = roomDatabase.GetSuitableRooms(childNode.Type, childNode.ConnectedNodes.Count, eventType, false)
                                         .OrderBy(x => _random.Next())
                                         .ToList();

            foreach (var prefab in candidates)
            {
                // Перебираем все свободные двери родительской комнаты
                for (int pIndex = 0; pIndex < parentData.FreeConnections.Count; pIndex++)
                {
                    var parentConn = parentData.FreeConnections[pIndex];

                    // Перебираем все 4 варианта поворота для новой комнаты
                    for (int r = 0; r < 4; r++)
                    {
                        RoomRotation currentRotation = (RoomRotation)r;
                        var virtualPlates = RoomRotationHelper.GetRotatedPlates(prefab, currentRotation);

                        // Ищем дверь в новой комнате, которая смотрит ровно противоположно родительской
                        var matchingConn = FindMatchingConnection(virtualPlates, parentConn);

                        if (matchingConn.HasValue)
                        {
                            // Вычисляем Origin для новой комнаты
                            var targetGlobalCell = parentConn.GlobalPosition + parentConn.Direction;
                            var calculatedOrigin = targetGlobalCell - matchingConn.Value.LocalPosition;

                            // Проверяем коллизии
                            if (RoomCollisionValidator.IsPlacementValid(levelGrid, prefab, currentRotation, calculatedOrigin))
                            {
                                // Если место свободно — спавним комнату
                                var newRoomData = InstantiateAndRegisterRoom(childNode, prefab, calculatedOrigin, currentRotation);

                                // Удаляем использованную дверь у родителя
                                parentData.FreeConnections.RemoveAt(pIndex);

                                // Удаляем использованную дверь у новой комнаты (чтобы из нее больше не строили)
                                RemoveUsedConnection(newRoomData, targetGlobalCell, -parentConn.Direction);

                                return true; // Комната успешно размещена
                            }
                        }
                    }
                }
            }

            return false; // Ни один префаб не подошел
        }

        private PlacedRoomData InstantiateAndRegisterRoom(RoomNode node, LevelRoom prefab, Vector3Int origin, RoomRotation rotation)
        {
            var rotationQuaternion = GetRotationQuaternion(rotation);
            var worldPos = levelGrid.UnityGrid.CellToWorld(origin);

            var instance = Instantiate(prefab, worldPos, rotationQuaternion, levelContainer);
            instance.name = $"Room_{node.NodeId}_{node.Type}";

            var placedData = new PlacedRoomData { Instance = instance };
            var virtualPlates = RoomRotationHelper.GetRotatedPlates(prefab, rotation);

            // Регистрация занятых ячеек в сетке и сбор всех свободных дверей
            foreach (var plate in virtualPlates)
            {
                var globalPos = origin + plate.LocalPosition;
                levelGrid.SetCellState(globalPos, true);

                ExtractConnections(placedData.FreeConnections, plate, globalPos);
            }

            _placedRooms.Add(node, placedData);
            return placedData;
        }

        private (Vector3Int LocalPosition, RoomsConnectionTypes Type)? FindMatchingConnection(VirtualPlateData[] plates, ConnectionPoint parentConn)
        {
            var targetDirection = -parentConn.Direction; // Ищем противоположное направление

            foreach (var plate in plates)
            {
                if (targetDirection == Vector3Int.forward && plate.ConnectionNorth == parentConn.Type) return (plate.LocalPosition, plate.ConnectionNorth);
                if (targetDirection == Vector3Int.right && plate.ConnectionEast == parentConn.Type) return (plate.LocalPosition, plate.ConnectionEast);
                if (targetDirection == Vector3Int.back && plate.ConnectionSouth == parentConn.Type) return (plate.LocalPosition, plate.ConnectionSouth);
                if (targetDirection == Vector3Int.left && plate.ConnectionWest == parentConn.Type) return (plate.LocalPosition, plate.ConnectionWest);
            }

            return null;
        }

        private void ExtractConnections(List<ConnectionPoint> list, VirtualPlateData plate, Vector3Int globalPos)
        {
            if (plate.ConnectionNorth != RoomsConnectionTypes.None) list.Add(new ConnectionPoint { GlobalPosition = globalPos, Direction = Vector3Int.forward, Type = plate.ConnectionNorth });
            if (plate.ConnectionEast != RoomsConnectionTypes.None) list.Add(new ConnectionPoint { GlobalPosition = globalPos, Direction = Vector3Int.right, Type = plate.ConnectionEast });
            if (plate.ConnectionSouth != RoomsConnectionTypes.None) list.Add(new ConnectionPoint { GlobalPosition = globalPos, Direction = Vector3Int.back, Type = plate.ConnectionSouth });
            if (plate.ConnectionWest != RoomsConnectionTypes.None) list.Add(new ConnectionPoint { GlobalPosition = globalPos, Direction = Vector3Int.left, Type = plate.ConnectionWest });
        }

        private void RemoveUsedConnection(PlacedRoomData roomData, Vector3Int globalPos, Vector3Int direction)
        {
            for (int i = 0; i < roomData.FreeConnections.Count; i++)
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