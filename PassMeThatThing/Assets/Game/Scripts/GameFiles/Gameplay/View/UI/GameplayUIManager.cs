using System.Runtime.InteropServices;
using Enums;
using Game.Gameplay.View.UI.ScreenMinigame;
using Game.Gameplay.View.UI.ScreenPauseMenu;
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
            

            // _gameInputManager.GameInput.Gameplay.PauseMenu.performed += OnTogglePause;
        }
        
        

        public ScreenGameplayViewModel OpenScreenGameplay()
        {
            var viewModel = new ScreenGameplayViewModel(this, Container);
            // _mainCharacterMovement.UnlockMovement();
            // LockUpCursor();
            // UnlockCamera();
            rootUI.OpenScreen(viewModel);
            
            _gameInputManager.ToggleMap(InputMapType.Gameplay);
            
            return viewModel;
        }
        
        public ScreenMinigameViewModel OpenScreenMinigame(MinigameParameters  parameters)
        {
            var viewModel = new ScreenMinigameViewModel(this, Container, parameters);
            // _mainCharacterMovement.UnlockMovement();
            UnlockCursor();
            // UnlockCamera();
            rootUI.OpenScreen(viewModel);
            
            _gameInputManager.ToggleMap(InputMapType.UI);
            
            return viewModel;
        }
        
        private void OnTogglePause(InputAction.CallbackContext c)
        {
            if (rootUI.OpenedScreen.CurrentValue is not ScreenPauseMenuViewModel)
            {
                OpenScreenPauseMenu();
            }
            else
            {
                OpenScreenGameplay();
            }
            
        }
        
        
        public ScreenPauseMenuViewModel OpenScreenPauseMenu()
        {
            var viewModel = new ScreenPauseMenuViewModel(this, Container);
            
            
            // _mainCharacterMovement.LockUpMovement();
            UnlockCursor();
            // LockUpCamera();
            rootUI.OpenScreen(viewModel);
            _gameInputManager.ToggleMap(InputMapType.UI);

            return viewModel;
        }
        // блокировка камеры
        // public void LockUpCamera()
        // {
        //     _mainCharacterCamera.IsCameraRotating = false;
        // }
        //
        //
        // public void UnlockCamera()
        // {
        //     _mainCharacterCamera.IsCameraRotating = true;
        // }
        
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