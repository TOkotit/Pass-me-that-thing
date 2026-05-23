using Game.Gameplay.View.UI;
using Game.MainMenu.View.UI.ScreenMainMenu;
using Game.MainMenu.View.UI.ScreenOptionsMenu;
using Game.UI;
using R3;
using VContainer;

namespace Game.MainMenu.View.UI
{
    public class MainMenuUIManager : UIManager
    {
        private MainMenuUIRootViewModel rootUI;

        public MainMenuUIManager(IObjectResolver container) : base(container)
        {
            rootUI = Container.Resolve<MainMenuUIRootViewModel>();
        }
        
        public ScreenMainMenuViewModel OpenScreenMainMenu()
        {
            var viewModel = new ScreenMainMenuViewModel(this, Container);
            
            rootUI.OpenScreen(viewModel);

            return viewModel;
        }
        
        public ScreenOptionsMenuViewModel OpenScreenOptionsMenu()
        {
            var viewModel = new ScreenOptionsMenuViewModel(this, Container);
            
            rootUI.OpenScreen(viewModel);

            return viewModel;
        }
    }
}