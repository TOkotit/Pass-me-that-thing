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
    
    /// <summary>
    /// Класс для хранения данных о кластере комнат<br/>
    /// Содержит:
    /// <list type="bullet">
    /// <item><see cref="RoomDataEntry"/></item>
    /// <item><see cref="RoomType"/></item>
    /// <item>Ссылку на <see cref="LevelRoom"/></item>
    /// <item>Ссылку на префаб</item>
    /// <item>Origin</item>
    /// <item>Rotation</item>
    /// <item><see cref="RoomCluster"/></item>
    /// <item>Список свободных соединений List&lt;<see cref="ConnectionPoint"/>&gt; <see cref="FreeConnections"/></item>
    /// <item>Список занятых клеток List&lt;Vector3Int&gt; <see cref="OccupiedCells"/></item>
    /// <item>Список тоннелей List&lt;<see cref="VirtualTunnelData"/>&gt; <see cref="AttachedTunnels"/></item>
    /// </list> 
    /// </summary>
    public class PlacedRoomDataCluster
    {
        public RoomDataEntry Entry;
        public RoomType RoomType;
        public LevelRoom RoomComponent;
        public GameObject PrefabInstance;
        public Vector3Int Origin;
        public RoomRotation Rotation;
        public RoomCluster Cluster;
        
        public int RoomId;
        public string GlobalId => Cluster != null ? $"{Cluster.Id}_{RoomId}" : RoomId.ToString();
        
        public List<ConnectionPoint> FreeConnections = new();
        public List<Vector3Int> OccupiedCells = new();
        public List<VirtualTunnelData> AttachedTunnels = new();
    }
    
    /// <summary>
    /// Список точек соединения комнат<br/>
    /// Хранит глобальную позицую и направление
    /// </summary>
    public struct ConnectionPoint
    {
        public Vector3Int GlobalPosition;
        public Vector3Int Direction;
    }
    
    /// <summary>
    /// Главный класс запускающий виртуальную генерацию уровня, и физически расставляющего по заданным данным префабы комнат.<br/>
    /// Выполняет локальный детерминированный спавн объектов по сиду на всех клиентах.<br/>
    /// Отвечает за жизненный цикл генерации, использует <see cref="LevelGeneratorSolver"/> для расчётов.
    /// </summary>
    public class LevelOrchestrator : MonoBehaviour
    {
        [SerializeField] private RoomDatabase roomDatabase;
        [SerializeField] public LevelGrid levelGrid;
        [SerializeField] public NetworkObjectsOrchestrator networkObjectsOrchestrator;
        [SerializeField] public NetworkRarityItemsOrchestrator networkRarityItemsOrchestrator;
        [SerializeField] private Transform levelContainer;
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject wallWithPassagePrefab;

        public static Transform ActiveLevelContainer { get; private set; }

        private LevelGeneratorSolver _solver;
        private List<GameObject> _placedWalls = new();

        public List<NetworkObjectSpot> AllLevelSpots { get; private set; } = new();
        public Dictionary<LevelRoom, List<NetworkRarityItemSpot>> AllLevelRarityItemSpots { get; private set; } = new();

        public IReadOnlyList<PlacedRoomDataCluster> AllPlacedRooms => _solver?.AllPlacedRooms;
        public IReadOnlyList<RoomCluster> Clusters => _solver?.Clusters;

        [SerializeField] private LevelConfig levelConfig;
        
        /// <summary>
        /// Получает пустышку на сцене как контейнер и создаёт экземпляр <see cref="LevelGeneratorSolver"/>
        /// </summary>
        private void Awake()
        {
            ActiveLevelContainer = levelContainer;
            _solver = new LevelGeneratorSolver(roomDatabase, levelGrid, wallPrefab, wallWithPassagePrefab);
        }

        [Inject]
        private void Construct(
            ItemRarityDatabase rarityDatabase,
            ItemPoolManager itemPoolManager,
            PhysicalItemRegistry physicalItemRegistry)
        {
            networkRarityItemsOrchestrator.Init(rarityDatabase, itemPoolManager, physicalItemRegistry);
        }
        
        /// <summary>
        /// Запуск генерации по конфигу.
        /// </summary>
        /// <param name="who"></param>
        /// <param name="forcedSeed"></param>
        public int GenerateLevelFromConfig(string who, int? forcedSeed = null)
        {
            if (!levelConfig)
            {
                Debug.LogError($"[GENERATOR] ({who}) LevelConfig не назначен в LevelOrchestrator!");
                return 0;
            }

            var activeSeed = forcedSeed ?? (levelConfig.UseRandomSeed 
                ? UnityEngine.Random.Range(int.MinValue, int.MaxValue) 
                : levelConfig.CustomSeed);

            var generator = new LevelGenerator(levelConfig, activeSeed);
            var clusters = generator.GenerateClusters();

            if (clusters != null && clusters.Count > 0)
            {
                GeneratePhysicalLevel(clusters, activeSeed);
                Debug.Log($"[GENERATOR] ({who}) Уровень сгенерирован. Seed: {activeSeed}, Кластеров: {clusters.Count}.");
            }
            else
            {
                Debug.LogError($"[GENERATOR] ({who}) Ошибка: Не удалось сгенерировать кластеры.");
            }

            return activeSeed;
        }

        /// <summary>
        /// Очищает контейнер на сцене и вызывает генерацию уровня.<br/>
        /// Спавнит нетворк объекты и префабы по окончании генерации.<br/>
        /// </summary>
        /// <param name="clusters">Список с кластерами</param>
        /// <param name="seed">Сид</param>
        public void GeneratePhysicalLevel(List<RoomCluster> clusters, int seed)
        {
            ClearLevel();

            if (clusters == null || clusters.Count == 0) return;

            if (_solver == null)
            {
                _solver = new LevelGeneratorSolver(roomDatabase, levelGrid, wallPrefab, wallWithPassagePrefab);
            }

            var success = _solver.GenerateVirtualLevel(clusters, seed);
            if (!success) return;

            InstantiateVirtualLevel();

            networkObjectsOrchestrator.SpawnNetworkObjects(AllLevelSpots);
            BakeNavMeshes();
            networkRarityItemsOrchestrator.SpawnNetworkRarityItem(AllLevelRarityItemSpots);

            Debug.Log($"[GENERATOR] Total clusters: {clusters.Count}. Total placed rooms: {_solver.AllPlacedRooms.Count}.");
        }

        /// <summary>
        /// Физически проходится по списку объектов и спавнит их
        /// </summary>
        private void InstantiateVirtualLevel()
        {
            foreach (var roomData in _solver.AllPlacedRooms)
            {
                var centerWorldPos = levelGrid.UnityGrid.GetCellCenterWorld(roomData.Origin);
                var baseWorldPos = levelGrid.UnityGrid.CellToWorld(roomData.Origin);
                var worldPos = new Vector3(centerWorldPos.x, baseWorldPos.y, centerWorldPos.z);
                var rotQuat = GetRotationQuaternion(roomData.Rotation);

                var instanceGo = Instantiate(roomData.Entry.PrefabGameObject, worldPos, rotQuat, levelContainer);
                var spawnedRoomComponent = instanceGo.GetComponent<LevelRoom>();

                roomData.PrefabInstance = instanceGo;
                roomData.RoomComponent = spawnedRoomComponent;

                AllLevelSpots.AddRange(spawnedRoomComponent.NetworkObjects);
                AllLevelRarityItemSpots[spawnedRoomComponent] = spawnedRoomComponent.NetworkRarityItems;

                foreach (var tunnel in roomData.AttachedTunnels)
                {
                    var tCenter = levelGrid.UnityGrid.GetCellCenterWorld(tunnel.Origin);
                    var tBase = levelGrid.UnityGrid.CellToWorld(tunnel.Origin);
                    var tPos = new Vector3(tCenter.x, tBase.y, tCenter.z);
                    var tRot = GetRotationQuaternion(tunnel.Rotation);

                    Instantiate(tunnel.Entry.PrefabGameObject, tPos, tRot, levelContainer);
                }
            }

            foreach (var wallData in _solver.PlannedWalls)
            {
                var instance = Instantiate(wallData.Prefab, wallData.Position, wallData.Rotation, levelContainer);
                instance.name = wallData.Name;
                _placedWalls.Add(instance);
            }
        }

        /// <summary>
        /// Возвращает значение вращения по виртуальным данным 
        /// </summary>
        /// <param name="rotation">Поворот</param>
        /// <returns><see cref="Quaternion"/> значение вращения</returns>
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
        
        /// <summary>
        /// Запекание NavMesh префабах
        /// </summary>
        private void BakeNavMeshes()
        {
            foreach (var roomData in _solver.AllPlacedRooms)
            {
                var room = roomData.RoomComponent;
                if (!room) continue;

                var surface = room.NavMeshSurface;
                if (!surface)
                {
                    Debug.LogWarning($"[GENERATOR] У комнаты {room.name} не задан NavMeshSurface, навмеш не запечён.");
                    continue;
                }

                surface.BuildNavMesh();
            }
        }

        /// <summary>
        /// Очищает всё от старых префабов и списки
        /// </summary>
        private void ClearLevel()
        {
            levelGrid.ClearGrid();
            _solver?.Reset();
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
    }
}