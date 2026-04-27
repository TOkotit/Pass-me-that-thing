using System;
using Game.UI;
using MainCharacter;
using R3;
using Systems;
using UnityEngine;
using Utils;
using VContainer;

namespace Game.Gameplay.View.UI
{
    public class ScreenGameplayViewModel : WindowViewModel
    {
        private readonly GameplayUIManager _uiManager;
        private readonly GameManager _gameManager;
        private readonly ICoroutineRunner _coroutines;
        private readonly GameInputManager _gameInputManager;
        
        private readonly MainCharacterModel  _mainCharacter;
        
        public override string Id => "ScreenGameplay";

        public ScreenGameplayViewModel(GameplayUIManager uiManager, IObjectResolver container)
        {
            _uiManager = uiManager;
            _gameManager =  container.Resolve<GameManager>();
            _coroutines = container.Resolve<ICoroutineRunner>();
            _mainCharacter = container.Resolve<MainCharacterModel>();
            _gameInputManager = container.Resolve<GameInputManager>();
        }

        public void InitHealthText(Action<int> f)
        {
            Debug.Log("inithealthText");
            f(_mainCharacter.Health.CurrentHealth);
        }
        
        public void RequestSubHealthText(Action<int> f)
        {
            Debug.Log($"RequestSubText {_mainCharacter.Health == null}");
            _mainCharacter.Health.OnHealthChanged += f;
        }
        
        public void RequestUnsubHealthText(Action<int> f)
        {
            Debug.Log($"RequestUnsubText {_mainCharacter.Health == null}");
            _mainCharacter.Health.OnHealthChanged -= f;
        }
        
        public void InitStaminaText(Action<float> f)
        {
            f(_mainCharacter.Stamina.CurrentStamina);
        }
        
        public void RequestSubStaminaText(Action<float> f)
        {
            _mainCharacter.Stamina.OnStaminaChanged += f;
        }
        
        public void RequestUnsubStaminaText(Action<float> f)
        {
            _mainCharacter.Stamina.OnStaminaChanged -= f;
        }
        
        // public void RequestGoToMainMenu()
        // {
        //     _coroutines.StartRoutine(_gameManager.LoadMainMenu());
        // }
    }
}