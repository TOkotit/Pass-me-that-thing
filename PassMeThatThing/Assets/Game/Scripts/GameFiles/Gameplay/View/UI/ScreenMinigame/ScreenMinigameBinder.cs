using System;
using System.Collections.Generic;
using DG.Tweening;
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

        private MinigameUI currentMinigame;
        
        // public Action OnCompleteMinigame;
        
        private void Start()
        {
            EnableMinigameByType(ViewModel.Parameters);
            
            closeBtn.onClick.AddListener(CloseMinigame);
        }

        private void OnDestroy()
        {
            closeBtn.onClick.RemoveListener(CloseMinigame);
        }
        
        public void EnableMinigameByType(MinigameParameters parameters)
        {
            foreach (var m in minigames)
            {
                m.miniGameUI.gameObject.SetActive(false);
            }
            
            currentMinigame = minigames.Find(x => x.eventType == parameters.eventType)
                .miniGameUI;
            currentMinigame.gameObject.SetActive(true);
            
            currentMinigame.gameObject.transform.DOScale(1f, 0.5f).From(0f).SetEase(Ease.OutBounce);
        }

        public void CompleteMinigame()
        {
            // Debug.Log("OnCompleteMinigame");
            currentMinigame.gameObject.transform.DOScale(0f, 0.5f).From(1f).SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    ViewModel.RequestCompleteMinigame();
                });
        }

        public void CloseMinigame()
        {
            ViewModel.RequestCloseMinigame();
        }
    }
}