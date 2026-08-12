
using Enums;
using Game.Gameplay.View.UI.ScreenBuild;
using Game.Gameplay.View.UI.ScreenDefeat;
using Game.Gameplay.View.UI.ScreenMinigame;
using Game.Gameplay.View.UI.ScreenPauseMenu;
using Game.MainMenu.View.UI.ScreenOptionsMenu;
using Game.UI;

using Systems;
using VContainer;
using UnityEngine;
using Game.Scripts.GameFiles.GameRandomEvents;
using Assets.Game.Scripts.GameFiles.UIWorld;
using Assets.Game.Scripts.GameFiles.Gameplay.View.UI.WorldUI.WindowDescription;


namespace Game.Gameplay.View.UI
{
    public class GameplayUIManager : UIManager
    {
        private GameInputManager _gameInputManager;

        private GameplayUIRootViewModel rootUI;
        private WorldUIRootViewModel _worldUI;
        
        public GameplayUIManager(IObjectResolver container) : base(container)
        {
            rootUI = Container.Resolve<GameplayUIRootViewModel>();
            _worldUI = Container.Resolve<WorldUIRootViewModel>();

            _gameInputManager = Container.Resolve<GameInputManager>();
        }

        public ScreenGameplayViewModel OpenScreenGameplay()
        {
            var viewModel = new ScreenGameplayViewModel(this, Container);
            
            LockCursor();
            
            _gameInputManager.ToggleMap(InputMapType.Gameplay);
            _gameInputManager.EnableMouse();
            
            rootUI.OpenScreen(viewModel);
            
            return viewModel;
        }
        
        public ScreenMinigameViewModel OpenScreenMinigame(MinigameParameters  parameters)
        {
            var viewModel = new ScreenMinigameViewModel(this, Container, parameters);

            UnlockCursor();
            _gameInputManager.ToggleMap(InputMapType.UI);
            
            rootUI.OpenScreen(viewModel);
            
            return viewModel;
        }
        
        
        public ScreenPauseMenuViewModel OpenScreenPauseMenu()
        {
            var viewModel = new ScreenPauseMenuViewModel(this, Container);
            
            UnlockCursor();
            _gameInputManager.ToggleMap(InputMapType.UI);
            
            rootUI.OpenScreen(viewModel);
            
            return viewModel;
        }
        
        public ScreenWireMenuViewModel OpenScreenWireMenu()
        {
            var viewModel = new ScreenWireMenuViewModel(this, Container);
            
            UnlockCursor();
            
            _gameInputManager.ToggleMap(InputMapType.Gameplay);
            _gameInputManager.DisableMouse();
            
            rootUI.OpenScreen(viewModel);
            

            return viewModel;
        }

        public ScreenBuildViewModel OpenScreenBuild()
        {
            var viewModel = new ScreenBuildViewModel(this, Container);
            
            LockCursor();
            
            _gameInputManager.ToggleMap(InputMapType.Gameplay);
            _gameInputManager.DisableMouse();
            
            rootUI.OpenScreen(viewModel);
            
            return viewModel;
        }
        
        public ScreenCraftMenuViewModel OpenScreenCraft()
        {
            var viewModel = new ScreenCraftMenuViewModel(this, Container);
            
            UnlockCursor();
            _gameInputManager.ToggleMap(InputMapType.UI);
            
            rootUI.OpenScreen(viewModel);
            
            return viewModel;
        }
        
        public ScreenDefeatViewModel OpenScreenDefeat()
        {
            var viewModel = new ScreenDefeatViewModel(this, Container);
            
            UnlockCursor();
            _gameInputManager.ToggleMap(InputMapType.UI);
            
            rootUI.OpenScreen(viewModel);
            
            return viewModel;
        }
        
        public ScreenOptionsViewModel OpenScreenOptions()
        {
            var viewModel = new ScreenOptionsViewModel(this, Container);
            
            UnlockCursor();
            _gameInputManager.ToggleMap(InputMapType.UI);
            
            rootUI.OpenScreen(viewModel);
            

            return viewModel;
        }
        
        // Блокировать или разблокировать курсор
        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        //WorldSpace

        public WindowDescriptionViewModel OpenWindowDescription()
        {
            var viewModel = new WindowDescriptionViewModel(this, Container);

            _worldUI.OpenWorldWindow(viewModel);


            return viewModel;
        }

        public void CloseWindowDescription(WindowDescriptionViewModel viewModel)
        {
            _worldUI.CloseWorldWindow(viewModel);
        }
    }
}