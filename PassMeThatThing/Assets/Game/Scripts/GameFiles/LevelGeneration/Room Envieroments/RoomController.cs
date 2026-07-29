using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.LevelGeneration.Room_Envieroments
{
    public class RoomController : MonoBehaviour
    {
        public bool IsPowerOn { get; private set; } = true;
        
        private readonly List<NetworkOutlineShader> _lights = new();
        private GlobalVisionShaderManager _globalmanger;
        public IReadOnlyList<NetworkOutlineShader> Lights => _lights;


        public void Start()
        {
            var globalmanger = FindFirstObjectByType<GlobalVisionShaderManager>();
            _globalmanger = globalmanger;
            globalmanger.RegisterRoom(this);
        }

        public void RegisterLight(NetworkOutlineShader roomLight)
        {
            if (!_lights.Contains(roomLight))
            {
                _lights.Add(roomLight);
                if (NetworkServer.active && !IsPowerOn)
                {
                    roomLight.SetVisionState(false);
                }
            }
        }


        public void UnregisterLight(NetworkOutlineShader roomLight)
        {
            if (_lights.Contains(roomLight))
                _lights.Remove(roomLight);
        }

        public void SetLightsState(bool state)
        {
            foreach (var networkOutlineShader in _lights.Where(networkOutlineShader => networkOutlineShader != null))
            {
                networkOutlineShader.SetVisionState(state);
            }
        }

        public void OnDestroy()
        {
            _globalmanger.UnregisterRoom(this);
        }
    }
}