using System.IO;
using Ami.BroAudio;
using UnityEngine;

namespace Game.Scripts.Systems
{
    public class OptionsManager : IJsonSaveable
    {
        private const string FilePath = "OptionsData.json";
        
        public OptionsData OptionsData;
        public bool IsDataSaved;
        
        public Resolution[] Resolutions => Screen.resolutions;


        public void SetInitialSettings()
        {
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
            
            SetLanguage(OptionsData.language);
            SetResolution(OptionsData.resolutionIndex);
            SetFullScreen(OptionsData.isFullScreen);
            foreach (var pair in OptionsData.audioValues)
            {
                SetAudioVolume(pair.Key, pair.Value);
            }
        }
        
        //general game settings
        public void SetLanguage(string language)
        {
            OptionsData.language = language;
        }
        
        //audio settings
        public void SetAudioVolume(BroAudioType audioType, float volume, float fadeTime=0f)
        {
            OptionsData.audioValues[audioType] = volume;
            BroAudio.SetVolume(audioType, volume, fadeTime);
        }
        
        //video settings
        public void SetResolution(int resolutionIndex)
        {
            OptionsData.resolutionIndex = resolutionIndex;
            Screen.SetResolution(Resolutions[resolutionIndex].width, 
                Resolutions[resolutionIndex].height, Screen.fullScreen);
        }
        
        public void SetFullScreen(bool isFullScreen)
        {
            OptionsData.isFullScreen = isFullScreen;
            Screen.fullScreen = isFullScreen;
        }
        
        public void SaveSettings() => SaveToJson();
        
        //save
        public void SaveToJson()
        {
            var jsonData = JsonUtility.ToJson(OptionsData);
            File.WriteAllText(Application.dataPath + "/" + FilePath, jsonData);
            IsDataSaved =  true;
        }

        public void LoadFromJson()
        {
            var jsonData =  File.ReadAllText(Application.dataPath + "/" + FilePath);
            OptionsData = JsonUtility.FromJson<OptionsData>(jsonData);
        }
    }
}