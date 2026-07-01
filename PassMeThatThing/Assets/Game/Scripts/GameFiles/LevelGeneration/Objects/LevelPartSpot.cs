using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration.Objects
{
    public class LevelPartSpot : MonoBehaviour
    {
        public SpotType spotType;
        public Vector3 spotSize = new Vector3(1f, 1f, 1f);
        
        private void OnDrawGizmos()
        {
            var oldMatrix = Gizmos.matrix;
            
            Gizmos.matrix = transform.localToWorldMatrix;

            var baseColor = GetColorForType(spotType);

            Gizmos.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.3f);
            Gizmos.DrawCube(Vector3.zero, spotSize);

            Gizmos.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
            Gizmos.DrawWireCube(Vector3.zero, spotSize);

            Gizmos.color = Color.white;
            var frontPoint = Vector3.forward * (spotSize.z * 0.5f);
            Gizmos.DrawLine(Vector3.zero, frontPoint + Vector3.forward * 0.5f);
            Gizmos.DrawWireSphere(frontPoint + Vector3.forward * 0.5f, 0.1f);

            Gizmos.matrix = oldMatrix;
        }

        private Color GetColorForType(SpotType type)
        {
            return type switch
            {
                SpotType.Pipe => Color.blue,      
                SpotType.Lamp => Color.yellow,        
                SpotType.EventTerminal => Color.red,   
                SpotType.Ventilation => Color.cyan,   
                _ => Color.white
            };
        }
    }
}