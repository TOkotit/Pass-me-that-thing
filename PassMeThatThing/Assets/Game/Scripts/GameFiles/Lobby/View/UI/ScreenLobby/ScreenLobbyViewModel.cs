using System;
using System.Collections.Generic;
using Assets.Game.Scripts.GameFiles.GameRoot;
using Game.UI;
using Mirror;
using Root;
using VContainer;

namespace Game.MainMenu.View.UI.ScreenMainMenu
{
    public class ScreenLobbyViewModel : WindowViewModel
    {
        public override string Id =>  "ScreenLobby";
        
        private readonly LobbyUIManager _uiManager;
        
        private readonly NetworkRoomManager _networkRoomManager;
        private readonly NetworkIdentity _networkIdentity;

        private readonly RoomViewHandler _roomViewHandler;
        private readonly ConnectedPlayers _connectedPlayers;

        public ScreenLobbyViewModel(LobbyUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;

            if (container.Resolve<NetworkManager>() is NetworkRoomManager roomManager)
            {
                _networkRoomManager = roomManager;
            }
            _networkIdentity = container.Resolve<NetworkIdentity>();
            _roomViewHandler = container.Resolve<RoomViewHandler>();
            _connectedPlayers = container.Resolve<ConnectedPlayers>();
        }
        
        public void RequestGoOffline()
        {
            if (_networkIdentity.isServer && _networkIdentity.isClient)
            {
                _networkRoomManager.StopHost();
            }
            else if (_networkIdentity.isClient)
            {
                _networkRoomManager.StopClient();
            }
        }

        public void RequestSetReadyState()
        {
            _roomViewHandler.LocalReadyState = !_roomViewHandler.LocalReadyState;
        }
        
        public void RequestSubPlayerView(Action<List<CustomRoomPlayer>> f)
        {
            f(_connectedPlayers.players);

            _connectedPlayers.OnPlayersViewDataChanged += f;
        }
        
        public void RequestUnsubPlayerView(Action<List<CustomRoomPlayer>> f)
        {
            _connectedPlayers.OnPlayersViewDataChanged -= f;
        }
    }
}