using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.LevelGeneration.Editor_Grid;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    
    /// <summary>
    /// Отвечает за логический и математический расчёт структуры на виртуальной стеке.<br/>
    /// Все размещения проводит виртуально без размещения префабов.
    /// </summary>
    public class LevelGeneratorSolver
    {
        /// <summary>
        /// Ограничитель количества итераций генерации.
        /// Предотвращает бесконечные циклы
        /// </summary>
        private const int MAX_CLUSTER_PLACEMENT_ATTEMPTS = 15;

        /// <summary>
        /// Массив базовых направлений перемещения по сетке.
        /// </summary>
        private static readonly Vector3Int[] SearchDirections = {
            new(1, 0, 0), new(-1, 0, 0),
            new(0, 0, 1), new(0, 0, -1)
        };

        /// <summary>
        /// Узел для алгоритма поиска пути (BFS).
        /// </summary>
        private class PathNode
        {
            public Vector3Int Cell;
            public PathNode Parent;
            public int Depth;
        }

        private readonly RoomDatabase _roomDatabase;
        private readonly LevelGrid _levelGrid;
        private readonly GameObject _wallPrefab;
        private readonly GameObject _wallWithPassagePrefab;

        private System.Random _random;
        private int _virtualRoomIdCounter = 1;

        private readonly List<PlacedRoomDataCluster> _allPlacedRooms = new();
        private readonly List<RoomCluster> _clusters = new();
        private readonly List<(PlacedRoomDataCluster Room, ConnectionPoint Conn)> _usedConnections = new();
        private readonly List<VirtualWallData> _plannedWalls = new();
        private readonly List<(PlacedRoomDataCluster Room, ConnectionPoint Conn)> _clusterExits = new();
        
        public IReadOnlyList<PlacedRoomDataCluster> AllPlacedRooms => _allPlacedRooms;
        public IReadOnlyList<RoomCluster> Clusters => _clusters;
        public IReadOnlyList<(PlacedRoomDataCluster Room, ConnectionPoint Conn)> UsedConnections => _usedConnections;
        public IReadOnlyList<VirtualWallData> PlannedWalls => _plannedWalls;

        public List<(PlacedRoomDataCluster Room, ConnectionPoint Conn)> ClusterExits =>  _clusterExits;

        /// <summary>
        /// </summary>
        /// <param name="roomDatabase">База данных префабов комнат</param>
        /// <param name="levelGrid">Сетка уровня</param>
        /// <param name="wallPrefab">Префаб стены</param>
        /// <param name="wallWithPassagePrefab">Префаб дверей</param>
        public LevelGeneratorSolver(RoomDatabase roomDatabase, LevelGrid levelGrid, GameObject wallPrefab, GameObject wallWithPassagePrefab)
        {
            _roomDatabase = roomDatabase;
            _levelGrid = levelGrid;
            _wallPrefab = wallPrefab;
            _wallWithPassagePrefab = wallWithPassagePrefab;
        }

        /// <summary>
        /// Очистка всех списков и ресет
        /// </summary>
        public void Reset()
        {
            _allPlacedRooms.Clear();
            _clusters.Clear();
            _usedConnections.Clear();
            _plannedWalls.Clear();
            _clusterExits.Clear();
            _virtualRoomIdCounter = 1;
        }

        /// <summary>
        /// Главный конвейер алгоритм.<br/>
        /// Инициализирует _random, очищает данные.<br/>
        /// Размещает ядро в координатах (0, 0) по стеке. После итерирует и ращмещает остальные кластеры со случайным смещением.<br/>
        /// Вызывает прокладку тоннелей <see cref="ConnectAllFreeExits"/>.
        /// Присоединение ангара эвакуации <see cref="PlaceRecoveryHangar"/>.
        /// Расстановку стен <see cref="BlockUnusedExits"/> и дверей<see cref="PlaceUsedExitPassages"/> 
        /// </summary>
        /// <param name="clusters">Кластеры для размещения</param>
        /// <param name="seed">Сид</param>
        /// <returns>Возвращает true в случае успешной генерации. False в случае ошибки</returns>
        public bool GenerateVirtualLevel(List<RoomCluster> clusters, int seed)
        {
            _random = new System.Random(seed);
            Reset();

            if (clusters == null || clusters.Count == 0) return false;

            _clusters.AddRange(clusters);

            var coreCluster = clusters[0];
            if (!PlaceCoreCluster(coreCluster))
            {
                Debug.LogError("[GENERATOR] Can't place the core after a few attempts.");
                return false;
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

            ValidateAndSanitizeConnections();
            
            BlockUnusedExits();
            PlaceUsedExitPassages();

            return true;
        }

        /// <summary>
        /// Обёртка для размещения корневого кластера
        /// </summary>
        /// <param name="coreCluster">Корневой <see cref="RoomCluster"/></param>
        /// <returns>Успешность размещения</returns>
        private bool PlaceCoreCluster(RoomCluster coreCluster) => TryPlaceCluster(coreCluster, true, Vector3Int.zero);

        /// <summary>
        /// Обёртка для размещения обычного кластера
        /// </summary>
        /// <param name="cluster"><see cref="RoomCluster"/> из списпка</param>
        /// <param name="direction">Вектор направления для поиска свободного места</param>
        private void PlaceCluster(RoomCluster cluster, Vector3Int direction) => TryPlaceCluster(cluster, false, direction);
        
        /// <summary>
        /// Выполняет попытку размещения кластера комнат на виртуальной сетке уровня.<br/>
        /// Запускает цикл с лимитом <see cref="MAX_CLUSTER_PLACEMENT_ATTEMPTS"/> попыток.<br/>
        /// Если <see cref="isCore"/>: ищется узел с типом CommandCenter. Если отсутствует — возвращается false.
        /// Из <see cref="RoomDatabase"/> запрашиваются подходящие комнаты для стартового узла.
        /// Для ядра список сортируется по имени префаба. При пустом списке кандидатов возвращается false.<br/>
        /// Из списка кандидатов выбирается случайный префаб. Метод <see cref="RegisterVirtualRoom"/> фиксирует его в сетке <see cref="LevelGrid"/>.
        /// Комната добавляется в инициализированный список <see cref="placedRooms"/><br/>
        /// Запускается цикл попыток пристыковки через метод <see cref="TryPlaceRoomInCluster"/>.
        /// Если <see cref="TryPlaceRoomInCluster"/> возвращает false, цикл прерывается, флаг success переключается в false. Вызывается откат <see cref="UndoClusterPlacement"/><br/>
        /// Если все комнаты кластера успешно размещены на сетке, вызывается проверка <see cref="ValidateClusterIntegrity"/>. При ее успешном прохождении метод завершает работу и возвращает true.<br/>
        /// Если на этапах пристыковки или валидации произошла ошибка, вызывается <see cref="UndoClusterPlacement"/><br/>
        /// </summary>
        /// <param name="cluster">Объект кластера, содержащий список комнат для размещения.</param>
        /// <param name="isCore">Булевый флаг. При значении true применяется логика для центрального кластера, при false — для второстепенного.</param>
        /// <param name="direction">Вектор направления для поиска свободного места(используется только если isCore имеет значение false)</param>
        /// <returns>Успешное или не успешное размещение кластера</returns>
        private bool TryPlaceCluster(RoomCluster cluster, bool isCore, Vector3Int direction)
        {
            for (var attempt = 0; attempt < MAX_CLUSTER_PLACEMENT_ATTEMPTS; attempt++)
            {
                var startNode = isCore 
                    ? cluster.Rooms.FirstOrDefault(r => r.Type == RoomType.CommandCenter) 
                    : cluster.Rooms[0];

                if (startNode == null) return false;

                var candidates = _roomDatabase.GetSuitableRooms(startNode.Type, 1, false);
                if (isCore) 
                {
                    candidates = candidates.OrderBy(r => r.PrefabGameObject.name).ToList();
                }

                if (candidates.Count == 0) return false;

                Vector3Int origin;
                RoomRotation rotation;

                if (isCore)
                {
                    origin = Vector3Int.zero;
                    rotation = RoomRotation.Deg0;
                }
                else
                {
                    var foundOrigin = FindFreeSpaceAroundCore(candidates, direction);
                    if (!foundOrigin.HasValue) continue;
                    
                    origin = foundOrigin.Value;
                    rotation = (RoomRotation)_random.Next(4);
                }

                var entry = candidates[_random.Next(candidates.Count)];
                var startData = RegisterVirtualRoom(entry, origin, rotation, cluster);

                var placedRooms = new List<PlacedRoomDataCluster> { startData };
                var remainingRooms = isCore 
                    ? cluster.Rooms.Where(r => r != startNode) 
                    : cluster.Rooms.Skip(1);
                
                var success = true;

                foreach (var roomNode in remainingRooms)
                {
                    if (!TryPlaceRoomInCluster(roomNode, placedRooms, cluster))
                    {
                        success = false;
                        break;
                    }
                }

                if (success && ValidateClusterIntegrity(placedRooms))
                {
                    return true;
                }

                UndoClusterPlacement(placedRooms);
            }
            return false;
        }

        /// <summary>
        /// Установка точки ангара.<br/>
        /// Сравнивает дистанцию от ядра уровня до каждого кластера, имеющего минимум 2 свободных выхода. К свободному выходу самого удаленного кластера подстыковывает случайный префаб типа RecoveryHangar.
        /// </summary>
        /// <param name="clusters"></param>
        private void PlaceRecoveryHangar(List<RoomCluster> clusters)
        {
            if (clusters == null || clusters.Count < 2) return;

            var commandCenterRoom = _allPlacedRooms.FirstOrDefault(r => r.RoomType == RoomType.CommandCenter);
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
                return;

            var hangarCandidates = _roomDatabase.GetSuitableRooms(RoomType.RecoveryHangar, 1, false);
            if (hangarCandidates.Count == 0)
                return;

            var exitOptions = _allPlacedRooms
                .Where(r => r.Cluster == farthestCluster)
                .SelectMany(r => r.FreeConnections.Select(c => (Room: r, Conn: c)))
                .Where(x => !_levelGrid.IsCellOccupied(x.Conn.GlobalPosition + x.Conn.Direction))
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

                        if (!RoomCollisionValidator.IsPlacementValid(_levelGrid, entry, rot, origin)) continue;

                        RegisterVirtualRoom(entry, origin, rot, farthestCluster);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Рассчитывает манхэттенское расстояние между двумя точками на <see cref="LevelGrid"/>
        /// </summary>
        /// <param name="a">Первая точка</param>
        /// <param name="b">Вторая точка</param>
        /// <returns>Расстояние</returns>
        private static int GridDistance(Vector3Int a, Vector3Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);

        /// <summary>
        /// Проверка пригодности собранного кластера.<br/>
        /// Считает суммарное количество свободных дверей кластера. Исключает двери, выходящие в тупиковые зоны сетки <see cref="IsCellAWell"/>. Требует наличие минимум двух валидных выходов.
        /// </summary>
        /// <param name="clusterRooms">Данные о размещённых кластерах</param>
        /// <returns>Результат проверки: true или false</returns>
        private bool ValidateClusterIntegrity(List<PlacedRoomDataCluster> clusterRooms)
        {
            var allFreeConnections = clusterRooms.SelectMany(r => r.FreeConnections).ToList();

            if (allFreeConnections.Count < 2) return false;

            var validConnectionsCount = 0;

            foreach (var connection in allFreeConnections)
            {
                var targetCell = connection.GlobalPosition + connection.Direction;
                if (_levelGrid.IsCellOccupied(targetCell)) continue;

                if (!IsCellAWell(targetCell))
                {
                    validConnectionsCount++;
                }
            }

            return validConnectionsCount >= 2;
        }

        /// <summary>
        /// Проверяет то, не выходит ли выход из комнаты в замкнутое пространство.<br/>
        /// Делает обход в ширину. Если пространство вокруг больше <see cref="maxPocketSize"/> пространство считается открытым.
        /// </summary>
        /// <param name="emptyCellPos">Проверяемая клетка граничащая с выходом</param>
        /// <param name="maxPocketSize">Количество клеток включительно до которых пространство считается закрытым</param>
        /// <returns>Результат проверки: true/false</returns>
        private bool IsCellAWell(Vector3Int emptyCellPos, int maxPocketSize = 5)
        {
            if (_levelGrid.IsCellOccupied(emptyCellPos))
            {
                return false;
            }

            var visited = new HashSet<Vector3Int>();
            var queue = new Queue<Vector3Int>();

            queue.Enqueue(emptyCellPos);
            visited.Add(emptyCellPos);

            var directions = new[] { Vector3Int.forward, Vector3Int.back, Vector3Int.left, Vector3Int.right };

            while (queue.Count > 0)
            {
                if (visited.Count > maxPocketSize)
                {
                    return false; 
                }

                var currentCell = queue.Dequeue();

                foreach (var dir in directions)
                {
                    var neighborPos = currentCell + dir;

                    if (!_levelGrid.IsCellOccupied(neighborPos) && visited.Add(neighborPos))
                    {
                        queue.Enqueue(neighborPos);
                    }
                }
            }

            return true; 
        }

        /// <summary>
        /// Откат ошибочного размещения.<br/>
        /// Удаляет комнаты из списка <see cref="_allPlacedRooms"/>, очищает их клетки в <see cref="_levelGrid"/> и удаляет связанные с ними соединения из <see cref="_usedConnections"/>
        /// </summary>
        /// <param name="placedRoomsToUndo">Данные о кластере комнат</param>
        private void UndoClusterPlacement(List<PlacedRoomDataCluster> placedRoomsToUndo)
        {
            foreach (var roomData in placedRoomsToUndo)
            {
                foreach (var cellPos in roomData.OccupiedCells)
                {
                    _levelGrid.SetCellState(cellPos, false);
                }
                _usedConnections.RemoveAll(uc => uc.Room == roomData);
                _allPlacedRooms.Remove(roomData);
            }

            placedRoomsToUndo.Clear();
        }

        /// <summary>
        /// Пристыковка одной комнаты к существующему кластеру.<br/>
        /// Перебирает свободные двери уже установленных комнат. Сопоставляет их со свободными дверями префаба новой комнаты с учетом 4 вариантов поворота.<br/>
        /// Проверяет наличие коллизий в сетке и изоляцию от клеток чужих кластеров <see cref="IsSpaceIsolatedFromOtherClusters"/>.
        /// </summary>
        /// <param name="nodeToPlace">Нода комнаты для размещения</param>
        /// <param name="clusterPlacedRooms">Уже размещённые комнаты</param>
        /// <param name="cluster">Кластер к которому стыкуется комната</param>
        /// <returns></returns>
        private bool TryPlaceRoomInCluster(RoomNode nodeToPlace, List<PlacedRoomDataCluster> clusterPlacedRooms, RoomCluster cluster)
        {
            var candidates = _roomDatabase.GetSuitableRooms(nodeToPlace.Type, 1, false)
                .OrderBy(_ => _random.Next()).ToList();

            var otherClustersCells = _allPlacedRooms
                .Where(r => r.Cluster != cluster)
                .SelectMany(r => r.OccupiedCells)
                .ToHashSet();

            var validPlacements = new List<(RoomDataEntry entry, ConnectionPoint parentConn, RoomRotation rot, Vector3Int origin)>();

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

                            if (!RoomCollisionValidator.IsPlacementValid(_levelGrid, entry, rot, origin)) continue;
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
            var newRoomData = RegisterVirtualRoom(selected.entry, selected.origin, selected.rot, cluster);

            clusterPlacedRooms.Add(newRoomData);
            return true;
        }

        /// <summary>
        /// Поиск точки спавна нового кластера.<br/>
        /// Вычисляет внешние границы клеток центрального кластера. Совершает отступ в выбранном направлении на 3, 4 или 5 клеток. Проверяет каждую целевую клетку на возможность размещения там новой комнаты без нарушения изоляции.<br/>
        /// </summary>
        /// <param name="candidates">Список <see cref="RoomDataEntry"/></param>
        /// <param name="preferredDirection">Направление отступа</param>
        /// <returns>Координаты клетки для размещения</returns>
        private Vector3Int? FindFreeSpaceAroundCore(List<RoomDataEntry> candidates, Vector3Int preferredDirection)
        {
            if (_allPlacedRooms.Count == 0) return Vector3Int.zero;

            var coreRoom = _allPlacedRooms.FirstOrDefault(r => r.RoomType == RoomType.CommandCenter);
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

                                    if (RoomCollisionValidator.IsPlacementValid(_levelGrid, prefab, rotation, potentialOrigin))
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

        /// <summary>
        /// Проверяет соблюдение дистанции между кластерами.<br/>
        /// Перебирает клетки, занимаемые комнатой. Проверяет, что в радиусе <see cref="minDistance"/> вокруг каждой клетки нет клеток, принадлежащих другим кластерам.
        /// </summary>
        /// <param name="origin">Ориджин комнаты</param>
        /// <param name="entry">Данные комнаты</param>
        /// <param name="rotation">Поворот комнаты</param>
        /// <param name="otherClustersCells">Другие кластеры</param>
        /// <param name="minDistance">Минимальное заданное расстояние</param>
        /// <returns>Результат проверки, true или false</returns>
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

        /// <summary>
        /// Регистрация установленной комнаты. Виртуальное размещение её на сетке <see cref="LevelGrid"/><br/>
        /// Создает экземпляр <see cref="PlacedRoomDataCluster"/>. Рассчитывает глобальные координаты занимаемых клеток и дверей.<br/>
        /// Обновляет <see cref="_levelGrid"/> данными о занятых ячейках. Вызывает автоматическое связывание дверей внутри кластера <see cref="TryConnectAdjacentDoorsWithinCluster"/>.<br/>
        /// </summary>
        /// <param name="entry">Данные о префабе и компоненте комнаты</param>
        /// <param name="origin">Ориджин комнаты на сетке</param>
        /// <param name="rotation">Поворот комнаты на сетке</param>
        /// <param name="cluster">Кластер к которому относится комната</param>
        /// <returns>Возвращает объект данных о комнате</returns>
        private PlacedRoomDataCluster RegisterVirtualRoom(RoomDataEntry entry, Vector3Int origin, RoomRotation rotation, RoomCluster cluster)
        {
            var roomComp = entry.PrefabGameObject.GetComponent<LevelRoom>();
            var roomType = roomComp ? roomComp.RoomType : RoomType.None;
            var virtualId = _virtualRoomIdCounter++;

            var data = new PlacedRoomDataCluster
            {
                Entry = entry,
                RoomType = roomType,
                Origin = origin,
                Rotation = rotation,
                Cluster = cluster,
                RoomId = virtualId
            };

            var virtualPlates = RoomRotationHelper.GetRotatedPlates(entry, rotation);

            foreach (var plate in virtualPlates)
            {
                var globalPos = origin + plate.LocalPosition;
                var doorDirs = plate.Doors.Select(d => d.GlobalDirection).ToList();

                _levelGrid.SetCellState(globalPos, true, doorDirs, virtualId, cluster.Id, roomType);

                data.OccupiedCells.Add(globalPos);

                foreach (var door in plate.Doors)
                {
                    data.FreeConnections.Add(new ConnectionPoint
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

        /// <summary>
        /// Внутренняя связность кластера.<br/>
        /// Устанавливает прямые соединения между дверями смежных комнат, принадлежащих одному кластеру, удаляя их из списка свободных выходов.
        /// </summary>
        /// <param name="newRoomData">Данные комнаты</param>
        /// <param name="cluster">Данные кластера</param>
        private void TryConnectAdjacentDoorsWithinCluster(
            PlacedRoomDataCluster newRoomData,
            RoomCluster cluster)
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
        
        /// <summary>
        /// Поиск стыковочного узла.<br/>
        /// Ищет на <see cref="VirtualPlateData"/> префаба дверь, вектор направления которой инвертирован по отношению к целевой двери <see cref="parentConn"/>.
        /// </summary>
        /// <param name="plates">Массив данных с данными об отдельных плейтах комнат</param>
        /// <param name="parentConn">Точка соединения коматы</param>
        /// <returns>Точка соединения</returns>
        private Vector3Int? FindMatchingConnection(VirtualPlateData[] plates, ConnectionPoint parentConn)
        {
            var targetDirection = -parentConn.Direction;
            foreach (var plate in plates)
            {
                if (plate.Doors.Any(door => door.GlobalDirection == targetDirection))
                    return plate.LocalPosition;
            }
            return null;
        }
        
        /// <summary>
        /// Фильтрация свободных выходов
        /// </summary>
        /// <param name="clusterFilter">Фильтр для поиска</param>
        /// <returns>Список свободных выходов List&lt;<see cref="PlacedRoomDataCluster"/>, <see cref="ConnectionPoint"/>&gt;</returns>
        private List<(PlacedRoomDataCluster Room, ConnectionPoint Conn)> GetFreeExits(Func<RoomCluster, bool> clusterFilter = null, bool excludeBlocked = false)
        {
            return _allPlacedRooms
                .Where(r => r.Entry.PrefabGameObject &&
                            r.RoomType is not (RoomType.CommandCenter or RoomType.RecoveryHangar))
                .Where(r => clusterFilter == null || clusterFilter(r.Cluster))
                .SelectMany(r => r.FreeConnections.Select(c => (Room: r, Conn: c)))
                .Where(x => !excludeBlocked || !_levelGrid.IsCellOccupied(x.Conn.GlobalPosition + x.Conn.Direction))
                .ToList();
        }

        /// <summary>
        /// Построение сети переходов.<br/>
        /// Управляет объединением выходов между кластерами. Сначала объединяет соприкасающиеся двери <see cref="ConnectAdjacentDoorsGlobally"/>,
        /// затем итеративно запускает поиск путей для прокладки туннелей <see cref="ConnectFreeExitPairs"/>,
        /// постепенно увеличивая лимит длины пути, и в конце устраняет изолированные группы кластеров <see cref="EnsureAllClustersConnected"/>.
        /// </summary>
        private void ConnectAllFreeExits()
        {
            var clusterLinks = new Dictionary<RoomCluster, HashSet<RoomCluster>>();

            ConnectAdjacentDoorsGlobally(GetFreeExits(), clusterLinks);

            var tunnelPrefabs = _roomDatabase.GetSuitableRooms(RoomType.TechnicalTunnels, 1, false);
            if (tunnelPrefabs == null || tunnelPrefabs.Count == 0) return;

            bool connectionsAdded;
            var currentMaxDepth = 2;
            var absoluteMaxDepth = 14;

            do
            {
                connectionsAdded = false;
                var currentExits = GetFreeExits(null, true);
                var freeExitsBefore = currentExits.Count;

                ConnectFreeExitPairs(currentExits, tunnelPrefabs, clusterLinks, currentMaxDepth);

                var freeExitsAfter = GetFreeExits(null, true).Count;
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

        /// <summary>
        /// Поиск пути алгоритмом волнового обхода.<br/>
        /// Реализует BFS от клетки старта. Добавляет соседние свободные клетки в очередь с увеличением глубины.<br/>
        /// Если найдена целевая координата из словаря <see cref="candidateTargets"/>, восстанавливает маршрут, двигаясь по ссылкам. Передает маршрут в логику спавна.
        /// </summary>
        /// <param name="startExit">Стартовая точка</param>
        /// <param name="candidateTargets">Цели</param>
        /// <param name="tunnelPrefabs">Префаб тоннеля</param>
        /// <param name="clusterLinks">Данные о кластерах</param>
        /// <param name="maxPathLength">Максимальная длина тоннеля (14 по умолчанию)</param>
        /// <returns>Результат поиска</returns>
        private bool TryLinkExitToTargets(
            (PlacedRoomDataCluster Room, ConnectionPoint Conn) startExit,
            List<(PlacedRoomDataCluster Room, ConnectionPoint Conn)> candidateTargets,
            List<RoomDataEntry> tunnelPrefabs,
            Dictionary<RoomCluster, HashSet<RoomCluster>> clusterLinks,
            int maxPathLength = 14)
        {
            var targetDict = candidateTargets
                .Where(x => x.Room.Cluster != startExit.Room.Cluster)
                .GroupBy(x => x.Conn.GlobalPosition + x.Conn.Direction)
                .ToDictionary(g => g.Key, g => g.First());

            return TryLinkExitViaPath(startExit, targetDict, tunnelPrefabs, clusterLinks, maxPathLength, out _);
        }

        /// <summary>
        /// Общий поиск пути алгоритмом волнового обхода (BFS) от стартовой клетки до одной из целевых клеток словаря.<br/>
        /// Вынесен из дублировавшихся <see cref="TryLinkExitToTargets"/> и <see cref="ConnectFreeExitPairs"/>.
        /// </summary>
        /// <param name="startCell">Клетка старта пути</param>
        /// <param name="targetDict">Словарь целевых клеток → связанные с ними выходы</param>
        /// <param name="maxPathLength">Максимальная длина пути</param>
        /// <param name="path">Найденный маршрут (валиден только при успехе)</param>
        /// <param name="foundTarget">Найденная целевая точка соединения (валидна только при успехе)</param>
        /// <returns>Найден ли путь до одной из целей</returns>
        private bool TryFindPath(
            Vector3Int startCell,
            Dictionary<Vector3Int, (PlacedRoomDataCluster Room, ConnectionPoint Conn)> targetDict,
            int maxPathLength,
            out List<Vector3Int> path,
            out (PlacedRoomDataCluster Room, ConnectionPoint Conn) foundTarget)
        {
            path = null;
            foundTarget = default;

            if (targetDict.Count == 0) return false;
            if (_levelGrid.IsCellOccupied(startCell) && !targetDict.ContainsKey(startCell)) return false;

            var queue = new Queue<PathNode>();
            var visited = new HashSet<Vector3Int>();
            queue.Enqueue(new PathNode { Cell = startCell, Parent = null, Depth = 1 });
            visited.Add(startCell);

            PathNode endNode = null;

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

                    if (_levelGrid.IsCellOccupied(nextCell) && !targetDict.ContainsKey(nextCell))
                        continue;

                    visited.Add(nextCell);
                    queue.Enqueue(new PathNode { Cell = nextCell, Parent = curr, Depth = curr.Depth + 1 });
                }
            }

            if (endNode == null) return false;

            var result = new List<Vector3Int>();
            var node = endNode;
            while (node != null)
            {
                result.Add(node.Cell);
                node = node.Parent;
            }
            result.Reverse();

            if (result.Count > maxPathLength) return false;

            path = result;
            return true;
        }

        /// <summary>
        /// Общая логика стыковки выхода с целью: поиск пути (<see cref="TryFindPath"/>), простановка туннелей вдоль него,
        /// регистрация связи кластеров и обновление списков соединений.<br/>
        /// Вынесена из дублировавшихся <see cref="TryLinkExitToTargets"/> и <see cref="ConnectFreeExitPairs"/>.
        /// </summary>
        /// <param name="startExit">Стартовый выход</param>
        /// <param name="targetDict">Словарь целевых клеток → связанные с ними выходы</param>
        /// <param name="tunnelPrefabs">Префабы тоннелей</param>
        /// <param name="clusterLinks">Словарь связей кластеров</param>
        /// <param name="maxPathLength">Максимальная длина тоннеля</param>
        /// <param name="connectedTarget">Выход, с которым удалось состыковаться (валиден только при успехе)</param>
        /// <returns>Успешность стыковки</returns>
        private bool TryLinkExitViaPath(
            (PlacedRoomDataCluster Room, ConnectionPoint Conn) startExit,
            Dictionary<Vector3Int, (PlacedRoomDataCluster Room, ConnectionPoint Conn)> targetDict,
            List<RoomDataEntry> tunnelPrefabs,
            Dictionary<RoomCluster, HashSet<RoomCluster>> clusterLinks,
            int maxPathLength,
            out (PlacedRoomDataCluster Room, ConnectionPoint Conn) connectedTarget)
        {
            connectedTarget = default;

            var startCell = startExit.Conn.GlobalPosition + startExit.Conn.Direction;

            if (!TryFindPath(startCell, targetDict, maxPathLength, out var path, out var foundTarget))
                return false;

            var targetRoom = foundTarget.Room;
            var targetConn = foundTarget.Conn;

            if (!TryPlaceTunnelsAlongPath(path, tunnelPrefabs, startExit.Room, startExit.Conn, targetConn))
                return false;

            RegisterClusterLink(clusterLinks, startExit.Room.Cluster, targetRoom.Cluster);

            startExit.Room.FreeConnections.Remove(startExit.Conn);
            targetRoom.FreeConnections.Remove(targetConn);
            _usedConnections.Add((startExit.Room, startExit.Conn));
            _usedConnections.Add((targetRoom, targetConn));
            _clusterExits.Add((startExit.Room, startExit.Conn));
            _clusterExits.Add((targetRoom, targetConn));

            connectedTarget = foundTarget;
            return true;
        }
        
        /// <summary>
        /// Последовательно пытается проложить путь от любого выхода из списка <see cref="ownExits"/> к любому выходу из списка <see cref="targets"/>
        /// </summary>
        /// <param name="ownExits">Первый список выходов</param>
        /// <param name="targets">Второй список выходов</param>
        /// <param name="tunnelPrefabs">Префаб тоннеля</param>
        /// <param name="clusterLinks">Словарь с данными кластеров</param>
        /// <returns>Результат поиска true/false</returns>
        private bool TryConnectFirstAvailable(
            List<(PlacedRoomDataCluster Room, ConnectionPoint Conn)> ownExits,
            List<(PlacedRoomDataCluster Room, ConnectionPoint Conn)> targets,
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
        

        /// <summary>
        /// Прямая состыковка смежных дверей.<br/>
        /// Перебирает все свободные выходы. Если выходы разных комнат находятся в соседних клетках и смотрят друг на друга, они помечаются как соединенные без спавна туннелей. Фиксирует связь между их кластерами.
        /// </summary>
        /// <param name="allFreeConnections">Данные кластера</param>
        /// <param name="clusterLinks">Точка соединения</param>
        private void ConnectAdjacentDoorsGlobally(
            List<(PlacedRoomDataCluster Room, ConnectionPoint Conn)> allFreeConnections,
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
                            _clusterExits.Add((connAData.Room, connAData.Conn));
                            _clusterExits.Add((connBData.Room, connBData.Conn));
                        }

                        allFreeConnections.RemoveAt(i);
                        allFreeConnections.RemoveAt(j);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Маршрутизация туннелей.<br/>
        /// Перебирает свободные выходы. Для каждого ищет кратчайший путь (до <see cref="maxPathLength"/>) алгоритмом BFS по пустым клеткам сетки до любого свободного выхода чужого кластера. При успехе вызывает метод установки префабов туннелей.
        /// </summary>
        /// <param name="allFreeConnections">Данные кластера с точкой соединения</param>
        /// <param name="tunnelPrefabs">Данные о тоннеле</param>
        /// <param name="clusterLinks">Словарь с кластерами</param>
        /// <param name="maxPathLength">максимальная длинна тоннеля</param>
        private void ConnectFreeExitPairs(
            List<(PlacedRoomDataCluster Room, ConnectionPoint Conn)> allFreeConnections,
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

                if (TryLinkExitViaPath(startData, targetDict, tunnelPrefabs, clusterLinks, maxPathLength, out var connectedTarget))
                {
                    allFreeConnections.Remove(connectedTarget);
                    allFreeConnections.Remove(startData);
                }
                else
                {
                    i++;
                }
            }
        }

        /// <summary>
        /// Преобразование маршрута сетки в префабы туннелей.<br/>
        /// Идет по вычисленному списку координат. Подбирает префабы туннелей (предпочитая длинные сегменты), проверяя совпадение их входных/выходных дверей с вектором маршрута.
        /// Записывает туннели в <see cref="_levelGrid"/> и прикрепляет к объекту комнаты <see cref="PlacedRoomDataCluster.AttachedTunnels"/>.
        /// </summary>
        /// <param name="path">Список точек</param>
        /// <param name="tunnelPrefabs">Список префабов тоннелей</param>
        /// <param name="ownerData">Данные родительского кластера</param>
        /// <param name="startConn">Начало соединения</param>
        /// <param name="endConn">Конец соединения</param>
        /// <returns>Успешность попытки поставить префабы</returns>
        private bool TryPlaceTunnelsAlongPath(List<Vector3Int> path,
            List<RoomDataEntry> tunnelPrefabs,
            PlacedRoomDataCluster ownerData,
            ConnectionPoint startConn,
            ConnectionPoint endConn)
        {
            var sortedPrefabs = tunnelPrefabs
                .OrderByDescending(p => RoomRotationHelper.GetRotatedPlates(p, RoomRotation.Deg0).Length)
                .ThenBy(p => p.PrefabGameObject.name)
                .ToList();

            var virtualTunnels = new List<VirtualTunnelData>();
            var modifiedCells = new List<Vector3Int>();

            var i = 0;
            while (i < path.Count)
            {
                var cell = path[i];
                if (_levelGrid.IsCellOccupied(cell))
                {
                    i++;
                    continue;
                }

                var placed = false;
                var prevCell = i == 0 ? startConn.GlobalPosition : path[i - 1];

                foreach (var entry in sortedPrefabs)
                {
                    for (var r = 0; r < 4; r++)
                    {
                        var rot = (RoomRotation)r;
                        var plates = RoomRotationHelper.GetRotatedPlates(entry, rot);
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

                        if (matchPath && hasPrevDoor && hasNextDoor && RoomCollisionValidator.IsPlacementValid(_levelGrid, entry, rot, cell))
                        {
                            var tunnelData = new VirtualTunnelData { Entry = entry, Origin = cell, Rotation = rot };
                            virtualTunnels.Add(tunnelData);

                            var tunnelId = _virtualRoomIdCounter++;
                            foreach (var p in plates)
                            {
                                var globalPos = cell + p.LocalPosition;
                                var doorDirs = p.Doors.Select(d => d.GlobalDirection).ToList();
                                _levelGrid.SetCellState(globalPos, true, doorDirs, tunnelId, ownerData.Cluster.Id, RoomType.TechnicalTunnels);
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
                    foreach (var modifiedCell in modifiedCells)
                    {
                        _levelGrid.SetCellState(modifiedCell, false);
                    }
                    return false;
                }
                i++;
            }

            ownerData.AttachedTunnels.AddRange(virtualTunnels);
            return true;
        }

        /// <summary>
        /// Обновление графа уровня.<br/>
        /// Добавляет двунаправленную запись о соединении двух кластеров в структуру Dictionary&lt;<see cref="RoomCluster"/>, HashSet&lt;<see cref="RoomCluster"/>&gt;&gt;.
        /// </summary>
        /// <param name="clusterLinks">Словарь</param>
        /// <param name="a">Первый кластер</param>
        /// <param name="b">Второй кластер</param>
        private static void RegisterClusterLink(
            Dictionary<RoomCluster, HashSet<RoomCluster>> clusterLinks,
            RoomCluster a,
            RoomCluster b)
        {
            if (!clusterLinks.TryGetValue(a, out var setA)) clusterLinks[a] = setA = new HashSet<RoomCluster>();
            setA.Add(b);

            if (!clusterLinks.TryGetValue(b, out var setB)) clusterLinks[b] = setB = new HashSet<RoomCluster>();
            setB.Add(a);
        }

        
        /// <summary>
        /// Устранение разрывов сети.<br/>
        /// Анализирует граф соединений алгоритмом BFS <see cref="ComputeReachableClusters"/>. Находит список кластеров, изолированных от стартового узла <see cref="RoomType.CommandCenter"/>.
        /// Пытается принудительно проложить маршрут от любого порта изолированного кластера к любому порту достижимой сети <see cref="TryConnectFirstAvailable"/>.
        /// </summary>
        /// <param name="tunnelPrefabs">Список префабов</param>
        /// <param name="clusterLinks">Словарь с данными кластеров</param>
        private void EnsureAllClustersConnected(List<RoomDataEntry> tunnelPrefabs,
            Dictionary<RoomCluster, HashSet<RoomCluster>> clusterLinks)
        {
            var commandCenterRoom = _allPlacedRooms.FirstOrDefault(r => r.RoomType == RoomType.CommandCenter);
            if (commandCenterRoom == null) return;

            var coreCluster = commandCenterRoom.Cluster;
            var allClusters = _allPlacedRooms.Select(r => r.Cluster).Distinct().ToList();

            var reachable = ComputeReachableClusters(coreCluster, clusterLinks);
            var isolatedClusters = allClusters.Where(c => c != coreCluster && !reachable.Contains(c)).ToList();

            foreach (var isolatedCluster in isolatedClusters)
            {
                if (reachable.Contains(isolatedCluster)) continue;

                var ownExits = GetFreeExits(c => c == isolatedCluster, true);
                if (ownExits.Count == 0)
                    continue;

                var reachableExits = GetFreeExits(c => reachable.Contains(c), true);
                if (reachableExits.Count == 0)
                    continue;

                if (TryConnectFirstAvailable(ownExits, reachableExits, tunnelPrefabs, clusterLinks))
                {
                    reachable = ComputeReachableClusters(coreCluster, clusterLinks);
                }
            }
        }
        
        /// <summary>
        /// Обходит граф связей кластеров методом BFS и возвращает множество кластеров, имеющих путь к центральному кластеру.
        /// </summary>
        /// <param name="coreCluster">Центральный кластер</param>
        /// <param name="clusterLinks">Слоаврь с данными кластерами</param>
        /// <returns>Хеш данные класетра</returns>
        private HashSet<RoomCluster> ComputeReachableClusters(
            RoomCluster coreCluster,
            Dictionary<RoomCluster, HashSet<RoomCluster>> clusterLinks)
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

        /// <summary>
        /// Вычисление координат стен и проемов.<br/>
        /// Конвертирует координаты клетки в глобальные Unity-координаты. Рассчитывает смещение к краю клетки в зависимости от вектора направления двери.
        /// Вычисляет поворот. Добавляет результат в <see cref="_plannedWalls"/>.
        /// </summary>
        /// <param name="roomData">Данные комнаты</param>
        /// <param name="conn">Точка соединения</param>
        /// <param name="prefab">Префаб</param>
        /// <param name="instanceName">Имя спавнящегося инстанса</param>
        private void PlanDoorwayInsert(
            PlacedRoomDataCluster roomData,
            ConnectionPoint conn,
            GameObject prefab,
            string instanceName)
        {
            if (!prefab || roomData?.Entry == null) return;

            var centerWorldPos = _levelGrid.UnityGrid.GetCellCenterWorld(conn.GlobalPosition);
            var baseWorldPos = _levelGrid.UnityGrid.CellToWorld(conn.GlobalPosition);

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

            _plannedWalls.Add(new VirtualWallData
            {
                Prefab = prefab,
                Position = wallPos,
                Rotation = wallRot,
                Name = instanceName,
                OwnerRoom =  roomData
            });
        }

        /// <summary>
        /// Изоляция уровня.<br/>
        /// Итерирует список <see cref="PlacedRoomDataCluster.FreeConnections"/> всех комнат в конце генерации. Вызывает <see cref="PlanDoorwayInsert"/>, передавая префаб глухой стены.
        /// </summary>
        private void BlockUnusedExits()
        {
            if (!_wallPrefab)
            {
                Debug.LogError("[GENERATOR] Wall prefab not assigned.");
                return;
            }

            foreach (var roomData in _allPlacedRooms)
            {
                if (roomData == null) continue;

                foreach (var conn in roomData.FreeConnections)
                    PlanDoorwayInsert(roomData, conn, _wallPrefab, "BlockedExitWall");
            }
        }

        
        /// <summary>
        /// Декорирование переходов.<br/>
        /// Итерирует список <see cref="_usedConnections"/>. Вызывает <see cref="PlanDoorwayInsert"/>, передавая префаб стены с проемом.
        /// </summary>
        private void PlaceUsedExitPassages()
        {
            if (!_wallWithPassagePrefab)
            {
                Debug.LogError("[GENERATOR] Префаб стены с проходом не назначен.");
                return;
            }

            foreach (var (roomData, conn) in _usedConnections)
            {
                if (roomData == null) continue;
                PlanDoorwayInsert(roomData, conn, _wallWithPassagePrefab, "UsedExitPassageWall");
            }
        }

        
        /// <summary>
        /// Проверяет целостность всех проходов.<br/>
        /// Если проход односторонний (нет парной двери напротив или тоннеля), 
        /// он переводится в глухую стену.
        /// </summary>
        private void ValidateAndSanitizeConnections()
        {
            var usedSet = new HashSet<(Vector3Int pos, Vector3Int dir)>();
            foreach (var (room, conn) in _usedConnections)
            {
                usedSet.Add((conn.GlobalPosition, conn.Direction));
            }
            
            var tunnelDoorsSet = new HashSet<(Vector3Int pos, Vector3Int dir)>();
            foreach (var room in _allPlacedRooms)
            {
                foreach (var tunnel in room.AttachedTunnels)
                {
                    var plates = RoomRotationHelper.GetRotatedPlates(tunnel.Entry, tunnel.Rotation);
                    foreach (var plate in plates)
                    {
                        var globalPos = tunnel.Origin + plate.LocalPosition;
                        foreach (var door in plate.Doors)
                        {
                            tunnelDoorsSet.Add((globalPos, door.GlobalDirection));
                        }
                    }
                }
            }
            
            var invalidConnections = new List<(PlacedRoomDataCluster Room, ConnectionPoint Conn)>();

            foreach (var item in _usedConnections)
            {
                var targetPos = item.Conn.GlobalPosition + item.Conn.Direction;
                var targetDir = -item.Conn.Direction;
                
                var hasMatchingRoomDoor = usedSet.Contains((targetPos, targetDir));
                var hasMatchingTunnelDoor = tunnelDoorsSet.Contains((targetPos, targetDir));
                
                if (!hasMatchingRoomDoor && !hasMatchingTunnelDoor) invalidConnections.Add(item);
            }
            
            foreach (var invalid in invalidConnections)
            {
                _usedConnections.Remove(invalid);
                if (!invalid.Room.FreeConnections.Contains(invalid.Conn))
                {
                    invalid.Room.FreeConnections.Add(invalid.Conn);
                }
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
    }
}