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
    public class ScreenMainMenuViewModel : WindowViewModel
    {
        public override string Id =>  "ScreenMainMenu";
        
        private readonly MainMenuUIManager _uiManager;
        private readonly GameManager _gameManager;
        private readonly ICoroutineRunner _coroutines;
        

        public ScreenMainMenuViewModel(MainMenuUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            _gameManager =  container.Resolve<GameManager>();
            _coroutines = container.Resolve<ICoroutineRunner>();
        }
        
        public void RequestPlay()
        {
            Debug.Log("RequestPlay");
            _coroutines.StartRoutine(_gameManager.LoadGameplay());
        }

        public void RequestContinue()
        {
            
        }
        
        
    }
}