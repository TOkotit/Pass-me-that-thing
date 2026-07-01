using System;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using FishNet.Object.Synchronizing;
using Game.Scripts.Enums;
using Game.Scripts.GameFiles.Events;
using Game.UI;
using Mirror;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.View.UI
{
    public class ScreenGameplayBinder : WindowBinder<ScreenGameplayViewModel>
    {
        [SerializeField] private Color selectedColor = Color.cornflowerBlue;
        [SerializeField] private Color noSelectionColor = Color.dimGray;
        
        // [SerializeField] private Button _btnGoToMainMenu;
        
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private Image healthImage;
        
        [SerializeField] private Image deathImage;
        
        [SerializeField] private TextMeshProUGUI _staminaText;
        
        [SerializeField] private TextMeshProUGUI _throwChargeText;
        
        [SerializeField] private Image[] _itemSlots;
        [SerializeField] private Image[] _itemImages;
        [SerializeField] private GameObject _gameEventsConatainer;
        [SerializeField] private GameEventUIElement _gameEventPrefab;
        [SerializeField] private TextMeshProUGUI gameGlobalState;
        [SerializeField] private TextMeshProUGUI gameGlobalStateTimer;
        [SerializeField] private GameObject _interactionText;
        
        private Color _imageColor = new Color(1f, 1f, 1f, 1f);
        private Color _noImageColor = new Color(1f, 1f, 1f, 0f);
        
        private int _activeSlotIndex = -1;
        private Dictionary<int, GameEventUIElement> _gameEvents = new ();
        
        private GameEventsDatabase _gameEventsDatabase;

        public TextMeshProUGUI HealthText
        {
            get => _healthText;
            set => _healthText = value;
        }

        public TextMeshProUGUI StaminaText
        {
            get => _staminaText;
            set => _staminaText = value;
        }

        public TextMeshProUGUI ThrowChargeText
        {
            get => _throwChargeText;
            set => _throwChargeText = value;
        }

        private void Start()
        {
            // _btnGoToMainMenu?.onClick.AddListener(OnGoToMainMenuButtonClicked);
            ViewModel.InitHealthUI(UpdateCurrHealthUI);
            ViewModel.RequestSubHealthUI(UpdateCurrHealthUI);
            
            ViewModel.RequestSubDeathUI(UpdateDeathUI);

            ViewModel.InitActiveSlot(SetActiveItemSlot);
            ViewModel.RequestSubActiveSlot(SetActiveItemSlot);
            
            ViewModel.InitImage(SetItemImageSprite);
            
            ViewModel.RequestSubImage(SetItemImageSprite);
            ViewModel.RequestSubInteractionText(ChangeInteractionTextVisibility);

            ViewModel.InitGameEvent(Clear, AddGameEvent);
            ViewModel.InitGameEventToClient(SetupEventDatabase, ReceiveEvents);
            
            ViewModel.RequestSubGameEvent(AddGameEvent, UpdateGameEvent, RemoveGameEvent);
            
            ViewModel.RequestSubThrowCharge(UpdateThrowChargeText);
            ViewModel.RequestSubGlobalState(UpdateGameGlobalState);
            ViewModel.RequestSubGlobalStateTimer(UpdateGameGlobalStateTimer);
            
            // _gameEventsConatainer.transform.DOScale(1f, 0.5f).From(0f).SetEase(Ease.OutBounce);
        }

        private void OnDestroy()
        {
            // _btnGoToMainMenu?.onClick.RemoveListener(OnGoToMainMenuButtonClicked);
            ViewModel.RequestUnsubHealthUI(UpdateCurrHealthUI);
            // ViewModel.RequestUnsubStaminaText(UpdateStaminaText);
            
            ViewModel.RequestUnsubDeathUI(UpdateDeathUI);

            ViewModel.UnInitGameEventToClient(ReceiveEvents);
            
            ViewModel.RequestUnsubActiveSlot(SetActiveItemSlot);
            ViewModel.RequestUnsubInteractionText(ChangeInteractionTextVisibility);
            ViewModel.RequestUnsubThrowCharge(UpdateThrowChargeText);
            ViewModel.RequestUnsubGlobalState(UpdateGameGlobalState);
            ViewModel.RequestUnsubGameEvent(AddGameEvent, UpdateGameEvent, RemoveGameEvent);
            ViewModel.RequestUnsubGlobalStateTimer(UpdateGameGlobalStateTimer);
            ViewModel.RequestUnsub();
        }
        
        private void UpdateCurrHealthUI(int newValue, int maxHealth)
        {
            if (maxHealth <= 0) return;
            
            Debug.Log($"[UI] new hp {newValue}");
            healthImage.color = new Color(1f, 1f, 1f, (1 - (float)newValue / maxHealth) * 0.4f);
            HealthText.text = newValue.ToString();
        }
        
        private void UpdateDeathUI(bool isDead)
        {
            Debug.Log($"[UI] death {isDead}");
            if (isDead)
            {
                deathImage.DOFade(0.8f, 0.4f).From(0f);
            }
            else
            {
                deathImage.DOFade(0f, 0.2f).From(0.8f);
            }
        }

        private void UpdateThrowChargeText(int newValue)
        {
            ThrowChargeText.text = newValue == 0 ? "" : $"{newValue.ToString()}%";
        }
        
        private void ChangeInteractionTextVisibility(bool isVisible)
        {
            _interactionText.SetActive(isVisible);
        }

        private void UpdateGameGlobalState(GlobalStagesType newValue)
        {
            gameGlobalState.text = newValue switch
            {
                GlobalStagesType.Fight => "Фаза обороны",
                GlobalStagesType.Preparation => "Фаза подготовки",
                _ => "Неизвестная фаза"
            };
        }
        
        private void UpdateGameGlobalStateTimer(float remainingSeconds)
        {
            if (gameGlobalStateTimer == null)
            {
                Debug.LogError($"[ScreenGameplayBinder] Поле 'gameGlobalStateTimer' (TextMeshProUGUI) НЕ НАЗНАЧЕНО в инспекторе на объекте {gameObject.name}!", this);
                return;
            }
            
            var minutes = Mathf.FloorToInt(remainingSeconds / 60f);
            var seconds = Mathf.FloorToInt(remainingSeconds % 60f);
            
            gameGlobalStateTimer.text = $"{minutes:00}:{seconds:00}";
        }

        private void SetActiveItemSlot(int index)
        {
            if (_activeSlotIndex != -1)
            {
                _itemSlots[_activeSlotIndex].color = noSelectionColor;
                _itemSlots[_activeSlotIndex].transform.DOScale(1f, 0.3f);
            }
            
            _activeSlotIndex = index;
            
            if (_activeSlotIndex != -1)
            {
                _itemSlots[_activeSlotIndex].color = selectedColor;
                _itemSlots[_activeSlotIndex].transform.DOScale(1.2f, 0.3f);
            }
        }

        private void SetItemImageSprite(int index, Sprite sprite)
        {
            // if (sprite == null)
            // {
            //     _itemImages[index].color = _noImageColor;
            //     
            // }
            
            _itemImages[index].color = sprite != null 
                ? _imageColor : _noImageColor;
            
            _itemImages[index].sprite = sprite;
        }

        private void ReceiveEvents(SyncDictionary<int, BaseGameEvent> dict)
        {
            foreach (var i in dict)
            {
                var e = _gameEventsDatabase.GetEvent(i.Value.eventType);
                AddGameEvent(i.Value.EventId, e.EventImage, i.Value.EventId);
            }
        }

        private void SetupEventDatabase(GameEventsDatabase gameEventsDatabase)
        {
            _gameEventsDatabase = gameEventsDatabase;
        }

        private void Clear()
        {
            _gameEvents.Clear();
        }
        
        private void AddGameEvent(int eventId, Sprite icon, int roomNumber)
        {
            if (_gameEvents.ContainsKey(eventId)) return;
            
            var gameEvent = Instantiate(_gameEventPrefab, _gameEventsConatainer.transform, false);
            
            gameEvent.Icon.sprite = icon;
            gameEvent.Text.text = $"R-{roomNumber}";
            
            _gameEvents.Add(eventId, gameEvent);
            
            if (_gameEventsConatainer.TryGetComponent<RectTransform>(out var containerRect))
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
            }
            
            gameEvent.transform.DOScale(1f, 0.2f).From(0f).SetEase(Ease.InOutBack);
        }
        
        private void UpdateGameEvent(int eventId, Sprite icon, int roomNumber)
        {
            if (_gameEvents.TryGetValue(eventId, out var gameEvent) && gameEvent != null)
            {
                gameEvent.Icon.sprite = icon;
                gameEvent.Text.text = $"R-{roomNumber}";
            }
        }

        private void RemoveGameEvent(int eventId)
        {
            if (_gameEvents.TryGetValue(eventId, out var gameEvent) && gameEvent != null)
                gameEvent.transform.DOScale(0f, 0.2f)
                    .From(1f).SetEase(Ease.InOutBack)
                    .OnComplete(() =>
                    {
                        Destroy(gameEvent.gameObject);
                        _gameEvents.Remove(eventId);
                    });
        }
        
        // private void OnGoToMainMenuButtonClicked()
        // {
        //     ViewModel.RequestGoToMainMenu();
        // }
        
    }
}