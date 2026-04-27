using System.Collections.Generic;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.View.UI.ScreenPauseMenu
{
    public class ScreenPauseMenuBinder : WindowBinder<ScreenPauseMenuViewModel>
    {
        [SerializeField] private Button _btnContinue;
        [SerializeField] private Button _btnOptions;
        [SerializeField] private Button _btnExitToMainMenu;
        private void Start()
        {
            _btnContinue?.onClick.AddListener(OnContinueButtonClicked);
            _btnOptions?.onClick.AddListener(OnOptionsButtonClicked);
            _btnExitToMainMenu?.onClick.AddListener(OnExitButtonClicked);

        }

        private void OnDestroy()
        {
            _btnContinue?.onClick.RemoveListener(OnContinueButtonClicked);
            _btnOptions?.onClick.RemoveListener(OnOptionsButtonClicked);
            _btnExitToMainMenu?.onClick.RemoveListener(OnExitButtonClicked);
        }

        private void OnContinueButtonClicked()
        {
            ViewModel.RequestGoToScreenGameplay();
        }
        private void OnOptionsButtonClicked()
        {
            
        }
        private void OnExitButtonClicked()
        {
            ViewModel.RequestGoToMainMenu();
        }
    }
}