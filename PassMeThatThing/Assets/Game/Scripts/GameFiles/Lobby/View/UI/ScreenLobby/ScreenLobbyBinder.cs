using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MainMenu.View.UI.ScreenMainMenu
{
    public class ScreenLobbyBinder : WindowBinder<ScreenLobbyViewModel>
    {
        [SerializeField] private Button _btnPlay;
        [SerializeField] private Button _btnContinue;
        [SerializeField] private Button _btnOptions;
        [SerializeField] private Button _btnExit;

        private void Start()
        {
            
        }

        private void OnDestroy()
        {

        }
    }
}