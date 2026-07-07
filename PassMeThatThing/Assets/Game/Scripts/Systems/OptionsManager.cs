using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ami.BroAudio;
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
        
        public OptionsData OptionsData;
        public bool IsDataSaved;
        
        public Resolution[] Resolutions => Screen.resolutions;

        [Inject] private GameInputManager _inputManager;
        private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;
        
        public void SetInitialSettings()
        {
            Debug.Log("OptionsManager SetInitialSettings");
            LoadFromJson();
            
            if (OptionsData == null)
            {
                OptionsData = new OptionsData()
                {
                    isFullScreen = false,
                    resolutionIndex = 0,
                    language = "English",
                    audioValues = new ()
                    {
                        {BroAudioType.All, 0.5f},
                        {BroAudioType.Music, 0.5f},
                        {BroAudioType.SFX, 0.5f}
                    }
                };
                
            }
            
            SetLanguage(OptionsData.language, true);
            SetResolution(OptionsData.resolutionIndex, true);
            SetFullScreen(OptionsData.isFullScreen, true);
            foreach (var pair in OptionsData.audioValues)
            {
                SetAudioVolume(pair.Key, pair.Value, init: true);
            }
        }
        
        //general game settings
        public void SetLanguage(string language, bool init=false)
        {
            if (!init)
                OptionsData.language = language;
            else
            {
                IsDataSaved = false;
            }
            //
        }
        
        //audio settings
        public void SetAudioVolume(BroAudioType audioType, float volume, float fadeTime=0f, bool init=false)
        {
            if (!init)
                OptionsData.audioValues[audioType] = volume;
            else
            {
                IsDataSaved = false;
            }
            BroAudio.SetVolume(audioType, volume, fadeTime);
        }
        
        //video settings
        public void SetResolution(int resolutionIndex, bool init=false)
        {
            if (!init)
                OptionsData.resolutionIndex = resolutionIndex;
            else
            {
                IsDataSaved = false;
            }
            resolutionIndex = Mathf.Clamp(resolutionIndex, 0, Resolutions.Length-1);
            Screen.SetResolution(Resolutions[resolutionIndex].width, 
                Resolutions[resolutionIndex].height, Screen.fullScreen);
        }
        
        public void SetFullScreen(bool isFullScreen, bool init=false)
        {
            if (!init)
                OptionsData.isFullScreen = isFullScreen;
            else
            {
                IsDataSaved = false;
            }
            Screen.fullScreen = isFullScreen;
        }
        
        //keyboard

        public void StartRebindKey(InputAction inputAction, int targetIndex=-1, Action callback=null)
        {
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
                .OnComplete(operation => RebindComplete(callback))
                .Start();
        }
        
        private void RebindComplete(Action callback=null)
        {
            _rebindingOperation.Dispose();
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