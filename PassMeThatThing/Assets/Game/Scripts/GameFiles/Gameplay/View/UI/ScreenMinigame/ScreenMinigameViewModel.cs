using System;
using Game.Scripts.GameFiles.Events;
using Game.Scripts.GameFiles.GlobalStageManager;
using Game.Scripts.GameFiles.Items;
using Game.UI;
using R3;
using UnityEngine;
using VContainer;

namespace Game.Gameplay.View.UI.ScreenMinigame
{
    public class ScreenMinigameViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _uiManager;
        
        private readonly GameRandomEventManager _gameRandomEventManager;
        private readonly GameEventsDatabase _gameEventsDatabase;
        private readonly GlobalStageManager _globalStageManager;
        
        private readonly CompositeDisposable _subscriptions = new();
        
        private MinigameParameters _minigameParameters;
        
        public override string Id => "ScreenMinigame";

        public MinigameParameters Parameters => _minigameParameters;


        public ScreenMinigameViewModel(GameplayUIManager uiManager, 
            IObjectResolver container, MinigameParameters parameters)
        {
            _uiManager = uiManager;
            
            _gameEventsDatabase  = container.Resolve<GameEventsDatabase>();
            _gameRandomEventManager =  container.Resolve<GameRandomEventManager>();
            _globalStageManager = container.Resolve<GlobalStageManager>();
            
            _minigameParameters = parameters;
        }
        
        public void RequestCompleteMinigame()
        {
            // _gameRandomEventManager.CmdStopEventById(_minigameParameters.eventId);
            
            _minigameParameters.eventTerminal.CmdMinigameComplete();
            RequestCloseMinigame();
        }

        public void RequestCloseMinigame()
        {
            _minigameParameters.eventTerminal.CmdMinigameClose();
            _uiManager.OpenScreenGameplay();
        }
        
    }
}