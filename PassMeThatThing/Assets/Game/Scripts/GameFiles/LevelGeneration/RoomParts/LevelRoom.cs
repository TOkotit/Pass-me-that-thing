using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Game.Scripts.GameFiles.LevelGeneration.ItemSpawn;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.LevelGeneration.ItemSpawn;
using Game.Scripts.Utils;
using Mirror;
using Unity.AI.Navigation;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    
    [RequireComponent(typeof(Grid))]

    public class LevelRoom : MonoBehaviour
    {
        [SerializeField] private RoomType roomType;
        [SerializeField] private RoomPlateData[] plates;
        [SerializeField] private int totalDoors;
        [SerializeField] private List<NetworkObjectSpot> networkObjects;
        [SerializeField] private List<NetworkRarityItemSpot> networkRarityItems;
        [SerializeField] private NavMeshSurface navMeshSurface;

        [Header("Item Spawn")]
        [SerializeField] private int minItemCount = 0;
        [SerializeField] private float spawnRate = 1;
        [SerializeField] private float spawnRateStep = 1.2f;

        private int _maxItemCount;//не учитываются споты с UseConstSpawnChance
        private float[] _weights;
        private float _totalWeight;

        public RoomType RoomType => roomType;
        public int TotalDoors => totalDoors;
        public RoomPlateData[] Plates => plates;
        public List<NetworkObjectSpot> NetworkObjects => networkObjects;
        public NavMeshSurface NavMeshSurface => navMeshSurface;
        public List<NetworkRarityItemSpot> NetworkRarityItems  => networkRarityItems;

        public void Awake()
        {
            ReparentToLevelContainer();
        }

        private void ReparentToLevelContainer()
        {
            var container = LevelOrchestrator.ActiveLevelContainer;
            if (container != null && transform.parent != container)
            {
                transform.SetParent(container, true);
            }
        }

        
        [ContextMenu("Compile Room")]
        public void CompileRoom()
        {
            var grid = GetComponent<Grid>();
            
            var childNetworkObjects = GetComponentsInChildren<NetworkObjectSpot>();
            networkObjects = new List<NetworkObjectSpot>(childNetworkObjects);

            var childNetworkRarityItems = GetComponentsInChildren<NetworkRarityItemSpot>();
            networkRarityItems = new List<NetworkRarityItemSpot>(childNetworkRarityItems);

            navMeshSurface = GetComponentInChildren<NavMeshSurface>(true);
            if (navMeshSurface == null)
            {
                Debug.LogWarning($"[LEVEL ROOM] {name}: NavMeshSurface не найден среди дочерних объектов.");
            }

            
            var childPlates = GetComponentsInChildren<RoomPlate>();
            plates = new RoomPlateData[childPlates.Length];

            totalDoors = 0;

            for (var i = 0; i < childPlates.Length; i++)
            {
                var currentPlate = childPlates[i];
                var localPos = transform.InverseTransformPoint(currentPlate.transform.position);
                var gridPos = grid.LocalToCell(localPos);
                
                var relativeRotation = Quaternion.Inverse(transform.rotation) * currentPlate.transform.rotation;
                var plateRotation = SnapToRoomRotation(relativeRotation.eulerAngles.y);


                plates[i] = new RoomPlateData
                {
                    localPosition = gridPos,
                    plate = currentPlate,
                    localRotation = plateRotation
                };

                if (currentPlate.HasDoorNorth) totalDoors++;
                if (currentPlate.HasDoorEast) totalDoors++;
                if (currentPlate.HasDoorSouth) totalDoors++;
                if (currentPlate.HasDoorWest) totalDoors++;

            }
        }
        
        private static RoomRotation SnapToRoomRotation(float yEulerDegrees)
        {
            var normalized = ((yEulerDegrees % 360f) + 360f) % 360f;
            var steps = Mathf.RoundToInt(normalized / 90f) % 4;
            return (RoomRotation)steps;
        }

        //item spawn
        //TODO Сделать просчет весов спавна одноразовым
        //и где-то хранить это для каждой комнаты
        public void CacheItemCountWeights()
        {
            _maxItemCount = networkRarityItems.Where(s => !s.UseConstSpawnChance).Count();

            _weights = new float[_maxItemCount + 1];
            _weights[0] = spawnRate;

            for (var i = 1; i <= _maxItemCount; i++)
            {
                if (i < _maxItemCount / 2)
                    _weights[i] = MathF.Round(_weights[i - 1] * spawnRateStep, 3);
                else
                    _weights[i] = MathF.Round(_weights[i - 1] / spawnRateStep, 3);
            }

            _totalWeight = 0f;
            foreach (var w in _weights)
            {
                _totalWeight += w;
            }

            Debug.Log("Room CacheItemCountWeights" + string.Join(" ", _weights));
        }

        public int GetRandomItemCount()
        {
            if (_maxItemCount == 0) return 0;

            return RandomUtilities.RandomWeightedIndex(_weights, _totalWeight);
        }

        
    }
}