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

        [SerializeField] private Image successImage;
        
        [SerializeField] private List<EventTypeToMinigameUI> minigames;

        private MinigameUI currentMinigame;
        
        
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
            var anim =  DOTween.Sequence();
            
            successImage.gameObject.SetActive(true);
            
            anim.Append(successImage.DOFade(1f, 0.3f).From(0f))
                .Join(successImage.rectTransform.DOScale(1f, 0.3f).From(0f).SetEase(Ease.OutBounce))
                .Append(currentMinigame.gameObject.transform.DOScale(0f, 0.5f).From(1f).SetEase(Ease.OutBounce))
                .OnComplete(() =>
                {
                    ViewModel.RequestCompleteMinigame();
                });
        }

        public void CloseMinigame()
        {
            var anim =  DOTween.Sequence();

            anim.Append(currentMinigame.gameObject.transform.DOScale(0f, 0.5f).From(1f).SetEase(Ease.OutBounce))
                .OnComplete(() =>
                {
                    ViewModel.RequestCloseMinigame();
                });
        }
    }
}