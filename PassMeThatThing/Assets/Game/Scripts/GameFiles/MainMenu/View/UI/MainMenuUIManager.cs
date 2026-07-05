using Enums;
using Game.Gameplay.View.UI;
using Game.MainMenu.View.UI.ScreenMainMenu;
using Game.MainMenu.View.UI.ScreenOptionsMenu;
using Game.UI;
using R3;
using Systems;
using VContainer;

namespace Game.MainMenu.View.UI
{
    public class MainMenuUIManager : UIManager
    {
        private MainMenuUIRootViewModel rootUI;
        private GameInputManager _gameInputManager;

        public MainMenuUIManager(IObjectResolver container) : base(container)
        {
            rootUI = Container.Resolve<MainMenuUIRootViewModel>();
            _gameInputManager = Container.Resolve<GameInputManager>();
        }
        
        public ScreenMainMenuViewModel OpenScreenMainMenu()
        {
            var viewModel = new ScreenMainMenuViewModel(this, Container);
            
            rootUI.OpenScreen(viewModel);
            _gameInputManager.ToggleMap(InputMapType.UI);
            
            return viewModel;
        }
        
        public ScreenOptionsViewModel OpenScreenOptionsMenu()
        {
            var viewModel = new ScreenOptionsViewModel(this, Container);
            
            rootUI.OpenScreen(viewModel);
            _gameInputManager.ToggleMap(InputMapType.UI);
            
            return viewModel;
        }
    }
}