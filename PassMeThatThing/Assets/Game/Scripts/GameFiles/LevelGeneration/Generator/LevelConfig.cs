using System.Collections.Generic;
using Game.Scripts.Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.GameFiles.LevelGeneration.Graph
{
    /// <summary>
    /// ScriptableObject конфигурации генератора уровней.
    /// Позволяет настраивать параметры генерации прямо из инспектора Unity.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Level Generation/Level Generator Config")]
    public class LevelConfig : ScriptableObject
    {
        [FormerlySerializedAs("_useRandomSeed")] [Header("Seed Settings")] [SerializeField]
        private bool useRandomSeed = true;

        [SerializeField] private int customSeed;

        [Header("Room Count Limits")] [SerializeField]
        private int difficulty = 1;

        [SerializeField] private int minRooms = 10;
        [SerializeField] private int maxRooms = 30;
        [SerializeField] private int absoluteMinRooms = 7;
        [SerializeField] private int maxConnectionsPerRoom = 4;

        [Header("Cluster Settings")] [SerializeField]
        private int minClusterSize = 2;

        [SerializeField] private int maxClusterSize = 4;

        [Header("Core Cluster Rooms")]
        [Tooltip("Комнаты, которые гарантированно создаются в базовом кластере.")]
        [SerializeField]
        private List<RoomType> coreRooms = new()
        {
            RoomType.CommandCenter,
            RoomType.Generator,
            RoomType.Warehouse,
            RoomType.LivingBlock
        };

        [Header("Mandatory Pools")] [Tooltip("Комнаты, обязательные для распределения по кластерам.")] [SerializeField]
        private List<RoomType> mandatoryRooms = new()
        {
            RoomType.Laboratory,
            RoomType.Armory,
            RoomType.Workshop,
            RoomType.MedicalBlock
        };

        [Header("Event Rooms")] [SerializeField]
        private List<RoomType> eventRooms = new()
        {
            RoomType.Server,
            RoomType.WaterPurification
        };

        [SerializeField] private int highEventThreshold = 20;
        [SerializeField] private int lowEventThreshold = 15;

        [Header("Repeatable Rooms Pool")] [Tooltip("Базовый пул повторно используемых комнат.")] [SerializeField]
        private List<RoomType> _baseRepeatableRooms = new()
        {
            RoomType.LivingBlock,
            RoomType.LivingBlock,
            RoomType.Warehouse,
            RoomType.Warehouse
        };

        [SerializeField] private int _maxMedicalBlocksOnLevel = 2;

        public bool UseRandomSeed => useRandomSeed;
        public int CustomSeed => customSeed;
        public int Difficulty => difficulty;
        public int MinRooms => minRooms;
        public int MaxRooms => maxRooms;
        public int AbsoluteMinRooms => absoluteMinRooms;
        public int MaxConnectionsPerRoom => maxConnectionsPerRoom;
        public int MinClusterSize => minClusterSize;
        public int MaxClusterSize => maxClusterSize;
        public IReadOnlyList<RoomType> CoreRooms => coreRooms;
        public IReadOnlyList<RoomType> MandatoryRooms => mandatoryRooms;
        public IReadOnlyList<RoomType> EventRooms => eventRooms;
        public int HighEventThreshold => highEventThreshold;
        public int LowEventThreshold => lowEventThreshold;
        public IReadOnlyList<RoomType> BaseRepeatableRooms => _baseRepeatableRooms;
        public int MaxMedicalBlocksOnLevel => _maxMedicalBlocksOnLevel;
    }
}