using System;
using System.Collections;
using Assets.Game.Scripts.GameFiles.GameRoot;
using Game.Gameplay.View.UI;
using Game.UI;
using Mirror;
using R3;
using Root;
using Systems;
using UnityEngine;
using Utils;
using VContainer;

namespace Game.MainMenu.View.UI.ScreenMainMenu
{
    public class ScreenMainMenuViewModel : WindowViewModel
    {
        public override string Id =>  "ScreenMainMenu";
        
        private readonly MainMenuUIManager _uiManager;
        private readonly GameManager _gameManager;
        private readonly ICoroutineRunner _coroutines;
        
        private NetworkManager  _networkRoomManager;
        private SteamLobbyManager _steamLobbyManager;

        private RootNetworkConfig _networkConfig;


        public ScreenMainMenuViewModel(MainMenuUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            _gameManager =  container.Resolve<GameManager>();
            _coroutines = container.Resolve<ICoroutineRunner>();
            
            _networkRoomManager = container.Resolve<NetworkManager>();
            
            _networkConfig = container.Resolve<RootNetworkConfig>();

            if (_networkConfig.IsSteamUsing)
                _steamLobbyManager = container.Resolve<SteamLobbyManager>();
        }
        
        public void RequestHost()
        {
            Debug.Log("RequestHost");
            if (_networkConfig.IsSteamUsing)
            {
                Debug.Log("[STEAM] Найдено Стим-лобби. Запускаем создание виртуальной комнаты...");
                _steamLobbyManager.CreateSteamLobby();
            }
            else
            {
                // Если скрипта SteamLobbyManager нет на объекте, запускаем обычный локальный хост (для Radmin)
                Debug.Log("[LOCAL] Запускаем стандартный Host...");
                _networkRoomManager.StartHost(); 
            }
        }

        public void RequestJoin()
        {
            if (_networkConfig.IsSteamUsing)
            {
                _steamLobbyManager.OpenFriends();
            }
            else
            {
                _networkRoomManager.StartClient();
            }
        }
        
        public void RequestIpAddress(string value)
        {
            _networkRoomManager.networkAddress = value;
        }
        
        public void RequestGoToScreenOptions()
        {
            _uiManager.OpenScreenOptionsMenu();
        }
    }
}