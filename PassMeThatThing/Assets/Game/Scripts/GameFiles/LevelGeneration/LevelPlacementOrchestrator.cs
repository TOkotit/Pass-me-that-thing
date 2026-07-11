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
        public List<ConnectionPoint> FreeConnections = new List<ConnectionPoint>();
        
        // НОВОЕ: Запоминаем, какие клетки заняла комната, чтобы можно было её чисто удалить
        public List<Vector3Int> OccupiedCells = new List<Vector3Int>();
    }

    public class LevelPlacementOrchestrator : MonoBehaviour
    {
        [Header("Ссылки на компоненты")]
        [SerializeField] private RoomDatabase roomDatabase;
        [SerializeField] private LevelGrid levelGrid;
        [SerializeField] private Transform levelContainer;

        private Dictionary<RoomNode, PlacedRoomData> _placedRooms = new Dictionary<RoomNode, PlacedRoomData>();
        private System.Random _random = new System.Random();

        public void GeneratePhysicalLevel(RoomNode hubNode)
        {
            levelGrid.ClearGrid();
            _placedRooms.Clear();

            if (levelContainer != null)
            {
                foreach (Transform child in levelContainer) Destroy(child.gameObject);
            }

            PlaceHub(hubNode);

            // Очередь теперь хранит пару: (Узел для размещения, Ожидаемый Родитель)
            var queue = new Queue<(RoomNode Node, RoomNode Parent)>();
            var visited = new HashSet<RoomNode>();
            
            visited.Add(hubNode);

            foreach (var child in hubNode.ConnectedNodes)
            {
                queue.Enqueue((child, hubNode));
            }

            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                var childNode = item.Node;
                var parentNode = item.Parent;

                if (visited.Contains(childNode)) continue;

                // Запускаем пайплайн размещения с системой спасения
                bool success = ExecutePlacementPipeline(childNode, parentNode);

                if (success)
                {
                    visited.Add(childNode);
                    foreach (var nextChild in childNode.ConnectedNodes)
                    {
                        if (!visited.Contains(nextChild)) queue.Enqueue((nextChild, childNode));
                    }
                }
                else
                {
                    Debug.LogError($"[КРИТИЧЕСКИЙ СБОЙ] Узел ID {childNode.NodeId} ({childNode.Type}) полностью отброшен. Места нет.");
                }
            }
        }

        private void PlaceHub(RoomNode hubNode)
        {
            var candidates = roomDatabase.GetSuitableRooms(hubNode.Type, hubNode.ConnectedNodes.Count, null, false);
            var prefab = candidates[_random.Next(candidates.Count)];
            InstantiateAndRegisterRoom(hubNode, prefab, Vector3Int.zero, RoomRotation.Deg0);
        }

        // ==========================================
        // ПАЙПЛАЙН РАЗМЕЩЕНИЯ И FALLBACK-СИСТЕМА
        // ==========================================
        private bool ExecutePlacementPipeline(RoomNode nodeToPlace, RoomNode designatedParent)
        {
            bool isEvent = nodeToPlace.Type == RoomType.Event;
            var parentData = _placedRooms[designatedParent];

            // 1. СТАНДАРТНАЯ ПОПЫТКА (Мягкий матчинг: >= дверей у назначенного родителя)
            if (TryPlaceRoom(nodeToPlace, parentData, ignoreDoorCount: false)) 
                return true;

            // --- ЕСЛИ СТАНДАРТНЫЙ ПУТЬ НЕ СРАБОТАЛ ---

            if (!isEvent)
            {
                // МЯГКИЙ FALLBACK ДЛЯ ОБЫЧНЫХ КОМНАТ
                // Ищем ЛЮБУЮ свободную дверь на всем уровне, чтобы прицепить комнату
                if (TryPlaceGlobal(nodeToPlace, ignoreDoorCount: false)) 
                    return true;
                
                // Если места нет вообще нигде — сдаемся. Обычная комната не критична.
                Debug.LogWarning($"Обычная комната ID {nodeToPlace.NodeId} пропущена. Нет места.");
                return false;
            }
            else
            {
                // АГРЕССИВНЫЙ RESCUE-РЕЖИМ ДЛЯ ИВЕНТОВ
                Debug.LogWarning($"Запуск Rescue-режима для Ивента ID {nodeToPlace.NodeId}");

                // Шаг А: Дверная релаксация (Берем тупиковый ивент, игнорируем нужду в сквозном проходе)
                if (TryPlaceRoom(nodeToPlace, parentData, ignoreDoorCount: true)) 
                    return true;

                // Шаг Б: Глобальный поиск с дверной релаксацией
                if (TryPlaceGlobal(nodeToPlace, ignoreDoorCount: true)) 
                    return true;

                // Шаг В: Топологическая рокировка (Swap)
                // Уничтожаем уже построенную обычную комнату и ставим на её место ивент
                if (PerformTopologicalSwap(nodeToPlace))
                    return true;

                return false; // Фатальный сбой (встречается крайне редко)
            }
        }

        // ==========================================
        // ЯДРО РАЗМЕЩЕНИЯ (МАТЕМАТИКА)
        // ==========================================
        private bool TryPlaceRoom(RoomNode node, PlacedRoomData parentData, bool ignoreDoorCount)
        {
            GameEventsType? eventType = node.Type == RoomType.Event ? node.EventData?.EventType : null;
            
            // Если ignoreDoorCount = true, запрашиваем комнату с минимум 1 дверью (любую этого типа)
            int requiredDoors = ignoreDoorCount ? 1 : node.ConnectedNodes.Count;

            var candidates = roomDatabase.GetSuitableRooms(node.Type, requiredDoors, eventType, false)
                                         .OrderBy(x => _random.Next())
                                         .ToList();

            foreach (var prefab in candidates)
            {
                for (int pIndex = 0; pIndex < parentData.FreeConnections.Count; pIndex++)
                {
                    var parentConn = parentData.FreeConnections[pIndex];

                    for (int r = 0; r < 4; r++)
                    {
                        RoomRotation currentRotation = (RoomRotation)r;
                        var virtualPlates = RoomRotationHelper.GetRotatedPlates(prefab, currentRotation);

                        var matchingConn = FindMatchingConnection(virtualPlates, parentConn);

                        if (matchingConn.HasValue)
                        {
                            var targetGlobalCell = parentConn.GlobalPosition + parentConn.Direction;
                            var calculatedOrigin = targetGlobalCell - matchingConn.Value.LocalPosition;

                            if (RoomCollisionValidator.IsPlacementValid(levelGrid, prefab, currentRotation, calculatedOrigin))
                            {
                                var newRoomData = InstantiateAndRegisterRoom(node, prefab, calculatedOrigin, currentRotation);
                                parentData.FreeConnections.RemoveAt(pIndex);
                                RemoveUsedConnection(newRoomData, targetGlobalCell, -parentConn.Direction);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        // Глобальный поиск: перебирает все УЖЕ размещенные комнаты и пытается пристыковаться к ним
        private bool TryPlaceGlobal(RoomNode nodeToPlace, bool ignoreDoorCount)
        {
            var allPlacedNodes = _placedRooms.Keys.OrderBy(x => _random.Next()).ToList();
            
            foreach (var potentialParent in allPlacedNodes)
            {
                var parentData = _placedRooms[potentialParent];
                if (parentData.FreeConnections.Count == 0) continue;

                if (TryPlaceRoom(nodeToPlace, parentData, ignoreDoorCount))
                {
                    Debug.Log($"Узел ID {nodeToPlace.NodeId} пристыкован глобально к узлу ID {potentialParent.NodeId}");
                    return true;
                }
            }
            return false;
        }

        // ==========================================
        // ДЕСТРУКТИВНАЯ ЛОГИКА (SWAP)
        // ==========================================
        private bool PerformTopologicalSwap(RoomNode eventNodeToPlace)
        {
            // Ищем обычную комнату, которую не жалко удалить (предпочтительно тупиковую)
            var sacrificeCandidates = _placedRooms
                .Where(kvp => kvp.Key.Type == RoomType.Regular)
                .OrderBy(kvp => kvp.Value.FreeConnections.Count) // Те, у кого много свободных дверей = тупики (меньше детей)
                .ToList();

            foreach (var sacrifice in sacrificeCandidates)
            {
                var sacrificedNode = sacrifice.Key;
                var sacrificedData = sacrifice.Value;

                // 1. Полностью удаляем жертву из физического мира и освобождаем сетку
                DestroyAndUnregisterRoom(sacrificedNode);

                // 2. Теперь место свободно. Пытаемся глобально воткнуть туда наш Ивент
                // ignoreDoorCount = true, так как нам плевать сколько дверей, лишь бы влез
                if (TryPlaceGlobal(eventNodeToPlace, ignoreDoorCount: true))
                {
                    Debug.LogWarning($"[SWAP] Обычная комната ID {sacrificedNode.NodeId} уничтожена. На её место встал Ивент ID {eventNodeToPlace.NodeId}.");
                    return true;
                }
                else
                {
                    // Если даже после удаления ивент не влез (например, форма префаба другая), 
                    // мы потеряли обычную комнату. Для надежности можно было бы делать бэкап, 
                    // но так как это Regular комната - потеря не критична.
                }
            }
            return false;
        }

        private void DestroyAndUnregisterRoom(RoomNode node)
        {
            if (!_placedRooms.TryGetValue(node, out var data)) return;

            // Освобождаем сетку
            foreach (var cell in data.OccupiedCells)
            {
                levelGrid.SetCellState(cell, false);
            }

            // Уничтожаем объект
            if (data.Instance != null)
            {
                Destroy(data.Instance.gameObject);
            }

            _placedRooms.Remove(node);
        }

        // ==========================================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ (Остались без изменений, кроме OccupiedCells)
        // ==========================================
        private PlacedRoomData InstantiateAndRegisterRoom(RoomNode node, LevelRoom prefab, Vector3Int origin, RoomRotation rotation)
        {
            var rotationQuaternion = GetRotationQuaternion(rotation);
            var worldPos = levelGrid.UnityGrid.CellToWorld(origin);

            var instance = Instantiate(prefab, worldPos, rotationQuaternion, levelContainer);
            instance.name = $"Room_{node.NodeId}_{node.Type}";

            var placedData = new PlacedRoomData { Instance = instance };
            var virtualPlates = RoomRotationHelper.GetRotatedPlates(prefab, rotation);

            foreach (var plate in virtualPlates)
            {
                var globalPos = origin + plate.LocalPosition;
                
                levelGrid.SetCellState(globalPos, true);
                placedData.OccupiedCells.Add(globalPos); // ЗАПОМИНАЕМ КЛЕТКУ

                ExtractConnections(placedData.FreeConnections, plate, globalPos);
            }

            _placedRooms.Add(node, placedData);
            return placedData;
        }

        private (Vector3Int LocalPosition, RoomsConnectionTypes Type)? FindMatchingConnection(VirtualPlateData[] plates, ConnectionPoint parentConn)
        {
            var targetDirection = -parentConn.Direction;
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