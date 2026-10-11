using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ami.BroAudio;
using Assets.Game.Scripts.Systems;
using Enums;
using Systems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using VContainer;

namespace Game.Scripts.Systems
{
    public class OptionsManager : IJsonSaveable
    {
        private string FilePath => Application.dataPath + "/" + "OptionsData.json";
        private string RebindFilePath => Application.dataPath + "/" + "RebindKeysData.json";
        
        [Inject] private GameInputManager _inputManager;
        private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;

        public OptionsData OptionsData;
        private bool isDataSaved;

        public Resolution[] Resolutions => Screen.resolutions;

        public event Action<float> OnSensitivityChanged;
        public event Action<bool> OnIsDataSavedChanged;

        public bool IsTutorShow => OptionsData.isTutorShow;

        public bool IsDataSaved
        {
            get => isDataSaved;
            set
            {
                if (value != isDataSaved) OnIsDataSavedChanged?.Invoke(value);
                isDataSaved = value;
            }
        }

        public void SetInitialSettings()
        {
            Debug.Log("OptionsManager SetInitialSettings");
            LoadFromJson();
            
            if (OptionsData == null)
            {
                OptionsData = GetDefaultOptionsData();
            }

            SetSettingsFromOptionsData(true);
            IsDataSaved = true;
        }

        public void ResetToDefault()
        {
            Debug.Log("OptionsManager ResetToDefault");
            OptionsData = GetDefaultOptionsData();

            SetSettingsFromOptionsData(false);
            ResetKeyBindings();
        }

        public void SetSettingsFromOptionsData(bool init)
        {
            SetLanguage(OptionsData.language, init);
            SetTutor(OptionsData.isTutorShow, init);
            SetResolution(OptionsData.resolutionIndex, init);
            SetFullScreen(OptionsData.isFullScreen, init);
            var audioValCopy = OptionsData.audioValues.ToDictionary(k => k.Key, v => v.Value);
            foreach (var pair in audioValCopy)
            {
                SetAudioVolume(pair.Key, pair.Value, init: init);
            }
            SetMouseSensitivity(OptionsData.mouseSensitivity, init);
        }

        private OptionsData GetDefaultOptionsData()
        {
            return new OptionsData()
                {
                    isFullScreen = 0,
                    resolutionIndex = 0,
                    language = "English",
                    mouseSensitivity = 15f,
                    audioValues = new()
                        {
                            {BroAudioType.All, 10f},
                            {BroAudioType.Music, 10f},
                            {BroAudioType.SFX, 10f}
                        },
                    isTutorShow = true
                };
        }

        //general game settings
        public void SetLanguage(string language, bool init=false)
        {
            if (!init)
            {
                OptionsData.language = language;
                IsDataSaved = false;
            }

            //
        }
        public void SetTutor(bool value, bool init = false)
        {
            if (!init)
            {
                OptionsData.isTutorShow = value;
                IsDataSaved = false;
            }
        }

        //audio settings 
        public void SetAudioVolume(BroAudioType audioType, float volumePercent, float fadeTime=0f, bool init=false)
        {
            if (!init) 
            {
                OptionsData.audioValues[audioType] = volumePercent;
                IsDataSaved = false;
            }

            BroAudio.SetVolume(audioType, volumePercent / 10f, fadeTime);
        }
        
        //video settings
        public void SetResolution(int resolutionIndex, bool init=false)
        {
            if (!init)
            {
                OptionsData.resolutionIndex = resolutionIndex;
                IsDataSaved = false;
            }
            
            resolutionIndex = Mathf.Clamp(resolutionIndex, 0, Resolutions.Length-1);
            Screen.SetResolution(Resolutions[resolutionIndex].width, 
                Resolutions[resolutionIndex].height, Screen.fullScreen);
        }
        
        public void SetFullScreen(OptionsScreenMode isFullScreen, bool init=false)
        {
            if (!init)
            {
                OptionsData.isFullScreen = isFullScreen;
                IsDataSaved = false;
            }

            switch (isFullScreen)
            {
                case OptionsScreenMode.Fullscreen:
                    {
                        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                    }
                    break;
                case OptionsScreenMode.Windowed:
                    {
                        Screen.fullScreenMode = FullScreenMode.Windowed;
                    }
                    break;
                case OptionsScreenMode.Borderless:
                    {
                        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    }
                    break;
            }
            
        }
        
        //controls
        public void SetMouseSensitivity(float valuePercent, bool init = false)
        {
            if (!init)
                OptionsData.mouseSensitivity = valuePercent;
            else
            {
                IsDataSaved = false;
            }
            OnSensitivityChanged?.Invoke(valuePercent);
        }

        public void StartRebindKey(InputAction inputAction, int targetIndex=-1, Action callback=null)
        {
            inputAction.Disable();

            Debug.Log($"[REBINDS] {inputAction.enabled}");

            if (targetIndex == -1)
            {
                _rebindingOperation = inputAction.PerformInteractiveRebinding();
            }
            else
            {
                _rebindingOperation = inputAction.PerformInteractiveRebinding(targetIndex);
            }

            _rebindingOperation
                .WithControlsExcluding("Mouse")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation => RebindComplete(inputAction, callback))
                .Start();
        }


        private void RebindComplete(InputAction inputAction, Action callback=null)
        {
            _rebindingOperation.Dispose();
            inputAction.Enable();
            IsDataSaved = false;
            if (callback != null)
                callback();
        }

        public Dictionary<InputMapType, List<InputAction>> GetAllActions()
        {
            var d = new Dictionary<InputMapType, List<InputAction>>()
            {
                { InputMapType.Gameplay, _inputManager.GameInput.Gameplay.Get().actions.ToList() },
                { InputMapType.UI, _inputManager.GameInput.UI.Get().actions.ToList() },
            };
            return d;
        }

        public void SaveKeyBindingsToJson()
        {
            var jsonData = _inputManager.GameInput.SaveBindingOverridesAsJson();
            File.WriteAllText(RebindFilePath, jsonData);
        }
        
        public void LoadKeyBindingsFromJson()
        {
            if (File.Exists(RebindFilePath))
            {
                var jsonData =  File.ReadAllText(RebindFilePath);
                _inputManager.GameInput.LoadBindingOverridesFromJson(jsonData);
            }
        }

        public void ResetKeyBindings()
        {
            _inputManager.GameInput.RemoveAllBindingOverrides();
        }


        //save

        public void SaveSettings() => SaveToJson();
        
        public void SaveToJson()
        {
            var jsonData = JsonUtility.ToJson(OptionsData);
            File.WriteAllText(FilePath, jsonData);
            SaveKeyBindingsToJson();
            IsDataSaved =  true;
        }

        public void LoadFromJson()
        {
            if (File.Exists(FilePath))
            {
                var jsonData =  File.ReadAllText(FilePath);
                OptionsData = JsonUtility.FromJson<OptionsData>(jsonData);
                LoadKeyBindingsFromJson();
            }
        }
    }
}