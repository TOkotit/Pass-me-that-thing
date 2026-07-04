using Ami.BroAudio;
using Game.Scripts.Systems;
using Game.UI;
using Systems;
using Utils;
using VContainer;

namespace Game.MainMenu.View.UI.ScreenOptionsMenu
{
    public class ScreenOptionsViewModel : WindowViewModel
    {
        public override string Id =>  "ScreenOptions";
        
        private readonly MainMenuUIManager _uiManager;
        private readonly GameManager _gameManager;
        private readonly ICoroutineRunner _coroutines;
        
        private readonly OptionsManager _optionsManager;
        
        
        public ScreenOptionsViewModel(MainMenuUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            _gameManager =  container.Resolve<GameManager>();
            _coroutines = container.Resolve<ICoroutineRunner>();
            
            _optionsManager = container.Resolve<OptionsManager>();
        }
        
        public void RequestGoToScreenMainMenu()
        {
            _uiManager.OpenScreenMainMenu();
        }
        
        public void RequestSaveOptions()
        {
            _optionsManager.SaveSettings();
        }

        public void RequestChangeAudioValueOptions(BroAudioType broAudioType, float value)
        {
            _optionsManager.SetAudioVolume(broAudioType, value);
        }
    }
}