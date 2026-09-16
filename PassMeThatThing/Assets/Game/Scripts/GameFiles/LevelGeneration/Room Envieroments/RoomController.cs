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
    }
}