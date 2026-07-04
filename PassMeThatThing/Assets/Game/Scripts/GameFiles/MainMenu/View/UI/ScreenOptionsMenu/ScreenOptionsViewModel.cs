using System;
using Ami.BroAudio;
using Game.Gameplay.View.UI;
using Game.Scripts.Systems;
using Game.UI;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
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
            
            _gameInputManager.GameInput.UI.PauseMenu.performed += PauseMenuOnPerformed;
        }

        public override void Dispose()
        {
            // Debug.Log("Disposing ScreenGameplayViewModel");
            _gameInputManager.GameInput.UI.PauseMenu.performed -= PauseMenuOnPerformed;
        }

        private void PauseMenuOnPerformed(InputAction.CallbackContext c)
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

        public void RequestChangeFullscreen(bool isFullscreen)
        {
            _optionsManager.SetFullScreen(isFullscreen);
        }
    }
}