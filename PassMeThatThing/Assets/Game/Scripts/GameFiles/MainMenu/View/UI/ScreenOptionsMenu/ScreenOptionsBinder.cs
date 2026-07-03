using Ami.BroAudio;
using AYellowpaper.SerializedCollections;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MainMenu.View.UI.ScreenOptionsMenu
{
    public class ScreenOptionsBinder : WindowBinder<ScreenOptionsViewModel>
    {
        [SerializeField] private Button btnClose;
        [Header("Settings Pages")]
        [SerializeField] private SerializedDictionary<SettingsPage, GameObject> settingsPages;
        
        [Header("Audio Settings")]
        [SerializeField] private SerializedDictionary<BroAudioType, Slider> audioSliders;
        
        private void Start()
        {
            btnClose?.onClick.AddListener(OnCloseButtonClicked);
            
        }
        
        private void OnDestroy()
        {
            btnClose?.onClick.RemoveListener(OnCloseButtonClicked);
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