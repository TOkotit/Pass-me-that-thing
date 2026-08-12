using Assets.Game.Scripts.GameFiles.Gameplay.View.UI.UIWorld;
using Assets.Game.Scripts.GameFiles.UIWorld;
using Game.Gameplay.View.UI;
using Game.Scripts.GameFiles.GameRandomEvents;
using Game.Scripts.GameFiles.GlobalStageManager;
using Mirror;
using R3;
using Systems;
using UIRoot;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Gameplay.Root
{
    public class GameplayEntryPoint : IStartable
    {
        private GameplayUIRootBinder _sceneUIRootPrefab;
        private WorldUIRootBinder _sceneWorldUIRootPrefab;

        [Inject] readonly GameManager _gameManager;
        [Inject] IObjectResolver resolver;
        
        public GameplayEntryPoint()
        {
            Debug.Log("GameplayEntryPoint");
            _sceneUIRootPrefab = Resources.Load<GameplayUIRootBinder>("Prefabs/UI/Root/GameplayUI");
            _sceneWorldUIRootPrefab = Resources.Load<WorldUIRootBinder>("Prefabs/UI/Root/WorldUI");


        }
        
        public void Start()
        {
            Debug.Log("GameplayEntryPoint.Start");
            
            InitUI();
            
            // if (NetworkServer.active)
            // {
            //     SpawnNetworkManagers();
            // }
            _gameManager.SetState(GameState.Gameplay);
        }
        

        private void InitUI()
        {
            var uiRoot = resolver.Resolve<UIRootView>();

            var uiSceneRootBinder = resolver.Instantiate(_sceneUIRootPrefab);
            uiRoot.AttachSceneUI(uiSceneRootBinder.gameObject);

            var uiSceneRootViewModel = resolver.Resolve<GameplayUIRootViewModel>();
            uiSceneRootBinder.Bind(uiSceneRootViewModel);

            //world
            var uiWorldSceneRootBinder = resolver.Instantiate(_sceneWorldUIRootPrefab);
            uiRoot.AttachSceneUI(uiWorldSceneRootBinder.gameObject);

            var uiWorldSceneRootViewModel = resolver.Resolve<WorldUIRootViewModel>();
            uiWorldSceneRootBinder.Bind(uiWorldSceneRootViewModel);


            var uiManager = resolver.Resolve<GameplayUIManager>();
            uiManager.OpenScreenGameplay();
        }

        private void SpawnNetworkManagers()
        {
            Debug.Log("[Server] Начало спавна глобальных сетевых менеджеров...");

            var eventManagerPrefab = resolver.Resolve<GameRandomEventManager>();
            var globalStageManagerPrefab = resolver.Resolve<GlobalStageManager>();

            var eventManagerInstance = Object.Instantiate(eventManagerPrefab);
            Object.DontDestroyOnLoad(eventManagerInstance.gameObject);
            
            resolver.Inject(eventManagerInstance); 
            NetworkServer.Spawn(eventManagerInstance.gameObject); 

            var stageManagerInstance = Object.Instantiate(globalStageManagerPrefab);
            Object.DontDestroyOnLoad(stageManagerInstance.gameObject);
            
            resolver.Inject(stageManagerInstance); 
            NetworkServer.Spawn(stageManagerInstance.gameObject);
            
        }
    }
}