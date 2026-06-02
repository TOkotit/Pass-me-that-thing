using Game.UI;
using Systems;
using Utils;
using VContainer;

namespace Game.Gameplay.View.UI.ScreenPauseMenu
{
    public class ScreenPauseMenuViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _uiManager;
        private readonly GameManager _gameManager;
        private readonly ICoroutineRunner _coroutines;
        private readonly GameInputManager _gameInputManager;
        
        public override string Id => "ScreenPauseMenu";
        
        public ScreenPauseMenuViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            _gameManager =  container.Resolve<GameManager>();
            _coroutines = container.Resolve<ICoroutineRunner>();
            
            _gameInputManager = container.Resolve<GameInputManager>();
            
        }

        public void RequestGoToScreenGameplay()
        {
            _uiManager.OpenScreenGameplay();
        }
        
        public void RequestGoToMainMenu()
        {
            // _coroutines.StartRoutine(_gameManager.LoadMainMenu());
        }
        
        public void RequestGoToScreenOptions()
        {
            // _uiManager.OpenScreenGameplay();
        }
    }
}