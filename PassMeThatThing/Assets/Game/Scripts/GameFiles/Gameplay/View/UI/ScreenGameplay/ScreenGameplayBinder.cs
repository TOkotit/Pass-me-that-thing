using System;
using System.Globalization;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.View.UI
{
    public class ScreenGameplayBinder : WindowBinder<ScreenGameplayViewModel>
    {
        // [SerializeField] private Button _btnGoToMainMenu;
        
        [SerializeField] private TextMeshProUGUI _healthText;
        
        [SerializeField] private TextMeshProUGUI _staminaText;

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

        private void Start()
        {
            // _btnGoToMainMenu?.onClick.AddListener(OnGoToMainMenuButtonClicked);
            
            ViewModel.InitHealthText(UpdateHealthText);
            ViewModel.RequestSubHealthText(UpdateHealthText);
            
            ViewModel.InitStaminaText(UpdateStaminaText);
            ViewModel.RequestSubStaminaText(UpdateStaminaText);
        }

        private void OnDestroy()
        {
            // _btnGoToMainMenu?.onClick.RemoveListener(OnGoToMainMenuButtonClicked);

            
            ViewModel.RequestUnsubHealthText(UpdateHealthText);
            
            ViewModel.RequestUnsubStaminaText(UpdateStaminaText);
        }

        private void UpdateHealthText(int newValue)
        {
            HealthText.text = newValue.ToString();
        }
        
        private void UpdateStaminaText(float newValue)
        {
            StaminaText.text = newValue.ToString(CultureInfo.InvariantCulture);
        }
        
        // private void OnGoToMainMenuButtonClicked()
        // {
        //     ViewModel.RequestGoToMainMenu();
        // }
        
    }
}