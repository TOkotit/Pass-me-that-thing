using Game.MainMenu.View.UI;
using Systems;
using UIRoot;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Scripts.GameFiles.Lobby.Root
{
    public class LobbyEntryPoint : IStartable
    {
        private LobbyUIRootBinder _sceneUIRootPrefab;
    
        readonly GameManager _gameManager;
        
        public void Start()
        {
            Debug.Log("LobbyEntryPoint.Start");
        }
        
        public LobbyEntryPoint(IObjectResolver resolver)
        {
            Debug.Log("LobbyEntryPoint");
            _sceneUIRootPrefab = Resources.Load<LobbyUIRootBinder>("Prefabs/UI/Root/LobbyUI");
            _gameManager =  resolver.Resolve<GameManager>();
        
            InitUI(resolver);
        
        }
    
        private void InitUI(IObjectResolver resolver)
        {
            Debug.Log($"InitUI Lobby");
        
            // Создали UI для сцены (это было)
            var uiRoot = resolver.Resolve<UIRootView>();
            var uiSceneRootBinder = resolver
                .Instantiate<LobbyUIRootBinder>(_sceneUIRootPrefab);
            uiRoot.AttachSceneUI(uiSceneRootBinder.gameObject);
        
            // Запрашиваем рутовую вью модель и пихаем ее в баиндер, который создали
            var uiSceneRootViewModel = resolver.Resolve<LobbyUIRootViewModel>();
            uiSceneRootBinder.Bind(uiSceneRootViewModel);
        
            // Открываем окно
            var uiManager = resolver.Resolve<LobbyUIManager>();
            uiManager.OpenScreenLobby();
        }
    }
}