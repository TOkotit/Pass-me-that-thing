using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration
{
    public class RoomPlate : MonoBehaviour
    {
        public Color roomColor =  new Color(0.2f, 0.8f, 0.2f, 0.4f);
        public Color doorColor =  Color.red;
        public Color gateColor = Color.blue;
        
        [SerializeField] private Grid parentGrid = null;
        [SerializeField] private LevelRoom parentRoom = null;
        public RoomsConnectionTypes ConnectionNorth;
        public RoomsConnectionTypes ConnectionEast;  
        public RoomsConnectionTypes ConnectionSouth; 
        public RoomsConnectionTypes ConnectionWest;
        
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
            
            DrawConnection(Vector3.forward * lineLength, ConnectionNorth, cellSize);
            DrawConnection(Vector3.right * lineLength, ConnectionEast, cellSize);
            DrawConnection(Vector3.back * lineLength, ConnectionSouth, cellSize);
            DrawConnection(Vector3.left * lineLength, ConnectionWest, cellSize);
            
            Gizmos.matrix = oldMatrix;
        }
        
        private void DrawConnection(Vector3 direction, RoomsConnectionTypes type, float cellSize)
        {
            if (type == RoomsConnectionTypes.None) return;

            var start = Vector3.zero;
            var end = start + direction;

            if (type == RoomsConnectionTypes.Door)
            {
                Gizmos.color = doorColor;
                Gizmos.DrawLine(start, end);
                Gizmos.DrawSphere(end, 0.3f);
            }
            else if (type == RoomsConnectionTypes.Gate)
            {
                Gizmos.color = gateColor;
                Gizmos.DrawLine(start, end);
                
                Gizmos.DrawCube(end, new Vector3(
                    direction.x == 0 ? cellSize * 0.8f : 0.2f, 
                    0.2f, 
                    direction.z == 0 ? cellSize * 0.8f : 0.2f
                ));
            }
        }
        
        private Color GetPlateColor()
        {
            if (parentRoom == null) 
                return new Color(0.5f, 0.5f, 0.5f, 0.4f);

            switch (parentRoom.RoomType)
            {
                case RoomType.Hub:
                    return new Color(0.2f, 0.8f, 0.2f, 0.4f); 
                case RoomType.Defense:
                    return new Color(0.9f, 0.1f, 0.1f, 0.4f); 
                case RoomType.Exit:
                    return new Color(1f, 0.4f, 0.7f, 0.4f);   
                case RoomType.Regular:
                    return new Color(0.5f, 0.3f, 0.15f, 0.4f);
                case RoomType.Event:
                    return GetEventColor(parentRoom.EventType);
                default:
                    return new Color(0.5f, 0.5f, 0.5f, 0.4f);
            }
        }
        
        private Color GetEventColor(GameEventsType eventType)
        {
            return eventType switch
            {
                GameEventsType.BlackoutBlowFuse => new Color(0.05f, 0.05f, 0.05f, 0.5f), 
                GameEventsType.BlackoutCutWires => new Color(0.9f, 0.9f, 0.1f, 0.4f), 
                GameEventsType.FloodBrokenPump  => new Color(0.95f, 0.95f, 0.95f, 0.4f),
                GameEventsType.FloodPipeBreak   => new Color(0.1f, 0.4f, 0.9f, 0.4f),   
                _                               => new Color(0.3f, 0.3f, 0.3f, 0.4f)    
            };
        }
    }
}