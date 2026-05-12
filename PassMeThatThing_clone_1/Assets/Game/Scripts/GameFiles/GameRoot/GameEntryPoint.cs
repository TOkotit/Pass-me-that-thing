using System.Collections;
using DI;
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
        
        public void Start()
        {
            _gameManager.SetState(GameState.Booting);
            
            _coroutines.StartRoutine(InitialLoadRoutine());
        }
        
        private EntryPoint(
            ICoroutineRunner coroutines,
            GameManager gameManager,
            UIRootView uiRootPrefab)
        {
            _coroutines = coroutines;
            _gameManager = gameManager;
            _uiRoot = uiRootPrefab;
        }
        
        
        private IEnumerator InitialLoadRoutine()
        { 
            yield return _gameManager.LoadMainMenu();
        }
        
        
    }
}