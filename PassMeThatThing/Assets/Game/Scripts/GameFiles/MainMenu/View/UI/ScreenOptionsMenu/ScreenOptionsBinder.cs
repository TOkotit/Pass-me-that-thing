using System;
using System.Collections.Generic;
using System.Linq;
using Ami.BroAudio;
using Assets.Game.Scripts.GameFiles.MainMenu.View.UI.ScreenOptionsMenu;
using Assets.Game.Scripts.Systems;
using Enums;
using Game.Scripts.Systems;
using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Game.MainMenu.View.UI.ScreenOptionsMenu
{
    public class ScreenOptionsBinder : WindowBinder<ScreenOptionsViewModel>
    {
        [SerializeField] private UIDocument uiDocument;

        private VisualElement _root;
        private Button _closeBtn;
        private Button _saveBtn;
        private VisualElement _rebindBlockImage;

        //gameSettings
        //

        //audio
        private Slider _allSlider;
        private Slider _musicSlider;
        private Slider _sfxSlider;

        //video
        private CustomDropdown _resolutionsDropdown;
        private CustomDropdown _fullscreenDropdown;
        private List<string> _screenNames = new List<string>();
        private List<OptionsScreenMode> _screenModes = new();

        private List<string> _resolutionNames = new();

        //controls
        private Slider _sensitivitySlider;
        private GroupBox _rebindsContainer;
        private Dictionary<InputMapType, List<InputAction>> _inputActions;
        private List<RebindButton> _rebindButtons = new();

        private void Awake()
        {
            _root = uiDocument.rootVisualElement;
            _closeBtn = _root.Q<Button>("CloseBtn");
            _saveBtn = _root.Q<Button>("SaveBtn");
            _rebindBlockImage = _root.Q<VisualElement>("RebindBlockImg");

            _allSlider = _root.Q<Slider>("AllAudioSlider");
            _musicSlider = _root.Q<Slider>("MusicAudioSlider");
            _sfxSlider = _root.Q<Slider>("SFXAudioSlider");

            _resolutionsDropdown = _root.Q<CustomDropdown>("ResolutionDropdown");
            _fullscreenDropdown = _root.Q<CustomDropdown>("FullscreenDropdown");

            _sensitivitySlider = _root.Q<Slider>("MouseSensitivitySlider");
            _rebindsContainer = _root.Q<GroupBox>("RebindsContainer");
        }


        private void Start()
        {
            ViewModel.RequestInitLoadOptions(InitUpdate);

            _allSlider.RegisterValueChangedCallback<float>(OnAllSliderValueChanged);
            _musicSlider.RegisterValueChangedCallback<float>(OnMusicSliderValueChanged);
            _sfxSlider.RegisterValueChangedCallback<float>(OnSFXSliderValueChanged);

            _resolutionsDropdown.RegisterValueChangedCallback<string>(OnResolutionsDropdownValueChanged);
            _fullscreenDropdown.RegisterValueChangedCallback<string>(OnFullscreenValueChanged);

            _closeBtn.RegisterCallback<ClickEvent>(OnCloseButtonClicked);
            _saveBtn.RegisterCallback<ClickEvent>(OnSaveButtonClicked);

            _sensitivitySlider.RegisterValueChangedCallback<float>(OnMouseSensitivitySliderValueChanged);
        }
        
        private void OnDestroy()
        {
            _allSlider.UnregisterValueChangedCallback<float>(OnAllSliderValueChanged);
            _musicSlider.UnregisterValueChangedCallback<float>(OnMusicSliderValueChanged);
            _sfxSlider.UnregisterValueChangedCallback<float>(OnSFXSliderValueChanged);

            _resolutionsDropdown.UnregisterValueChangedCallback<string>(OnResolutionsDropdownValueChanged);
            _fullscreenDropdown.UnregisterValueChangedCallback<string>(OnFullscreenValueChanged);

            _closeBtn.UnregisterCallback<ClickEvent>(OnCloseButtonClicked);
            _saveBtn.UnregisterCallback<ClickEvent>(OnSaveButtonClicked);

            _sensitivitySlider.UnregisterValueChangedCallback<float>(OnMouseSensitivitySliderValueChanged);

            foreach (var r in _rebindButtons)
            {
                r.OnRebindClick -= OnRebindStart;
            }
        }

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

            UpdateSensitivitySliderValue(data.mouseSensitivity);

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

            Debug.Log($"[AUDIO SLIDERS] {type}: {value}");
        }

        private void UpdateSensitivitySliderValue(float value)
        {
            _sensitivitySlider.value = value;
        }

        private void UpdateResolutions(Resolution[] resolutions, int currentResolutionIndex)
        {
            _resolutionNames.Clear();

            foreach (var resolution in resolutions)
            {
                var option = $"{resolution.width}x{resolution.height} {resolution.refreshRateRatio}Hz";
                _resolutionNames.Add(option);
            }

            _resolutionsDropdown.SetChoices(_resolutionNames);
            _resolutionsDropdown.value = _resolutionNames[currentResolutionIndex];
        }

        private void UpdateFullscreenToggle(OptionsScreenMode value)
        {
            _screenNames.Clear();
            _screenModes.Clear();

            foreach (OptionsScreenMode i in Enum.GetValues(typeof(OptionsScreenMode)))
            {
                var option = $"{i}";
                _screenNames.Add(option);
                _screenModes.Add(i);
            }

            _fullscreenDropdown.SetChoices(_screenNames);
            _fullscreenDropdown.value = value.ToString();
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

        private void OnMouseSensitivitySliderValueChanged(ChangeEvent<float> e)
        {
            ViewModel.RequestChangeMouseSensitivity(e.newValue);
        }

        private void OnResolutionsDropdownValueChanged(ChangeEvent<string> e)
        {
            ViewModel.RequestChangeResolution(_resolutionNames.IndexOf(e.newValue));
        }

        private void OnFullscreenValueChanged(ChangeEvent<string> e)
        {
            ViewModel.RequestChangeFullscreen(_screenModes[_screenNames.IndexOf(e.newValue)]);
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