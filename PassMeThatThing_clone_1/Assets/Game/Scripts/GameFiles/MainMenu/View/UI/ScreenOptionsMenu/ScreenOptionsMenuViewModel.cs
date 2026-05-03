using Game.UI;
using Systems;
using Utils;
using VContainer;

namespace Game.MainMenu.View.UI.ScreenOptionsMenu
{
    public class ScreenOptionsMenuViewModel : WindowViewModel
    {
        public override string Id =>  "ScreenOptionsMenu";
        
        private readonly MainMenuUIManager _uiManager;
        private readonly GameManager _gameManager;
        private readonly ICoroutineRunner _coroutines;
        
        
        public ScreenOptionsMenuViewModel(MainMenuUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            _gameManager =  container.Resolve<GameManager>();
            _coroutines = container.Resolve<ICoroutineRunner>();
        }
    }
}