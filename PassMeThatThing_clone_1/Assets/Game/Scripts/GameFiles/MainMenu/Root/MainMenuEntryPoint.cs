using System.Collections;
using Game;
using Game.Gameplay.View.UI;
using Game.MainMenu.View.UI;
using R3;
using Root;
using Systems;
using UIRoot;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Utils;
using VContainer;
using VContainer.Unity;

public class MainMenuEntryPoint : IStartable
{
    private MainMenuUIRootBinder _sceneUIRootPrefab;
    
    readonly GameManager _gameManager;
    
    public void Start()
    {
        Debug.Log("MainMenuEntryPoint.Start");
        
        _gameManager.SetState(GameState.Menu);
    }

    public MainMenuEntryPoint(IObjectResolver resolver)
    {
        Debug.Log("MainMenuEntryPoint");
        _sceneUIRootPrefab = Resources.Load<MainMenuUIRootBinder>("Prefabs/UI/Root/MainMenuUI");
        _gameManager =  resolver.Resolve<GameManager>();
        
        InitUI(resolver);
        
    }
    
    private void InitUI(IObjectResolver resolver)
    {
        Debug.Log($"InitUI MainMenu");
        
        // Создали UI для сцены (это было)
        var uiRoot = resolver.Resolve<UIRootView>();
        var uiSceneRootBinder = resolver
            .Instantiate<MainMenuUIRootBinder>(_sceneUIRootPrefab);
        uiRoot.AttachSceneUI(uiSceneRootBinder.gameObject);
        
        // Запрашиваем рутовую вью модель и пихаем ее в баиндер, который создали
        var uiSceneRootViewModel = resolver.Resolve<MainMenuUIRootViewModel>();
        uiSceneRootBinder.Bind(uiSceneRootViewModel);
        
        // Открываем окно
        var uiManager = resolver.Resolve<MainMenuUIManager>();
        uiManager.OpenScreenMainMenu();
    }
}
