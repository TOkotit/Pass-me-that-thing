using System;
using System.Collections.Generic;
using System.Linq;
using Ami.BroAudio;
using AYellowpaper.SerializedCollections;
using Enums;
using Game.Scripts.GameFiles.MainMenu.View.UI.ScreenOptionsMenu;
using Game.Scripts.Systems;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

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
        
        [Header("Keyboard Settings")]
        [SerializeField] private RebindObject rebindButtonPrefab;
        [SerializeField] private GameObject rebindBackground;
        [SerializeField] private GameObject scrollView;
        private Dictionary<RebindObject, InputAction> _inputActionReferences = new();
        private Dictionary<InputMapType,List<InputAction>>  _inputActions;
        
        
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
            
            audioSliders[BroAudioType.None]?.onValueChanged.AddListener(OnAllSliderValueChanged);
            audioSliders[BroAudioType.Music]?.onValueChanged.AddListener(OnMusicSliderValueChanged);
            audioSliders[BroAudioType.SFX]?.onValueChanged.AddListener(OnSFXSliderValueChanged);

            // foreach (var p in _inputActionReferences)
            // {
            //     p.Key?.onClick.AddListener(() => OnRebindStart(p.Value));
            // }
            
            btnClose?.onClick.AddListener(OnCloseButtonClicked);
            btnSave?.onClick.AddListener(OnSaveButtonClicked);
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
            
            audioSliders[BroAudioType.None]?.onValueChanged.RemoveListener(OnAllSliderValueChanged);
            audioSliders[BroAudioType.Music]?.onValueChanged.RemoveListener(OnMusicSliderValueChanged);
            audioSliders[BroAudioType.SFX]?.onValueChanged.RemoveListener(OnSFXSliderValueChanged);
            
            foreach (var p in _inputActionReferences)
            {
                p.Key?.button.onClick.RemoveAllListeners();
            }
            
            btnClose?.onClick.RemoveListener(OnCloseButtonClicked);
            btnSave?.onClick.RemoveListener(OnSaveButtonClicked);
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

            _inputActions = ViewModel.RequestGetAllInputActions();
            
            foreach (var p in _inputActions)
            {
                if (p.Key == InputMapType.UI) continue; //TODO сделать отдельные вкладки для gameplay и ui кнопок
                foreach (var a in p.Value)
                {
                    if (a.name == "MouseDrag") continue;
                    if (a.name == "Zoom") continue;
                    if (a.type == InputActionType.Button)
                    {
                        var i = Instantiate(rebindButtonPrefab, scrollView.transform);
                        _inputActionReferences.Add(i, a);
                        i.BindingIndex = 0;
                        
                        i.button.onClick.AddListener(() => OnRebindStart(a));
                        
                    }
                    else if (a.type == InputActionType.Value)
                    {
                        Debug.Log($"{a.name} : {string.Join(", ",a.bindings.ToList().Select(b => b.name).ToList())}");

                        for (int b = 0; b < a.bindings.Count; b++)
                        {
                            if (a.bindings[b].isComposite) continue;
                            
                            var i = Instantiate(rebindButtonPrefab, scrollView.transform);
                            _inputActionReferences.Add(i, a);
                            i.BindingIndex = b;
                            
                            i.button.onClick.AddListener(() => OnRebindStart(a, i.BindingIndex));
                        }
                    }
                }
                UpdateAllKeysNames();
            }
        }

        private void UpdateAllKeysNames()
        {
            foreach (var p in _inputActionReferences)
            {
                if (p.Value.type == InputActionType.Button)
                {
                    p.Key.text.text 
                        = $"{p.Value.name} " +
                         $"- {InputControlPath.ToHumanReadableString(p.Value.bindings[p.Key.BindingIndex].effectivePath)}";
                }
                else if (p.Value.type == InputActionType.Value)
                {
                    p.Key.text.text 
                        = $"{p.Value.bindings[p.Key.BindingIndex].name} " +
                         $"- {InputControlPath.ToHumanReadableString(p.Value.bindings[p.Key.BindingIndex].effectivePath)}";
                }
            }
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

        private void OnRebindStart(InputAction inputAction, int targetIndex=-1)
        {
            // Debug.Log(targetIndex);
            ViewModel.RequestStartKeyRebind(inputAction, targetIndex, OnRebindEnd);
            
            rebindBackground.SetActive(true);
        }

        private void OnRebindEnd()
        {
            rebindBackground.SetActive(false);
            UpdateAllKeysNames();
        }
        
    }
}