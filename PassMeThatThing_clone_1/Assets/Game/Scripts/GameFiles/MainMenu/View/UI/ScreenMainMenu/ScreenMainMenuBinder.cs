using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MainMenu.View.UI.ScreenMainMenu
{
    public class ScreenMainMenuBinder : WindowBinder<ScreenMainMenuViewModel>
    {
        [SerializeField] private Button _btnPlay;
        [SerializeField] private Button _btnContinue;
        [SerializeField] private Button _btnOptions;
        [SerializeField] private Button _btnExit;


        private void Start()
        {
            _btnPlay?.onClick.AddListener(OnPlayButtonClicked);
            _btnContinue?.onClick.AddListener(OnContinueButtonClicked);
            _btnOptions?.onClick.AddListener(OnOptionsButtonClicked);
            _btnExit?.onClick.AddListener(OnExitButtonClicked);
        }

        private void OnDestroy()
        {
            _btnPlay?.onClick.RemoveListener(OnPlayButtonClicked);
            _btnContinue?.onClick.RemoveListener(OnContinueButtonClicked);
            _btnOptions?.onClick.RemoveListener(OnOptionsButtonClicked);
            _btnExit?.onClick.RemoveListener(OnExitButtonClicked);
        }

        private void OnPlayButtonClicked()
        {
            ViewModel.RequestPlay();
        }
        
        private void OnContinueButtonClicked()
        {
            
        }
        
        private void OnOptionsButtonClicked()
        {
            
        }
        
        private void OnExitButtonClicked()
        {
            
        }
        
    }
}