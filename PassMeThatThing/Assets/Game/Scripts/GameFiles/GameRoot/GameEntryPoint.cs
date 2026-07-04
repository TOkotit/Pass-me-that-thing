using System.Collections;
using DI;
using Game.Scripts.Systems;
using Systems;
using UIRoot;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using VContainer;
using VContainer.Unity;

namespace Root
{
    public class EntryPoint : IStartable
    {
        private readonly ICoroutineRunner _coroutines;
        readonly IUIRootView _uiRoot;
        readonly GameManager _gameManager;
        private readonly OptionsManager _optionsManager;
        
        private EntryPoint(
            ICoroutineRunner coroutines,
            GameManager gameManager,
            UIRootView uiRootPrefab,
            OptionsManager optionsManager)
        {
            _coroutines = coroutines;
            _gameManager = gameManager;
            _uiRoot = uiRootPrefab;
            _optionsManager = optionsManager;
        }
        
        public void Start()
        {
            // _gameManager.SetState(GameState.Booting);
            _optionsManager.SetInitialSettings();
            
        }
        
        
        // private IEnumerator InitialLoadRoutine()
        // { 
        //     yield return _gameManager.LoadMainMenu();
        // }
        
        
    }
}