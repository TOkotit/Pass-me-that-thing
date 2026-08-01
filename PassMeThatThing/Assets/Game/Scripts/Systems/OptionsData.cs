using System;
using Ami.BroAudio;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Game.Scripts.Systems
{
    [Serializable]
    public class OptionsData
    {
        public bool isFullScreen;
        public int resolutionIndex;

        public float mouseSensitivity; //% from 0 to 100

        public string language;
        
        public SerializedDictionary<BroAudioType, float> audioValues; //% from 0 to 100

    }
}