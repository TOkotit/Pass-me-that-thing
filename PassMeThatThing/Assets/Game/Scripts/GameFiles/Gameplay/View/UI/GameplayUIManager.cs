using System;
using System.Runtime.InteropServices;
using Enums;
using Game.Gameplay.View.UI.ScreenBuild;
using Game.Gameplay.View.UI.ScreenMinigame;
using Game.Gameplay.View.UI.ScreenPauseMenu;
using Game.MainMenu.View.UI.ScreenOptionsMenu;
using Game.Scripts.GameFiles.Events;
using Game.UI;
using MainCharacter_old;
using R3;
using Systems;
using VContainer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Gameplay.View.UI
{
    public class GameplayUIManager : UIManager
    {
        // private MainCharacterCamera _mainCharacterCamera;
        // private MainCharacterMovement _mainCharacterMovement;
        private GameInputManager _gameInputManager;

        private GameplayUIRootViewModel rootUI;
        
        public GameplayUIManager(IObjectResolver container) : base(container)
        {
            rootUI = Container.Resolve<GameplayUIRootViewModel>();
            
            // _mainCharacterCamera = Container.Resolve<MainCharacterCamera>();
            // _mainCharacterMovement = Container.Resolve<MainCharacterMovement>();
            _gameInputManager = Container.Resolve<GameInputManager>();
            

        }

        public ScreenGameplayViewModel OpenScreenGameplay()
        {
            var viewModel = new ScreenGameplayViewModel(this, Container);
            
            LockUpCursor();
            
            rootUI.OpenScreen(viewModel);
            
            _gameInputManager.ToggleMap(InputMapType.Gameplay);
            
            return viewModel;
        }
        
        public ScreenMinigameViewModel OpenScreenMinigame(MinigameParameters  parameters)
        {
            var viewModel = new ScreenMinigameViewModel(this, Container, parameters);

            UnlockCursor();
            
            rootUI.OpenScreen(viewModel);
            
            _gameInputManager.ToggleMap(InputMapType.UI);
            
            return viewModel;
        }
        
        
        public ScreenPauseMenuViewModel OpenScreenPauseMenu()
        {
            var viewModel = new ScreenPauseMenuViewModel(this, Container);
            
            UnlockCursor();
            
            rootUI.OpenScreen(viewModel);
            _gameInputManager.ToggleMap(InputMapType.UI);

            return viewModel;
        }
        
        public ScreenBuildViewModel OpenScreenBuild()
        {
            var viewModel = new ScreenBuildViewModel(this, Container);
            
            UnlockCursor();
            rootUI.OpenScreen(viewModel);
            _gameInputManager.ToggleMap(InputMapType.Gameplay);

            return viewModel;
        }
        
        
        
        public ScreenOptionsViewModel OpenScreenOptions()
        {
            var viewModel = new ScreenOptionsViewModel(this, Container);
            
            UnlockCursor();
            
            rootUI.OpenScreen(viewModel);
            _gameInputManager.ToggleMap(InputMapType.UI);

            return viewModel;
        }
        
        // Блокировать или разблокировать курсор
        public void LockUpCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }
}