using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class RoomPlate : MonoBehaviour 
    {
        public Color doorColor = Color.red;
        
        [SerializeField] private Grid parentGrid = null;
        [SerializeField] private LevelRoom parentRoom = null;
        public bool HasDoorNorth;
        public bool HasDoorEast;  
        public bool HasDoorSouth; 
        public bool HasDoorWest; 
        
        private void OnDrawGizmosSelected()
        {
            if (parentGrid == null)
                parentGrid = GetComponentInParent<Grid>();
            if (parentRoom == null)
                parentRoom = GetComponentInParent<LevelRoom>();
                
            var cellSize = parentGrid != null ? parentGrid.cellSize.x : 1f;
            var oldMatrix = Gizmos.matrix;
            
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = GetPlateColor();
            Gizmos.DrawCube(Vector3.zero, new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));

            var lineLength = cellSize * 0.5f;
            
            DrawConnection(Vector3.forward * lineLength, HasDoorNorth);
            DrawConnection(Vector3.right * lineLength, HasDoorEast);
            DrawConnection(Vector3.back * lineLength, HasDoorSouth);
            DrawConnection(Vector3.left * lineLength, HasDoorWest);
            
            Gizmos.matrix = oldMatrix;
        }
        
        private void DrawConnection(Vector3 direction, bool hasDoor)
        {
            if (!hasDoor) return;

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
                RoomType.CommandCenter     => new Color(0f, 1f, 0f, 0.4f),
                RoomType.Generator         => new Color(0.9f, 0.9f, 0.1f, 0.4f),
                RoomType.Warehouse         => new Color(0.6f, 0.4f, 0.2f, 0.4f),
                RoomType.LivingBlock       => new Color(0.12f, 0.69f, 0.26f, 0.4f),
                RoomType.MedicalBlock      => new Color(1f, 0f, 0f, 0.4f),
                RoomType.RecoveryHangar    => new Color(1f, 0.4f, 0.7f, 0.4f),
                RoomType.TechnicalTunnels  => new Color(0.4f, 0.4f, 0.4f, 0.4f),
                RoomType.Laboratory        => new Color(1f, 1f, 1f, 0.4f),
                RoomType.Workshop          => new Color(0.54f, 1f, 1f, 0.4f),
                RoomType.Server            => new Color(0f, 0f, 0f, 0.4f),
                RoomType.WaterPurification => new Color(0.1f, 0.3f, 1f, 0.4f),
                RoomType.Armory            => new Color(0.9f, 0.2f, 0.3f, 0.4f),
                RoomType.None              => new Color(0.5f, 0.5f, 0.5f, 0.4f),
                _                             => new Color(0.5f, 0.5f, 0.5f, 0.4f)
            };
        }
    }
}