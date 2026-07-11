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
        [SerializeField] private int totalConnections;
        
        
        public RoomType RoomType => roomType;
        public GameEventsType EventType => eventType;
        public int TotalConnections => totalConnections;
        public RoomPlateData[] Plates => plates;
        
        
        [ContextMenu("Compile Room")]
        public void CompileRoom()
        {
            var grid = GetComponent<Grid>();
            var childPlates = GetComponentsInChildren<RoomPlate>();
            plates = new RoomPlateData[childPlates.Length];
            
            totalConnections = 0; 

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

                if (currentPlate.ConnectionNorth != RoomsConnectionTypes.None) totalConnections++;
                if (currentPlate.ConnectionEast != RoomsConnectionTypes.None) totalConnections++;
                if (currentPlate.ConnectionSouth != RoomsConnectionTypes.None) totalConnections++;
                if (currentPlate.ConnectionWest != RoomsConnectionTypes.None) totalConnections++;
            }
        }
    }
}