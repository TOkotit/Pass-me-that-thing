using AYellowpaper.SerializedCollections;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MainMenu.View.UI.ScreenOptionsMenu
{
    public class ScreenOptionsBinder : WindowBinder<ScreenOptionsViewModel>
    {
        [SerializeField] private Button _btnClose;

        [SerializeField] private SerializedDictionary<SettingsPage, GameObject> settingsPages;
        
        private void Start()
        {
            _btnClose?.onClick.AddListener(OnCloseButtonClicked);
            
        }
        
        private void OnDestroy()
        {
            _btnClose?.onClick.RemoveListener(OnCloseButtonClicked);
        }
        
        private void OnCloseButtonClicked()
        {
            ViewModel.RequestGoToScreenMainMenu();
        }

        public void OpenSettingsPage(SettingsPage settingsPage)
        {
            foreach (var p in settingsPages)
            {
                p.Value.SetActive(false);
            }
            
            settingsPages[settingsPage].SetActive(true);
        }
        
    }
}