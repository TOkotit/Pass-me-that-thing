using Game.UI;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using VContainer;

namespace Game.Gameplay.View.UI.ScreenPauseMenu
{
    public class ScreenPauseMenuViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _uiManager;
        private readonly GameInputManager _gameInputManager;
        
        public override string Id => "ScreenPauseMenu";
        
        public ScreenPauseMenuViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            
            _gameInputManager = container.Resolve<GameInputManager>();
            
            _gameInputManager.GameInput.UI.PauseMenu.performed += PauseMenuPerformed;
            
        }

        public override void Dispose()
        {
            // Debug.Log("Disposing ScreenPauseMenuViewModel");
            _gameInputManager.GameInput.UI.PauseMenu.performed -= PauseMenuPerformed;
        }
        

        public void PauseMenuPerformed(InputAction.CallbackContext c)
        {
            RequestGoToScreenGameplay();
        }

        public void RequestGoToScreenGameplay()
        {
            _uiManager.OpenScreenGameplay();
        }
        
        public void RequestGoToMainMenu()
        {

        }
        
        public void RequestGoToScreenOptions()
        {
            _uiManager.OpenScreenOptions();
        }
    }
}