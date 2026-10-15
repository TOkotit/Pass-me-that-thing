using System;
using System.Collections.Generic;
using Game.Scripts.Enums;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.GameFiles.LevelGeneration.Graph
{
    
    public enum RoomCountOperator
    {
        LessOrEqual,
        Equal,      
        GreaterOrEqual 
    }

    [Serializable]
    public struct ClusterRoomSetting
    {
        [Tooltip("Тип комнаты, разрешенный в кластерах")]
        public RoomType roomType;
        [Tooltip("Может ли эта комната встречаться в множественном числе внутри одного кластера")]
        public bool allowMultiplePerCluster;
    }

    [Serializable]
    public struct LevelRoomConstraint
    {
        [Tooltip("Тип комнаты")]
        public RoomType roomType;
        [Tooltip("Условие ограничения количества на весь уровень")]
        public RoomCountOperator countOperator;
        [Tooltip("Целевое количество комнат")]
        public int targetCount;
    }
    
    
    /// <summary>
    /// ScriptableObject конфигурации генератора уровней.
    /// Позволяет настраивать параметры генерации прямо из инспектора Unity.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Level Generation/Level Generator Config")]
    public class LevelConfig : ScriptableObject
    {
      [Header("Seed Settings")]
        [SerializeField] private bool useRandomSeed = true;
        [SerializeField] private int customSeed;

        [Header("Room Count Limits")]
        [SerializeField] private int difficulty = 1;
        [SerializeField] private int minRooms = 10;
        [SerializeField] private int maxRooms = 30;
        [SerializeField] private int absoluteMinRooms = 7;
        [SerializeField] private int maxConnectionsPerRoom = 4;

        [Header("Cluster Size Bounds")]
        [SerializeField] private int minClusterSize = 2;
        [SerializeField] private int maxClusterSize = 4;

        [Header("Core Rooms (Base Hub)")]
        [Tooltip("Комнаты, гарантированно создаваемые в базовом ядре уровня.")]
        [SerializeField] private List<RoomType> coreRooms = new();

        [Header("Mandatory Rooms (At least 1 on level)")]
        [Tooltip("Список комнат, каждая из которых обязана появиться на уровне хотя бы в единичном экземпляре.")]
        [SerializeField] private List<RoomType> mandatoryRooms = new();

        [Header("Cluster Room Pool")]
        [Tooltip("Все типы комнат, которые могут распределяться по обычным кластерам.")]
        [SerializeField] private List<ClusterRoomSetting> clusterRoomSettings = new();

        [Header("Level-Wide Room Constraints")]
        [Tooltip("Правила ограничения количества конкретных комнат на весь уровень.")]
        [SerializeField] private List<LevelRoomConstraint> levelConstraints = new();

        // Публичные свойства для доступа из генератора
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
        public IReadOnlyList<ClusterRoomSetting> ClusterRoomSettings => clusterRoomSettings;
        public IReadOnlyList<LevelRoomConstraint> LevelConstraints => levelConstraints;
    }
}