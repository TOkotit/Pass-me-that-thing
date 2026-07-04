using System.IO;
using Ami.BroAudio;
using UnityEngine;

namespace Game.Scripts.Systems
{
    public class OptionsManager : IJsonSaveable
    {
        private string FilePath => Application.dataPath + "/" + "OptionsData.json";
        
        public OptionsData OptionsData;
        public bool IsDataSaved;
        
        public Resolution[] Resolutions => Screen.resolutions;


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
        
        public void SaveSettings() => SaveToJson();
        
        //save
        public void SaveToJson()
        {
            var jsonData = JsonUtility.ToJson(OptionsData);
            File.WriteAllText(FilePath, jsonData);
            IsDataSaved =  true;
        }

        public void LoadFromJson()
        {
            if (File.Exists(FilePath))
            {
                var jsonData =  File.ReadAllText(FilePath);
                OptionsData = JsonUtility.FromJson<OptionsData>(jsonData);
            }
        }
    }
}