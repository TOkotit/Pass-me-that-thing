using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration.Room_Envieroments
{
    public class RoomLight : MonoBehaviour
    {
        private void Start()
        {
            var roomController = GetComponentInParent<RoomController>();
        
            if (roomController != null)
                roomController.RegisterLight(this);
            else
                Debug.LogWarning($"RoomController не найден в родительских объектах для {gameObject.name}");
        }
    }
}