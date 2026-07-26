using System;
using System.Collections.Generic;
using System.Linq;
using Ami.BroAudio;
using Assets.Game.Scripts.GameFiles.MainMenu.View.UI.ScreenOptionsMenu;
using AYellowpaper.SerializedCollections;
using Enums;
using Game.Scripts.GameFiles.MainMenu.View.UI.ScreenOptionsMenu;
using Game.Scripts.Systems;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Game.MainMenu.View.UI.ScreenOptionsMenu
{
    public class ScreenOptionsBinder : WindowBinder<ScreenOptionsViewModel>
    {
        //[SerializeField] private Button btnClose;
        //[SerializeField] private Button btnSave;
        //[Header("Settings Pages")]
        //[SerializeField] private SerializedDictionary<SettingsPage, GameObject> settingsPages;
        //[SerializeField] private SerializedDictionary<SettingsPage, Button> btnSettingsPages;
        
        //[Header("Audio Settings")]
        //[SerializeField] private SerializedDictionary<BroAudioType, Slider> audioSliders;
        
        //[Header("Video Settings")]
        //[SerializeField] private TMP_Dropdown resolutionsDropdown;
        //[SerializeField] private Toggle fullscreenToggle;
        
        //[Header("Keyboard Settings")]
        //[SerializeField] private RebindObject rebindButtonPrefab;
        //[SerializeField] private GameObject rebindBackground;
        //[SerializeField] private GameObject scrollView;

        //private Dictionary<RebindObject, InputAction> _inputActionReferences = new();

        private Dictionary<InputMapType, List<InputAction>>  _inputActions;
        private List<RebindButton> _rebindButtons = new();

        [SerializeField] private UIDocument uiDocument;
        //[SerializeField] private VisualTreeAsset rebindPrefab;

        private VisualElement _root;
        private Button _closeBtn;
        private Button _saveBtn;
        private VisualElement _rebindBlockImage;
        //gameSettings

        //audio
        private Slider _allSlider;
        private Slider _musicSlider;
        private Slider _sfxSlider;

        //video
        private DropdownField _resolutionsDropdown;
        private Toggle _fullscreenToggle;

        private List<string> _resolutionNames = new();

        //keyboard
        private GroupBox _rebindsContainer;

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            _closeBtn = _root.Q<Button>("CloseBtn");
            _saveBtn = _root.Q<Button>("SaveBtn");
            _rebindBlockImage = _root.Q<VisualElement>("RebindBlockImg");

            _allSlider = _root.Q<Slider>("AllAudioSlider");
            _musicSlider = _root.Q<Slider>("MusicAudioSlider");
            _sfxSlider = _root.Q<Slider>("SFXAudioSlider");

            _resolutionsDropdown = _root.Q<DropdownField>("ResolutionDropdown");
            _fullscreenToggle = _root.Q<Toggle>("FullscreenToggle");

            _rebindsContainer = _root.Q<GroupBox>("RebindsContainer");
        }


        private void Start()
        {
            ViewModel.RequestInitLoadOptions(InitUpdate);

            _allSlider.RegisterValueChangedCallback<float>(OnAllSliderValueChanged);
            _musicSlider.RegisterValueChangedCallback<float>(OnMusicSliderValueChanged);
            _sfxSlider.RegisterValueChangedCallback<float>(OnSFXSliderValueChanged);

            _resolutionsDropdown.RegisterValueChangedCallback<string>(OnResolutionsDropdownValueChanged);
            _fullscreenToggle.RegisterValueChangedCallback<bool>(OnFullscreenToggleValueChanged);

            _closeBtn.RegisterCallback<ClickEvent>(OnCloseButtonClicked);
            _saveBtn.RegisterCallback<ClickEvent>(OnSaveButtonClicked);

            //btnSettingsPages[SettingsPage.Settings].onClick.AddListener(OnBtnSettingsPageClick);
            //btnSettingsPages[SettingsPage.GameSettings].onClick.AddListener(OnBtnGameSettingsPageClick);
            //btnSettingsPages[SettingsPage.Audio].onClick.AddListener(OnBtnAudioSettingsPageClick);
            //btnSettingsPages[SettingsPage.Video].onClick.AddListener(OnBtnVideoSettingsPageClick);
            //btnSettingsPages[SettingsPage.Keyboard].onClick.AddListener(OnBtnKeyboardSettingsPageClick);

            //resolutionsDropdown.onValueChanged.AddListener(OnResolutionsDropdownValueChanged);
            //fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleValueChanged);

            //audioSliders[BroAudioType.None]?.onValueChanged.AddListener(OnAllSliderValueChanged);
            //audioSliders[BroAudioType.Music]?.onValueChanged.AddListener(OnMusicSliderValueChanged);
            //audioSliders[BroAudioType.SFX]?.onValueChanged.AddListener(OnSFXSliderValueChanged);

            //btnClose?.onClick.AddListener(OnCloseButtonClicked);
            //btnSave?.onClick.AddListener(OnSaveButtonClicked);
        }
        
        private void OnDestroy()
        {
            _allSlider.UnregisterValueChangedCallback<float>(OnAllSliderValueChanged);
            _musicSlider.UnregisterValueChangedCallback<float>(OnMusicSliderValueChanged);
            _sfxSlider.UnregisterValueChangedCallback<float>(OnSFXSliderValueChanged);

            _resolutionsDropdown.UnregisterValueChangedCallback<string>(OnResolutionsDropdownValueChanged);
            _fullscreenToggle.UnregisterValueChangedCallback<bool>(OnFullscreenToggleValueChanged);

            _closeBtn.UnregisterCallback<ClickEvent>(OnCloseButtonClicked);
            _saveBtn.UnregisterCallback<ClickEvent>(OnSaveButtonClicked);

            foreach (var r in _rebindButtons)
            {
                r.OnRebindClick -= OnRebindStart;
            }


            //btnSettingsPages[SettingsPage.Settings].onClick.RemoveListener(OnBtnSettingsPageClick);
            //btnSettingsPages[SettingsPage.GameSettings].onClick.RemoveListener(OnBtnGameSettingsPageClick);
            //btnSettingsPages[SettingsPage.Audio].onClick.RemoveListener(OnBtnAudioSettingsPageClick);
            //btnSettingsPages[SettingsPage.Video].onClick.RemoveListener(OnBtnVideoSettingsPageClick);
            //btnSettingsPages[SettingsPage.Keyboard].onClick.RemoveListener(OnBtnKeyboardSettingsPageClick);

            //resolutionsDropdown.onValueChanged.RemoveListener(OnResolutionsDropdownValueChanged);

            //fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggleValueChanged);

            //audioSliders[BroAudioType.None]?.onValueChanged.RemoveListener(OnAllSliderValueChanged);
            //audioSliders[BroAudioType.Music]?.onValueChanged.RemoveListener(OnMusicSliderValueChanged);
            //audioSliders[BroAudioType.SFX]?.onValueChanged.RemoveListener(OnSFXSliderValueChanged);

            //foreach (var p in _inputActionReferences)
            //{
            //    p.Key?.button.onClick.RemoveAllListeners();
            //}

            //btnClose?.onClick.RemoveListener(OnCloseButtonClicked);
            //btnSave?.onClick.RemoveListener(OnSaveButtonClicked);
        }

        //private void OnBtnSettingsPageClick() => OpenSettingsPage(SettingsPage.Settings);
        //private void OnBtnGameSettingsPageClick() => OpenSettingsPage(SettingsPage.GameSettings);
        //private void OnBtnAudioSettingsPageClick() => OpenSettingsPage(SettingsPage.Audio);
        //private void OnBtnVideoSettingsPageClick() => OpenSettingsPage(SettingsPage.Video);
        //private void OnBtnKeyboardSettingsPageClick() => OpenSettingsPage(SettingsPage.Keyboard);
        
        //public void OpenSettingsPage(SettingsPage settingsPage)
        //{
        //    foreach (var p in settingsPages)
        //    {
        //        p.Value.SetActive(false);
        //    }
            
        //    settingsPages[settingsPage].SetActive(true);
            
        //    btnSettingsPages[SettingsPage.Settings].gameObject
        //        .SetActive(settingsPage != SettingsPage.Settings);
        //}
        
        private void OnCloseButtonClicked(ClickEvent e)
        {
            ViewModel.RequestGoToPrevScreen();
        }
        
        private void OnSaveButtonClicked(ClickEvent e)
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
                for (var ai = 0; ai < p.Value.Count; ai++)
                {
                    if (p.Value[ai].name == "MouseDrag") continue;
                    if (p.Value[ai].name == "Zoom") continue;
                    if (p.Value[ai].type == InputActionType.Button)
                    {
                        //var i = Instantiate(rebindButtonPrefab, scrollView.transform);
                        //_inputActionReferences.Add(i, a);
                        //i.BindingIndex = 0;

                        //i.button.onClick.AddListener(() => OnRebindStart(a));

                        var rBtn = new RebindButton();
                        _rebindsContainer.Add(rBtn);

                        rBtn.inputActionId = ai;
                        rBtn.compBindingId = -1;
                        rBtn.inputMapType = p.Key;

                        _rebindButtons.Add(rBtn);
                        rBtn.OnRebindClick += OnRebindStart;

                    }
                    else if (p.Value[ai].type == InputActionType.Value)
                    {
                        Debug.Log($"[REBINDS] Composite {p.Value[ai].name} : {string.Join(", ", p.Value[ai].bindings.ToList().Select(b => b.name).ToList())}");

                        for (int b = 0; b < p.Value[ai].bindings.Count; b++)
                        {
                            if (p.Value[ai].bindings[b].isComposite) continue;

                            //var i = Instantiate(rebindButtonPrefab, scrollView.transform);
                            //_inputActionReferences.Add(i, a);
                            //i.BindingIndex = b;

                            //i.button.onClick.AddListener(() => OnRebindStart(a, i.BindingIndex));

                            var rBtn = new RebindButton();
                            _rebindsContainer.Add(rBtn);

                            rBtn.inputActionId = ai;
                            rBtn.compBindingId = b;
                            rBtn.inputMapType = p.Key;

                            _rebindButtons.Add(rBtn);
                            rBtn.OnRebindClick += OnRebindStart;
                        }
                    }
                }
                UpdateAllKeysNames();
            }
        }

        private void UpdateAllKeysNames()
        {
            //foreach (var p in _inputActionReferences)
            //{
            //    if (p.Value.type == InputActionType.Button)
            //    {
            //        p.Key.text.text 
            //            = $"{p.Value.name} " +
            //             $"- {InputControlPath.ToHumanReadableString(p.Value.bindings[p.Key.BindingIndex].effectivePath)}";
            //    }
            //    else if (p.Value.type == InputActionType.Value)
            //    {
            //        p.Key.text.text 
            //            = $"{p.Value.bindings[p.Key.BindingIndex].name} " +
            //             $"- {InputControlPath.ToHumanReadableString(p.Value.bindings[p.Key.BindingIndex].effectivePath)}";
            //    }
            //}

            foreach (var rBtn in _rebindButtons)
            {
                var a = _inputActions[rBtn.inputMapType][rBtn.inputActionId];

                if (a.type == InputActionType.Button)
                {
                    var bindText = InputControlPath.ToHumanReadableString(a.bindings[0].effectivePath, 
                        InputControlPath.HumanReadableStringOptions.OmitDevice);
                    rBtn.text = $"{a.name} - {bindText}";
                }
                else if (a.type == InputActionType.Value)
                {
                    var bindText = InputControlPath.ToHumanReadableString(a.bindings[rBtn.compBindingId].effectivePath,
                        InputControlPath.HumanReadableStringOptions.OmitDevice);
                    rBtn.text = $"{a.name} - {bindText}";
                }
            }
        }

        private void UpdateSliderValue(BroAudioType type, float value)
        {
            switch (type)
            {
                case BroAudioType.All:
                    _allSlider.value = value;
                    break;
                case BroAudioType.Music:
                    _musicSlider.value = value;
                    break;
                case BroAudioType.SFX:
                    _sfxSlider.value = value;
                    break;
            }

            //if (type == BroAudioType.All) type = BroAudioType.None;
            Debug.Log($"[AUDIO SLIDERS] {type}: {value}");
            //audioSliders[type].value = value;
        }

        private void UpdateResolutions(Resolution[] resolutions, int currentResolutionIndex)
        {
            //resolutionsDropdown.ClearOptions();
            //var optionsText = new List<string>();


            //for (var i=0; i<resolutions.Length; i++)
            //{
            //    var option = $"{resolutions[i].width}x{resolutions[i].height} {resolutions[i].refreshRateRatio}Hz";
            //    optionsText.Add(option);
            //}

            //resolutionsDropdown.AddOptions(optionsText);
            //resolutionsDropdown.value = currentResolutionIndex;


            _resolutionNames.Clear();

            foreach (var resolution in resolutions)
            {
                var option = $"{resolution.width}x{resolution.height} {resolution.refreshRateRatio}Hz";
                _resolutionNames.Add(option);
            }

            _resolutionsDropdown.choices = _resolutionNames;
            _resolutionsDropdown.value = _resolutionNames[currentResolutionIndex];
        }

        private void UpdateFullscreenToggle(bool value)
        {
            _fullscreenToggle.value = value;
        }
        
        
        private void OnAllSliderValueChanged(ChangeEvent<float> e)
        {
            ViewModel.RequestChangeAudioValueOptions(BroAudioType.All, e.newValue);
        }
        
        private void OnMusicSliderValueChanged(ChangeEvent<float> e)
        {
            ViewModel.RequestChangeAudioValueOptions(BroAudioType.Music, e.newValue);
        }
        
        private void OnSFXSliderValueChanged(ChangeEvent<float> e)
        {
            ViewModel.RequestChangeAudioValueOptions(BroAudioType.SFX, e.newValue);
        }

        private void OnResolutionsDropdownValueChanged(ChangeEvent<string> e)
        {
            ViewModel.RequestChangeResolution(_resolutionNames.IndexOf(e.newValue));
        }

        private void OnFullscreenToggleValueChanged(ChangeEvent<bool> e)
        {
            ViewModel.RequestChangeFullscreen(e.newValue);
        }


        private void OnRebindStart(int inputActionIndex, int targetIndex, InputMapType type = InputMapType.Gameplay)
        {
            Debug.Log($"[REBINDS] OnRebindStart {inputActionIndex}, {targetIndex}");
            ViewModel.RequestStartKeyRebind(_inputActions[type][inputActionIndex],
                targetIndex, OnRebindEnd);
            
            _rebindBlockImage.visible = true;
        }

        private void OnRebindEnd()
        {
            _rebindBlockImage.visible = false;
            UpdateAllKeysNames();
        }
        
    }
}