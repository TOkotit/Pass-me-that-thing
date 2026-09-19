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
        

        public ScreenMainMenuViewModel(MainMenuUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            _gameManager =  container.Resolve<GameManager>();
            _coroutines = container.Resolve<ICoroutineRunner>();
            
            _networkRoomManager = container.Resolve<NetworkManagerContainer>().Instance;
        }
        
        public void RequestHost()
        {
            Debug.Log("RequestHost");
            if (_networkRoomManager.TryGetComponent<SteamLobbyManager>(out var steamLobby))
            {
                Debug.Log("[STEAM] Найдено Стим-лобби. Запускаем создание виртуальной комнаты...");
                steamLobby.CreateSteamLobby();
            }
            else
            {
                // Если скрипта SteamLobbyManager нет на объекте, запускаем обычный локальный хост (для Radmin)
                Debug.Log("[LOCAL] Стим-менеджер не найден. Запускаем стандартный Host...");
                _networkRoomManager.StartHost(); 
            }
        }

        public void RequestJoin()
        {
            Debug.Log("RequestJoin");
            if (_networkRoomManager.TryGetComponent<SteamLobbyManager>(out var steamLobby))
            {
                Debug.Log("[STEAM] Найдено Стим-лобби. Запускаем создание виртуальной комнаты...");
                steamLobby.OpenFriends();
            }
            else
            {
                Debug.Log("[LOCAL] Стим-менеджер не найден. Запускаем стандартный клиент");
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