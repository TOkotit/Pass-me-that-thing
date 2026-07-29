using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration.Room_Envieroments
{
    public class RoomController : MonoBehaviour
    {
        private readonly List<RoomLight> _lights = new();

        public IReadOnlyList<RoomLight> Lights => _lights;

        public void RegisterLight(RoomLight roomLight)
        {
            if (!_lights.Contains(roomLight))
                _lights.Add(roomLight);
        }

        public void UnregisterLight(RoomLight roomLight)
        {
            if (_lights.Contains(roomLight))
                _lights.Remove(roomLight);
        }
    }
}