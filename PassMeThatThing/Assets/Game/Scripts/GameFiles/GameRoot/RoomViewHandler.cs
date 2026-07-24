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

        public List<CustomRoomPlayer> players=new();
        
        public event Action<List<CustomRoomPlayer>> OnPlayersViewDataChanged;
        
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

        public void PlayersViewDataChanged()
        {
            OnPlayersViewDataChanged?.Invoke(players);
        }
    }
}