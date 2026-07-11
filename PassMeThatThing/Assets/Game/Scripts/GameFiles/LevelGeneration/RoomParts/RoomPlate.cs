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
        public RoomsConnectionTypes ConnectionNorth;
        public RoomsConnectionTypes ConnectionEast;  
        public RoomsConnectionTypes ConnectionSouth; 
        public RoomsConnectionTypes ConnectionWest;
        
        private void OnDrawGizmosSelected()
        {
            if (parentGrid == null)
                parentGrid = GetComponentInParent<Grid>();
            var cellSize = parentGrid != null ? parentGrid.cellSize.x : 1f;

            Gizmos.color = roomColor;
            Gizmos.DrawCube(transform.position, new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));

            Gizmos.color = doorColor;
            var lineLength = cellSize * 0.5f; 
            
            DrawConnection(Vector3.forward * lineLength, ConnectionNorth, cellSize);
            DrawConnection(Vector3.right * lineLength, ConnectionEast, cellSize);
            DrawConnection(Vector3.back * lineLength, ConnectionSouth, cellSize);
            DrawConnection(Vector3.left * lineLength, ConnectionWest, cellSize);
        }
        
        private void DrawConnection(Vector3 direction, RoomsConnectionTypes type, float cellSize)
        {
            if (type == RoomsConnectionTypes.None) return;

            var start = transform.position;
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
    }
}