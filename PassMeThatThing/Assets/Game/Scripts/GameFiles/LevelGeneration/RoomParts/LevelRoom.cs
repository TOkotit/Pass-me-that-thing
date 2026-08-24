using System.Collections.Generic;
using Game.Scripts.Enums;
using Mirror;
using Unity.AI.Navigation;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    
    [RequireComponent(typeof(Grid))]

    public class LevelRoom : MonoBehaviour
    {
        [SerializeField] private RoomTypeNew roomType;
        [SerializeField] private RoomPlateData[] plates;
        [SerializeField] private int totalDoors;
        [SerializeField] private List<NetworkObjectSpot> networkObjects;
        [SerializeField] private NavMeshSurface navMeshSurface;

        public RoomTypeNew RoomType => roomType;
        public int TotalDoors => totalDoors;
        public RoomPlateData[] Plates => plates;
        public List<NetworkObjectSpot> NetworkObjects => networkObjects;
        public NavMeshSurface NavMeshSurface => navMeshSurface;
        
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

    }
}