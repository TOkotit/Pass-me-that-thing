using Game.Gameplay.View.UI;
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
        
        [Inject] readonly GameManager _gameManager;
        [Inject] IObjectResolver resolver;
        
        public GameplayEntryPoint()
        {
            Debug.Log("GameplayEntryPoint");
            _sceneUIRootPrefab = Resources.Load<GameplayUIRootBinder>("Prefabs/UI/Root/GameplayUI");
            
            
        }
        
        public void Start()
        {
            Debug.Log("GameplayEntryPoint.Start");
            
            InitUI();
            _gameManager.SetState(GameState.Gameplay);
        }
        

        private void InitUI()
        {
            // Создали UI для сцены (это было)
            var uiRoot = resolver.Resolve<UIRootView>();
            var uiSceneRootBinder = resolver.Instantiate(_sceneUIRootPrefab);
            uiRoot.AttachSceneUI(uiSceneRootBinder.gameObject);
            
            // Запрашиваем рутовую вью модель и пихаем ее в баиндер, который создали
            var uiSceneRootViewModel = resolver.Resolve<GameplayUIRootViewModel>();
            uiSceneRootBinder.Bind(uiSceneRootViewModel);
            // можно открывать окошки
        }

        
    }
}