using System.Collections.Generic;
using Game.UI;
using Root;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.MainMenu.View.UI.ScreenMainMenu
{
    public class ScreenLobbyBinder : WindowBinder<ScreenLobbyViewModel>
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private VisualTreeAsset lobbyPlayerViewPrefab;
        
        private VisualElement _root;
        private Button _returnBtn;
        private Button _readyBtn;
        private GroupBox _lobbyPlayersContainer;

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            _returnBtn = _root.Q<Button>("ReturnBtn");
            _readyBtn =  _root.Q<Button>("ReadyBtn");
            _lobbyPlayersContainer = _root.Q<GroupBox>("PlayersContainer");
            
        }
        
        private void Start()
        {
            _returnBtn.RegisterCallback<ClickEvent>(OnGoOffline);
            _readyBtn.RegisterCallback<ClickEvent>(SetReady);
            
            ViewModel.RequestSubPlayerView(UpdateLobbyPlayerView);
        }

        private void OnDestroy()
        {
            _returnBtn.UnregisterCallback<ClickEvent>(OnGoOffline);
            _readyBtn.UnregisterCallback<ClickEvent>(SetReady);
            
            ViewModel.RequestUnsubPlayerView(UpdateLobbyPlayerView);
        }

        public void OnGoOffline(ClickEvent evt)
        {
            ViewModel.RequestGoOffline();
        }
        public void SetReady(ClickEvent evt)
        {
            ViewModel.RequestSetReadyState();
        }

        //Перестраивается вся коллекция
        //тк индексы румплееров меняются и из-за этого все ломается
        public void UpdateLobbyPlayerView(List<CustomRoomPlayer> d)
        {
            _lobbyPlayersContainer.Clear();
            
            foreach (var p in d)
            {
                var playerView = lobbyPlayerViewPrefab.Instantiate();
                _lobbyPlayersContainer.Add(playerView);

                playerView.Q<VisualElement>("LobbyPlayerImg").style.backgroundImage
                    = p.avatarImage is null ? null : new StyleBackground(p.avatarImage);
                    
                playerView.Q<Label>("LobbyPlayerNameLb").text 
                    = string.IsNullOrEmpty(p.nameText) 
                    ? "player_" + p.index.ToString()
                    : p.nameText; 
                playerView.Q<Label>("LobbyPlayerReadyStatusLb").text = p.readyToBegin ?  "Ready" : "Not Ready";
            }
        }
    }
}