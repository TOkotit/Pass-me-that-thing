using System;
using System.Collections;
using System.Collections.Generic;
using Game.Gameplay.View.UI;
using Game.UI;
using Mirror;
using ObservableCollections;
using R3;
using Root;
using Systems;
using UnityEngine;
using Utils;
using VContainer;

namespace Game.MainMenu.View.UI.ScreenMainMenu
{
    public class ScreenLobbyViewModel : WindowViewModel
    {
        public override string Id =>  "ScreenLobby";
        
        private readonly LobbyUIManager _uiManager;
        private readonly GameManager _gameManager;
        private readonly ICoroutineRunner _coroutines;
        
        private readonly NetworkRoomManager _networkRoomManager;
        private readonly NetworkIdentity _networkIdentity;

        private readonly RoomViewHandler _roomViewHandler;
        
        private readonly CompositeDisposable _subscriptions = new();

        public ScreenLobbyViewModel(LobbyUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            _gameManager =  container.Resolve<GameManager>();
            _coroutines = container.Resolve<ICoroutineRunner>();

            if (container.Resolve<NetworkManager>() is NetworkRoomManager roomManager)
            {
                _networkRoomManager = roomManager;
            }
            _networkIdentity = container.Resolve<NetworkIdentity>();
            _roomViewHandler = container.Resolve<RoomViewHandler>();
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
            f(_roomViewHandler.players);
            
            _roomViewHandler.OnPlayersViewDataChanged += f;
        }
        
        public void RequestUnsubPlayerView(Action<List<CustomRoomPlayer>> f)
        {
            _roomViewHandler.OnPlayersViewDataChanged -= f;
        }
    }
}