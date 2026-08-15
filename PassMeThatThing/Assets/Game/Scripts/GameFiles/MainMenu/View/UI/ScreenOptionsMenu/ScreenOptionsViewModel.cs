using System;
using System.Collections.Generic;
using Ami.BroAudio;
using Assets.Game.Scripts.Systems;
using Enums;
using Game.Gameplay.View.UI;
using Game.Scripts.Systems;
using Game.UI;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Game.MainMenu.View.UI.ScreenOptionsMenu
{
    public class ScreenOptionsViewModel : WindowViewModel
    {
        public override string Id =>  "ScreenOptions";
        
        private readonly MainMenuUIManager _uiManager;
        private readonly GameplayUIManager _gameplayUIManager;
        private readonly GameInputManager _gameInputManager;
        
        private readonly OptionsManager _optionsManager;
        
        
        public ScreenOptionsViewModel(UIManager uiManager, IObjectResolver container)
        {
            if (uiManager is MainMenuUIManager manager)
                _uiManager = manager;
            else if  (uiManager is GameplayUIManager gameplayUIManager)
                _gameplayUIManager = gameplayUIManager;
            
            _optionsManager = container.Resolve<OptionsManager>();
            
            _gameInputManager = container.Resolve<GameInputManager>();
            
            _gameInputManager.GameInput.UI.PauseMenu.performed += OnClosePerformed;
        }

        public override void Dispose()
        {
            _gameInputManager.GameInput.UI.PauseMenu.performed -= OnClosePerformed;
        }

        private void OnClosePerformed(InputAction.CallbackContext c)
        {
            RequestGoToPrevScreen();
        }


        public void RequestGoToPrevScreen()
        {
            if (_uiManager == null)
            {
                _gameplayUIManager.OpenScreenPauseMenu();
            }
            else
            {
                _uiManager.OpenScreenMainMenu();
            }
        }
        
        public void RequestSaveOptions()
        {
            _optionsManager.SaveSettings();
        }

        public void RequestInitLoadOptions(Action<OptionsData, Resolution[]> update)
        {
            update(_optionsManager.OptionsData, _optionsManager.Resolutions);
        }
        
        public void RequestChangeAudioValueOptions(BroAudioType broAudioType, float value)
        {
            _optionsManager.SetAudioVolume(broAudioType, value);
        }

        public void RequestChangeResolution(int index)
        {
            _optionsManager.SetResolution(index);
        }

        private void GetAllActionRefs()
        {
            _optionsManager.GetAllActions();
        }

        public void RequestChangeFullscreen(OptionsScreenMode isFullscreen)
        {
            _optionsManager.SetFullScreen(isFullscreen);
        }

        public void RequestChangeMouseSensitivity(float value)
        {
            _optionsManager.SetMouseSensitivity(value);
        }

        public void RequestStartKeyRebind(InputAction inputActionReference, int targetIndex=-1, Action callback=null)
        {
            _optionsManager.StartRebindKey(inputActionReference, targetIndex, callback);
        }
        
        public Dictionary<InputMapType,List<InputAction>> RequestGetAllInputActions()
        {
            return _optionsManager.GetAllActions();
        }
    }
}