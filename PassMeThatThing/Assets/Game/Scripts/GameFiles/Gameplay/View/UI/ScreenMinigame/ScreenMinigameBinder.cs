using System;
using Game.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.View.UI.ScreenMinigame
{
    public class ScreenMinigameBinder : WindowBinder<ScreenMinigameViewModel>
    {
        [SerializeField] private Button closeBtn;
        
        
        private void Start()
        {
            
        }

        private void OnDestroy()
        {
            
        }
    }
}