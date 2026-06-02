using System;
using System.Collections;
using Game.Gameplay.View.UI;
using Game.UI;
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
        

        public ScreenLobbyViewModel(LobbyUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            _gameManager =  container.Resolve<GameManager>();
            _coroutines = container.Resolve<ICoroutineRunner>();
        }
        
        public void RequestReady()
        {
            
        }
        
    }
}