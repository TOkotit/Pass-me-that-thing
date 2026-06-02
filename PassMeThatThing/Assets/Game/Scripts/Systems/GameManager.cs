using System;
using System.Collections;
using DI;
using UIRoot;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Systems
{
    public class GameManager
    {
        private readonly UIRootView _uiRootView;
        private readonly GameInputManager _gameInputManager;

        public GameManager(UIRootView uiRootView, GameInputManager gameInputManager)
        {
            _uiRootView = uiRootView;
            _gameInputManager = gameInputManager;
            CurrentState = GameState.Booting;
            OnStateChange = new UnityEvent<GameState>();
            Debug.Log("GameManager: Инициализирован.");
        }
        
        public GameState CurrentState { get; private set; }
        
        public UnityEvent<GameState> OnStateChange { get; }
        
        public void SetState(GameState newState)
        {
            Debug.Log("GameManager.SetState");
            if (CurrentState == newState) return;

            CurrentState = newState;
            Debug.Log($"Game State changed to: {newState}");

            OnStateChange?.Invoke(newState);

            Time.timeScale = newState switch
            {
                GameState.Gameplay => 1f,
                GameState.Paused => 0f,
                GameState.GameOver => 0f,
                _ => Time.timeScale
            };
        }
        
        
        
        // public IEnumerator LoadMainMenu()
        // {
        //     _gameInputManager.GameInput.Gameplay.Disable();
        //     yield return LoadScene(SceneType.MainMenu);
        //     SetState(GameState.Menu);
        // }
        //
        // public IEnumerator LoadGameplay()
        // {
        //     _gameInputManager.GameInput.Gameplay.Enable();
        //     yield return LoadScene(SceneType.Gameplay);
        //     SetState(GameState.Gameplay);
        // }
        //
        //
        // private IEnumerator LoadScene(SceneType sceneType)
        // {
        //     _uiRootView.ShowLoadingScreen();
        //     
        //     var sceneName = sceneType.ToString();
        //     var operation = SceneManager.LoadSceneAsync(sceneName);
        //     
        //     while (!operation.isDone)
        //     {
        //         yield return null;
        //     }
        //     
        //     _uiRootView.HideLoadingScreen();
        // }
    }
}