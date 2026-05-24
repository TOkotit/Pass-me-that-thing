using System;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using Game.UI;
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
        
        [SerializeField] private TextMeshProUGUI _staminaText;
        
        [SerializeField] private TextMeshProUGUI _throwChargeText;
        
        [SerializeField] private Image[] _itemSlots;
        
        [SerializeField] private Image[] _itemImages;
        
        [SerializeField] private GameObject _gameEventsConatainer;
        
        [SerializeField] private GameEventUIElement _gameEventPrefab;
        
        [SerializeField] private GameObject _interactionText;
        
        private Color _imageColor = new Color(1f, 1f, 1f, 1f);
        private Color _noImageColor = new Color(1f, 1f, 1f, 0f);
        
        private int _activeSlotIndex;
        private Dictionary<int, GameEventUIElement> _gameEvents = new ();
        
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
            
            // ViewModel.InitHealthText(UpdateHealthText);
            // ViewModel.RequestSubHealthText(UpdateHealthText);
            //
            // ViewModel.InitStaminaText(UpdateStaminaText);
            // ViewModel.RequestSubStaminaText(UpdateStaminaText);

            SetActiveItemSlot(_activeSlotIndex);
            
            ViewModel.RequestSubActiveSlot(SetActiveItemSlot);
            
            ViewModel.InitImage(SetItemImageSprite);
            ViewModel.RequestSubImage(SetItemImageSprite);
            ViewModel.RequestSubInteractionText(ChangeInteractionTextVisibility);
            
            ViewModel.RequestSubGameEvent(AddGameEvent, UpdateGameEvent, RemoveGameEvent);
            
            ViewModel.RequestSubThrowCharge(UpdateThrowChargeText);
        }

        private void OnDestroy()
        {
            // _btnGoToMainMenu?.onClick.RemoveListener(OnGoToMainMenuButtonClicked);

            
            // ViewModel.RequestUnsubHealthText(UpdateHealthText);
            //
            // ViewModel.RequestUnsubStaminaText(UpdateStaminaText);
            
            ViewModel.RequestUnsubActiveSlot(SetActiveItemSlot);
            ViewModel.RequestUnsubInteractionText(ChangeInteractionTextVisibility);
            ViewModel.RequestUnsubThrowCharge(UpdateThrowChargeText);
            
            ViewModel.RequestUnsubGameEvent(AddGameEvent, UpdateGameEvent, RemoveGameEvent);
            
            ViewModel.RequestUnsub();
        }

        private void UpdateHealthText(int newValue)
        {
            HealthText.text = newValue.ToString();
        }
        
        private void UpdateStaminaText(float newValue)
        {
            StaminaText.text = newValue.ToString(CultureInfo.InvariantCulture);
        }

        private void UpdateThrowChargeText(int newValue)
        {
            ThrowChargeText.text = newValue == 0 ? "" : $"{newValue.ToString()}%";
        }
        
        private void ChangeInteractionTextVisibility(bool isVisible)
        {
            _interactionText.SetActive(isVisible);
        }

        private void SetActiveItemSlot(int index)
        {
            _itemSlots[_activeSlotIndex].color = noSelectionColor;
            _itemSlots[_activeSlotIndex].transform.DOScale(1f, 0.3f);
            
            _activeSlotIndex = index;

            _itemSlots[_activeSlotIndex].color = selectedColor;
            _itemSlots[_activeSlotIndex].transform.DOScale(1.2f, 0.3f);
        }

        private void SetItemImageSprite(int index, Sprite sprite)
        {
            _itemImages[index].color = sprite != null 
                ? _imageColor : _noImageColor;
            _itemImages[index].sprite = sprite;
        }

        private void AddGameEvent(int index, Sprite icon, int roomNumber)
        {
            var gameEvent = Instantiate(_gameEventPrefab, _gameEventsConatainer.transform);
            
            gameEvent.Icon.sprite = icon;
            _gameEvents[index].Text.text = $"{roomNumber}";
            
            _gameEvents[index] = gameEvent;
        }
        
        private void UpdateGameEvent(int index, Sprite icon, int roomNumber)
        {
            _gameEvents[index].Icon.sprite = icon;
            _gameEvents[index].Text.text = $"{roomNumber}";
        }

        private void RemoveGameEvent(int index)
        {
            Destroy(_gameEvents[index].gameObject);
            _gameEvents.Remove(index);
        }
        
        // private void OnGoToMainMenuButtonClicked()
        // {
        //     ViewModel.RequestGoToMainMenu();
        // }
        
    }
}