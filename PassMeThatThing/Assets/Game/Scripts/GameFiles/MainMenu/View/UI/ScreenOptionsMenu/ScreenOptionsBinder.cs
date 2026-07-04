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
        [SerializeField] private Button btnSave;
        [Header("Settings Pages")]
        [SerializeField] private SerializedDictionary<SettingsPage, GameObject> settingsPages;
        
        [Header("Audio Settings")]
        [SerializeField] private SerializedDictionary<BroAudioType, Slider> audioSliders;
        
        private void Start()
        {
            btnClose?.onClick.AddListener(OnCloseButtonClicked);
            btnSave?.onClick.AddListener(OnSaveButtonClicked);

            
            audioSliders[BroAudioType.All]?.onValueChanged.AddListener(OnAllSliderValueChanged);
            audioSliders[BroAudioType.Music]?.onValueChanged.AddListener(OnMusicSliderValueChanged);
            audioSliders[BroAudioType.SFX]?.onValueChanged.AddListener(OnSFXSliderValueChanged);
            
        }
        
        private void OnDestroy()
        {
            btnClose?.onClick.RemoveListener(OnCloseButtonClicked);
            btnSave?.onClick.RemoveListener(OnSaveButtonClicked);
            
            audioSliders[BroAudioType.All]?.onValueChanged.RemoveListener(OnAllSliderValueChanged);
            audioSliders[BroAudioType.Music]?.onValueChanged.RemoveListener(OnMusicSliderValueChanged);
            audioSliders[BroAudioType.SFX]?.onValueChanged.RemoveListener(OnSFXSliderValueChanged);
        }
        
        public void OpenSettingsPage(SettingsPage settingsPage)
        {
            foreach (var p in settingsPages)
            {
                p.Value.SetActive(false);
            }
            
            settingsPages[settingsPage].SetActive(true);
        }
        
        private void OnCloseButtonClicked()
        {
            ViewModel.RequestGoToScreenMainMenu();
        }
        
        private void OnSaveButtonClicked()
        {
            ViewModel.RequestSaveOptions();
        }

        private void OnAllSliderValueChanged(float value)
        {
            ViewModel.RequestChangeAudioValueOptions(BroAudioType.All, value);
        }
        
        private void OnMusicSliderValueChanged(float value)
        {
            ViewModel.RequestChangeAudioValueOptions(BroAudioType.Music, value);
        }
        
        private void OnSFXSliderValueChanged(float value)
        {
            ViewModel.RequestChangeAudioValueOptions(BroAudioType.SFX, value);
        }
        
    }
}