using Game.Gameplay.View.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MainCharacter
{
    public class MainCharacterInitializer : IStartable
    {
        private readonly GameplayUIManager _gameplayUIManager;

        public MainCharacterInitializer(GameplayUIManager gameplayUIManager)
        {
            _gameplayUIManager = gameplayUIManager;
        }

        public void Start()
        {
            Debug.Log("MainCharacterInitializer.Start()");
            _gameplayUIManager.OpenScreenGameplay(); 
        }
    }
}