using System;
using Assets.Game.Scripts.GameFiles.UIWorld;
using Game.Scripts.GameFiles.GameRandomEvents;
using Game.Scripts.GameFiles.GlobalStageManager;
using Game.Scripts.GameFiles.Items;
using Game.UI;
using R3;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Game.Gameplay.View.UI.ScreenMinigame
{
    public class ScreenMinigameViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _uiManager;
        
        private readonly GameRandomEventManager _gameRandomEventManager;
        private readonly GameEventsDatabase _gameEventsDatabase;
        private readonly GlobalStageManager _globalStageManager;
        private readonly GameInputManager _gameInputManager;
        
        private readonly CompositeDisposable _subscriptions = new();
        
        private MinigameParameters _minigameParameters;
        
        public override string Id => "ScreenMinigame";

        public Vector3 position;
        public Quaternion rotation;

        public MinigameParameters Parameters => _minigameParameters;


        public ScreenMinigameViewModel(GameplayUIManager uiManager, 
            IObjectResolver container, MinigameParameters parameters)
        {
            _uiManager = uiManager;
            
            _gameEventsDatabase  = container.Resolve<GameEventsDatabase>();
            _gameRandomEventManager =  container.Resolve<GameRandomEventManager>();
            _globalStageManager = container.Resolve<GlobalStageManager>();

            _gameInputManager = container.Resolve<GameInputManager>();

            _minigameParameters = parameters;
            position = parameters.position;
            rotation = parameters.rotation;

            _gameInputManager.GameInput.Gameplay.PauseMenu.performed += PauseMenuPerformed;
        }

        public override void Dispose()
        {
            // Debug.Log("Disposing ScreenMinigameViewModel");
            _gameInputManager.GameInput.Gameplay.PauseMenu.performed -= PauseMenuPerformed;
        }

        private void PauseMenuPerformed(InputAction.CallbackContext context)
        {
            RequestCloseMinigame();
        }

        public void RequestCompleteMinigame()
        {
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