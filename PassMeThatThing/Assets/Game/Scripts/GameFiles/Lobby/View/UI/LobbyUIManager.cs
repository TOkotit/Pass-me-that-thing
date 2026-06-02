using Game.Gameplay.View.UI;
using Game.MainMenu.View.UI.ScreenMainMenu;
using Game.MainMenu.View.UI.ScreenOptionsMenu;
using Game.UI;
using R3;
using VContainer;

namespace Game.MainMenu.View.UI
{
    public class LobbyUIManager : UIManager
    {
        private LobbyUIRootViewModel rootUI;

        public LobbyUIManager(IObjectResolver container) : base(container)
        {
            rootUI = Container.Resolve<LobbyUIRootViewModel>();
        }
        
        public ScreenLobbyViewModel OpenScreenLobby()
        {
            var viewModel = new ScreenLobbyViewModel(this, Container);
            
            rootUI.OpenScreen(viewModel);

            return viewModel;
        }
    }
}