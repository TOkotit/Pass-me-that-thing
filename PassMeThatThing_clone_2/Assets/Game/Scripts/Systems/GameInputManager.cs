using System;
using Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Systems
{
    public class GameInputManager : IDisposable
    {
        public GameInput GameInput { get; private set; }
        private InputMapType _current;
        
        public GameInputManager()
        {
            GameInput = new GameInput();
            GameInput.Gameplay.Enable();
            _current = InputMapType.Gameplay;
            Debug.Log("InputManager initialized");
        }

        public void ToggleMap(InputMapType map)
        {
            DisableMap(_current);
            EnableMap(map);
            _current = map;
        }
        
        private void EnableMap(InputMapType map)
        {
            switch (map)
            {
                case InputMapType.Gameplay:
                    GameInput.Gameplay.Enable();
                    break;
                
                case InputMapType.UI:
                    GameInput.UI.Enable();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(map), map, null);
            }
        }

        private void DisableMap(InputMapType map)
        {
            switch (map)
            {
                case InputMapType.Gameplay:
                    GameInput.Gameplay.Disable();
                    break;
                case InputMapType.UI:
                    GameInput.UI.Disable();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(map), map, null);
            }
        }

        public void Dispose()
        {
            if (GameInput != null)
            {
                GameInput.Gameplay.Disable();
                GameInput.UI.Disable();
                GameInput.Dispose();
                Debug.Log("Global GameInput disposed safely");
            }        }
    }
}