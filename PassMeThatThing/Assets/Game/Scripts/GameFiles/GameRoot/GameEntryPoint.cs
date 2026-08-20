using System.Collections;
using Assets.Game.Scripts.GameFiles.GameRoot;
using DI;
using Game.Scripts.Systems;
using Mirror;
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
        readonly UIRootView _uiRoot;
        readonly GameManager _gameManager;
        private readonly OptionsManager _optionsManager;
        private readonly CustomNetworkRoomManager _roomManager;
        
        private EntryPoint(
            ICoroutineRunner coroutines,
            GameManager gameManager,
            UIRootView uiRoot,
            NetworkManager roomManager,
            OptionsManager optionsManager)
        {
            _coroutines = coroutines;
            _gameManager = gameManager;
            _uiRoot = uiRoot;
            _optionsManager = optionsManager;

            if (roomManager is CustomNetworkRoomManager manager)
                _roomManager = manager;
        }
        
        public void Start()
        {
            _optionsManager.SetInitialSettings();

            HandleLoadingScreen();
        }
        
        private void HandleLoadingScreen()
        {
            _roomManager.OnClientSceneLoadStateChanged += _uiRoot.SetLoadingScreen;
            _roomManager.OnServerSceneLoadStateChanged += _uiRoot.SetLoadingScreen;
        }
    }
}