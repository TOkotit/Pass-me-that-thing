using System;
using System.Collections.Generic;
using Mirror;
using ObservableCollections;
using UnityEngine;

namespace Root
{    
    public class RoomViewHandler
    {
        private bool _localReadyState;
        
        public bool LocalReadyState
        {
            get => _localReadyState;
            set
            {
                _localReadyState = value;
                LocalReadyStateChanged?.Invoke(value);
            }
        }
        
        public event Action<bool> LocalReadyStateChanged;
    }
}