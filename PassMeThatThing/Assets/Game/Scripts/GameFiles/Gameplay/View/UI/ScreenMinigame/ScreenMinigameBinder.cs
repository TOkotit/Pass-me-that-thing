using System;
using System.Collections.Generic;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Events;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.View.UI.ScreenMinigame
{
    [Serializable]
    public class EventTypeToMinigameUI
    {
        public GameEventsType eventType;
        public MinigameUI miniGameUI;
    }
    
    public class ScreenMinigameBinder : WindowBinder<ScreenMinigameViewModel>
    {
        [SerializeField] private Button closeBtn;
        
        [SerializeField] private TextMeshProUGUI timeLeft;
        
        [SerializeField] private List<EventTypeToMinigameUI> minigames;


        public Action OnCompleteMinigame;
        
        private void Start()
        {
            EnableMinigameByType(ViewModel.Parameters);
        }

        private void OnDestroy()
        {
            // ViewModel.RequestUnsubMinigameParameters(ToggleMinigameByType);
        }
        
        public void EnableMinigameByType(MinigameParameters parameters)
        {
            foreach (var m in minigames)
            {
                m.miniGameUI.gameObject.SetActive(m.eventType == parameters.eventType);
            }
        }

        public void CompleteMinigame()
        {
            // Debug.Log("OnCompleteMinigame");
            ViewModel.RequestCompleteMinigame();
        }
    }
}