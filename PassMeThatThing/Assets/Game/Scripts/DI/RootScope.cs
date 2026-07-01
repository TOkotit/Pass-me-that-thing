using FishNet.Managing;
using Mirror;
using Systems;
using UIRoot;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using VContainer;
using VContainer.Unity;

namespace DI
{
    /// <summary>
    /// Главный DI контейнер проекта
    /// </summary>
    public class RootScope : LifetimeScope
    {
        [SerializeField] private GameObject networkManager;
        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("RootScope.Configure called");
            
            var coroutines = new GameObject("[COROUTINES]").AddComponent<Coroutines>();
            DontDestroyOnLoad(coroutines.gameObject);
            builder.RegisterInstance<ICoroutineRunner>(coroutines);
            
            var uiRoot = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Root/UIRoot"));
            DontDestroyOnLoad(uiRoot.gameObject);
            var uiRootView = uiRoot.GetComponent<UIRootView>();
            builder.RegisterInstance<UIRootView>(uiRootView);
            
            var networkManagerGo = Instantiate(networkManager);
            DontDestroyOnLoad(networkManagerGo);
            
            var networkManagerComponent = networkManagerGo.GetComponent<NetworkManager>();
            if (!networkManagerComponent)
            {
                Debug.LogError("NetworkManager component not found on networkManager prefab.");
            }
            else
            {
                builder.RegisterComponent(networkManagerComponent);
            }
            
            builder.Register<GameInputManager>(Lifetime.Singleton);
            builder.Register<GameManager>(Lifetime.Singleton);
            
            
            
            var activeSceneName = SceneManager.GetActiveScene().name;
            
            
            if (Application.isEditor && !string.Equals(activeSceneName, "Boot", System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"Skipping RegisterEntryPoint — running in editor and active scene is '{activeSceneName}' (not 'Boot').");
            }
            else
            {
                builder.RegisterEntryPoint<Root.EntryPoint>(Lifetime.Singleton);
            }
        }
        
        
    }
}