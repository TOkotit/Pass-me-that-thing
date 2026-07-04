using System.Collections.Generic;
using Ami.BroAudio;
using AYellowpaper.SerializedCollections;
using Game.Scripts.Systems;
using Game.UI;
using TMPro;
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
        [SerializeField] private SerializedDictionary<SettingsPage, Button> btnSettingsPages;
        
        [Header("Audio Settings")]
        [SerializeField] private SerializedDictionary<BroAudioType, Slider> audioSliders;
        
        [Header("Video Settings")]
        [SerializeField] private TMP_Dropdown resolutionsDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        
        private void Start()
        {
            ViewModel.RequestInitLoadOptions(InitUpdate);

            btnSettingsPages[SettingsPage.Settings].onClick.AddListener(OnBtnSettingsPageClick);
            btnSettingsPages[SettingsPage.GameSettings].onClick.AddListener(OnBtnGameSettingsPageClick);
            btnSettingsPages[SettingsPage.Audio].onClick.AddListener(OnBtnAudioSettingsPageClick);
            btnSettingsPages[SettingsPage.Video].onClick.AddListener(OnBtnVideoSettingsPageClick);
            btnSettingsPages[SettingsPage.Keyboard].onClick.AddListener(OnBtnKeyboardSettingsPageClick);
            
            resolutionsDropdown.onValueChanged.AddListener(OnResolutionsDropdownValueChanged);
            
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleValueChanged);
            
            btnClose?.onClick.AddListener(OnCloseButtonClicked);
            btnSave?.onClick.AddListener(OnSaveButtonClicked);
            
            audioSliders[BroAudioType.None]?.onValueChanged.AddListener(OnAllSliderValueChanged);
            audioSliders[BroAudioType.Music]?.onValueChanged.AddListener(OnMusicSliderValueChanged);
            audioSliders[BroAudioType.SFX]?.onValueChanged.AddListener(OnSFXSliderValueChanged);
        }
        
        private void OnDestroy()
        {
            btnSettingsPages[SettingsPage.Settings].onClick.RemoveListener(OnBtnSettingsPageClick);
            btnSettingsPages[SettingsPage.GameSettings].onClick.RemoveListener(OnBtnGameSettingsPageClick);
            btnSettingsPages[SettingsPage.Audio].onClick.RemoveListener(OnBtnAudioSettingsPageClick);
            btnSettingsPages[SettingsPage.Video].onClick.RemoveListener(OnBtnVideoSettingsPageClick);
            btnSettingsPages[SettingsPage.Keyboard].onClick.RemoveListener(OnBtnKeyboardSettingsPageClick);
            
            resolutionsDropdown.onValueChanged.RemoveListener(OnResolutionsDropdownValueChanged);
            
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggleValueChanged);
            
            btnClose?.onClick.RemoveListener(OnCloseButtonClicked);
            btnSave?.onClick.RemoveListener(OnSaveButtonClicked);
            
            audioSliders[BroAudioType.None]?.onValueChanged.RemoveListener(OnAllSliderValueChanged);
            audioSliders[BroAudioType.Music]?.onValueChanged.RemoveListener(OnMusicSliderValueChanged);
            audioSliders[BroAudioType.SFX]?.onValueChanged.RemoveListener(OnSFXSliderValueChanged);
        }

        private void OnBtnSettingsPageClick() => OpenSettingsPage(SettingsPage.Settings);
        private void OnBtnGameSettingsPageClick() => OpenSettingsPage(SettingsPage.GameSettings);
        private void OnBtnAudioSettingsPageClick() => OpenSettingsPage(SettingsPage.Audio);
        private void OnBtnVideoSettingsPageClick() => OpenSettingsPage(SettingsPage.Video);
        private void OnBtnKeyboardSettingsPageClick() => OpenSettingsPage(SettingsPage.Keyboard);
        
        public void OpenSettingsPage(SettingsPage settingsPage)
        {
            foreach (var p in settingsPages)
            {
                p.Value.SetActive(false);
            }
            
            settingsPages[settingsPage].SetActive(true);
            
            btnSettingsPages[SettingsPage.Settings].gameObject
                .SetActive(settingsPage != SettingsPage.Settings);
        }
        
        private void OnCloseButtonClicked()
        {
            ViewModel.RequestGoToPrevScreen();
        }
        
        private void OnSaveButtonClicked()
        {
            ViewModel.RequestSaveOptions();
        }

        private void InitUpdate(OptionsData data, Resolution[] resolutions)
        {
            foreach (var p in data.audioValues)
            {
                UpdateSliderValue(p.Key, p.Value);
            }

            UpdateResolutions(resolutions, data.resolutionIndex);
            UpdateFullscreenToggle(data.isFullScreen);
        }

        private void UpdateSliderValue(BroAudioType type, float value)
        {
            if (type == BroAudioType.All) type = BroAudioType.None;
            Debug.Log($"{type}: {value}");
            audioSliders[type].value = value;
        }

        private void UpdateResolutions(Resolution[] resolutions, int currentResolutionIndex)
        {
            resolutionsDropdown.ClearOptions();
            var optionsText = new List<string>();
            
            
            for (var i=0; i<resolutions.Length; i++)
            {
                var option = $"{resolutions[i].width}x{resolutions[i].height} {resolutions[i].refreshRateRatio}Hz";
                optionsText.Add(option);
            }
            
            resolutionsDropdown.AddOptions(optionsText);
            resolutionsDropdown.value = currentResolutionIndex;
        }

        private void UpdateFullscreenToggle(bool value)
        {
            fullscreenToggle.isOn = value;
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

        private void OnResolutionsDropdownValueChanged(int index)
        {
            ViewModel.RequestChangeResolution(index);
        }

        private void OnFullscreenToggleValueChanged(bool value)
        {
            ViewModel.RequestChangeFullscreen(value);
        }
    }
}