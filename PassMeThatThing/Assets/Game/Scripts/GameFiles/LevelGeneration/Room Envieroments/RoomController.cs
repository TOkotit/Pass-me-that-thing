using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration.Room_Envieroments
{
    public class RoomController : MonoBehaviour
    {

        
        public int RoomId { get; private set; } = -1;
        
        public void SetRoomId(int id)
        {
            RoomId = id;
        }

        private void Start()
        {
            if (RoomId < 0)
                Debug.LogWarning($"RoomId не назначен для {gameObject.name} — вызовите SetRoomId() из генератора уровня.");
        }
        
    }
}