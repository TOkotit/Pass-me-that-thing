using Root;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.GameRoot
{
    public class ConnectedPlayers
    {
        public List<CustomRoomPlayer> players = new();

        public event Action<List<CustomRoomPlayer>> OnPlayersViewDataChanged;

        public CustomRoomPlayer localPlayer;

        public void PlayersViewDataChanged()
        {
            OnPlayersViewDataChanged?.Invoke(players);
        }
    }
}