using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    [RequireComponent(typeof(Grid))]
    public class LevelRoom : MonoBehaviour
    {
        [SerializeField] private RoomType roomType;
        [SerializeField] private GameEventsType eventType;
        [SerializeField] private RoomPlateData[] plates;
        [SerializeField] private int totalDoors;
        [SerializeField] private int totalGates;
        
        public RoomType RoomType => roomType;
        public GameEventsType EventType => eventType;
        public int TotalDoors => totalDoors;
        public int TotalGates => totalGates;
        public RoomPlateData[] Plates => plates;
        public int DepthFromHub { get; set; }
        
        [ContextMenu("Compile Room")]
        public void CompileRoom()
        {
            var grid = GetComponent<Grid>();
            var childPlates = GetComponentsInChildren<RoomPlate>();
            plates = new RoomPlateData[childPlates.Length];
            
            totalDoors = 0; 
            totalGates = 0;

            for (var i = 0; i < childPlates.Length; i++)
            {
                var currentPlate = childPlates[i];
                var localPos = transform.InverseTransformPoint(currentPlate.transform.position);
                var gridPos = grid.LocalToCell(localPos);

                plates[i] = new RoomPlateData
                {
                    localPosition = gridPos,
                    plate = currentPlate
                };

                if (currentPlate.ConnectionNorth == RoomsConnectionTypes.Door) totalDoors++;
                else if (currentPlate.ConnectionNorth == RoomsConnectionTypes.Gate) totalGates++;

                if (currentPlate.ConnectionEast == RoomsConnectionTypes.Door) totalDoors++;
                else if (currentPlate.ConnectionEast == RoomsConnectionTypes.Gate) totalGates++;

                if (currentPlate.ConnectionSouth == RoomsConnectionTypes.Door) totalDoors++;
                else if (currentPlate.ConnectionSouth == RoomsConnectionTypes.Gate) totalGates++;

                if (currentPlate.ConnectionWest == RoomsConnectionTypes.Door) totalDoors++;
                else if (currentPlate.ConnectionWest == RoomsConnectionTypes.Gate) totalGates++;
            }
        }
    }
}