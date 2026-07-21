using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class LevelRoomNew : MonoBehaviour
    {
        [SerializeField] private RoomTypeNew roomType;
        [SerializeField] private RoomPlateData[] plates;
        [SerializeField] private int totalDoors;
        
        public RoomTypeNew RoomType => roomType;
        public int TotalDoors => totalDoors;
        public RoomPlateData[] Plates => plates;
        
        public void CompileRoom()
        {
            var grid = GetComponent<Grid>();
            var childPlates = GetComponentsInChildren<RoomPlate>();
            plates = new RoomPlateData[childPlates.Length];
            
            totalDoors = 0; 

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
                if (currentPlate.ConnectionEast == RoomsConnectionTypes.Door) totalDoors++;
                if (currentPlate.ConnectionSouth == RoomsConnectionTypes.Door) totalDoors++;
                if (currentPlate.ConnectionWest == RoomsConnectionTypes.Door) totalDoors++;
            }
        }
    }
}