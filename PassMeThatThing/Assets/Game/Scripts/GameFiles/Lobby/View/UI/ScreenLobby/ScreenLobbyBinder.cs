using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MainMenu.View.UI.ScreenMainMenu
{
    public class ScreenLobbyBinder : WindowBinder<ScreenLobbyViewModel>
    {
        [SerializeField] private Button goOfflineBtn;


        private void Start()
        {
            goOfflineBtn.onClick.AddListener(OnGoOffline);
        }

        private void OnDestroy()
        {
            goOfflineBtn.onClick.RemoveListener(OnGoOffline);
        }


        public void OnGoOffline()
        {
            ViewModel.RequestGoOffline();
        }
    }
}