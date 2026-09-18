using Mirror;
using Systems;
using UIRoot;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Assets.Game.Scripts.GameFiles.GameRoot
{
    public class NetworkManagerScopeEntryPoint : IStartable
    {
        private readonly GameManager _gameManager;
        private readonly UIRootView _rootView;
        private readonly CustomNetworkRoomManager _roomManager;
        readonly UIRootView _uiRoot;

        public NetworkManagerScopeEntryPoint(
            IObjectResolver resolver,
            UIRootView uiRoot,
            NetworkManager roomManager)
        {
            _gameManager = resolver.Resolve<GameManager>();
            _rootView = resolver.Resolve<UIRootView>();
            _uiRoot = uiRoot;

            if (roomManager is CustomNetworkRoomManager manager)
                _roomManager = manager;
        }

        public void Start()
        {
            Debug.Log("NetworkManagerScopeEntryPoint.Start");

            HandleLoadingScreen();
        }

        private void HandleLoadingScreen()
        {
            _roomManager.OnClientSceneLoadStateChanged += _uiRoot.SetLoadingScreen;
            _roomManager.OnServerSceneLoadStateChanged += _uiRoot.SetLoadingScreen;
        }
    }
}
