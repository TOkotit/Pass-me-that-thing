using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    
    [RequireComponent(typeof(Grid))]
    public class LevelRoomNew : MonoBehaviour
    {
        [SerializeField] private RoomTypeNew roomType;
        [SerializeField] private RoomPlateDataNew[] plates;
        [SerializeField] private int totalDoors;

        public int DepthFromHub { get; set; }
        public RoomTypeNew RoomType => roomType;
        public int TotalDoors => totalDoors;
        public RoomPlateDataNew[] Plates => plates;

        [ContextMenu("Compile Room")]
        public void CompileRoom()
        {
            var grid = GetComponent<Grid>();
            var childPlates = GetComponentsInChildren<RoomPlateNew>();
            plates = new RoomPlateDataNew[childPlates.Length];

            totalDoors = 0;

            for (var i = 0; i < childPlates.Length; i++)
            {
                var currentPlate = childPlates[i];
                var localPos = transform.InverseTransformPoint(currentPlate.transform.position);
                var gridPos = grid.LocalToCell(localPos);

                plates[i] = new RoomPlateDataNew
                {
                    localPosition = gridPos,
                    plate = currentPlate
                };

                if (currentPlate.HasDoorNorth) totalDoors++;
                if (currentPlate.HasDoorEast) totalDoors++;
                if (currentPlate.HasDoorSouth) totalDoors++;
                if (currentPlate.HasDoorWest) totalDoors++;
            }
        }
    }
}