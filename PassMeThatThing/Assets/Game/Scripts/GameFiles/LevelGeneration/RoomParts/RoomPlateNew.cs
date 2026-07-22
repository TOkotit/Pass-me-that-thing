using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class RoomPlateNew : MonoBehaviour 
    {
        public Color doorColor = Color.red;
        
        [SerializeField] private Grid parentGrid = null;
        [SerializeField] private LevelRoomNew parentRoom = null;
        public RoomsConnectionTypes ConnectionNorth;
        public RoomsConnectionTypes ConnectionEast;  
        public RoomsConnectionTypes ConnectionSouth; 
        public RoomsConnectionTypes ConnectionWest;   
        
        private void OnDrawGizmosSelected()
        {
            if (parentGrid == null)
                parentGrid = GetComponentInParent<Grid>();
            if (parentRoom == null)
                parentRoom = GetComponentInParent<LevelRoomNew>();
                
            var cellSize = parentGrid != null ? parentGrid.cellSize.x : 1f;
            var oldMatrix = Gizmos.matrix;
            
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = GetPlateColor();
            Gizmos.DrawCube(Vector3.zero, new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));

            var lineLength = cellSize * 0.5f;
            
            DrawConnection(Vector3.forward * lineLength, ConnectionNorth);
            DrawConnection(Vector3.right * lineLength, ConnectionEast);
            DrawConnection(Vector3.back * lineLength, ConnectionSouth);
            DrawConnection(Vector3.left * lineLength, ConnectionWest);
            
            Gizmos.matrix = oldMatrix;
        }
        
        private void DrawConnection(Vector3 direction, RoomsConnectionTypes type)
        {
            if (type != RoomsConnectionTypes.Door) return;

            var start = Vector3.zero;
            var end = start + direction;

            Gizmos.color = doorColor;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(end, 0.3f);
        }
        
        private Color GetPlateColor()
        {
            if (parentRoom == null) 
                return new Color(0.5f, 0.5f, 0.5f, 0.4f);

            return parentRoom.RoomType switch
            {
                RoomTypeNew.CommandCenter     => new Color(0.2f, 0.8f, 0.2f, 0.4f),
                RoomTypeNew.Generator         => new Color(0.9f, 0.9f, 0.1f, 0.4f),
                RoomTypeNew.Warehouse         => new Color(0.6f, 0.4f, 0.2f, 0.4f),
                RoomTypeNew.LivingBlock       => new Color(0.2f, 0.6f, 0.8f, 0.4f),
                RoomTypeNew.MedicalBlock      => new Color(0.9f, 0.2f, 0.2f, 0.4f),
                RoomTypeNew.RecoveryHangar    => new Color(1f, 0.4f, 0.7f, 0.4f),
                RoomTypeNew.TechnicalTunnels  => new Color(0.4f, 0.4f, 0.4f, 0.4f),
                RoomTypeNew.Laboratory        => new Color(0.6f, 0.2f, 0.8f, 0.4f),
                RoomTypeNew.Workshop          => new Color(0.8f, 0.5f, 0.2f, 0.4f),
                RoomTypeNew.Server            => new Color(0.1f, 0.8f, 0.9f, 0.4f),
                RoomTypeNew.WaterPurification => new Color(0.2f, 0.4f, 0.9f, 0.4f),
                RoomTypeNew.Armory            => new Color(0.3f, 0.3f, 0.3f, 0.4f),
                RoomTypeNew.None              => new Color(0.5f, 0.5f, 0.5f, 0.4f),
                _                             => new Color(0.5f, 0.5f, 0.5f, 0.4f)
            };
        }
    }
}