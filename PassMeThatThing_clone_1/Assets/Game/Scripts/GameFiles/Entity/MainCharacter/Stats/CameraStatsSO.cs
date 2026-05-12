using UnityEngine;

namespace MainCharacter
{
    [CreateAssetMenu(fileName = "CameraStats", menuName = "MainCharacter/CameraStatsSO")]
    public class CameraStatsSO : ScriptableObject
    {
        public float Sensitivity = 1f; 
        public float MaxPitch = 80f;  
    }
}