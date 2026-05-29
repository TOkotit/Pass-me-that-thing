using UnityEngine;

namespace MainCharacter_old
{
    [CreateAssetMenu(fileName = "CameraStats", menuName = "MainCharacter_old/CameraStatsSO")]
    public class CameraStatsSO : ScriptableObject
    {
        public float Sensitivity = 1f; 
        public float MaxPitch = 80f;  
    }
}