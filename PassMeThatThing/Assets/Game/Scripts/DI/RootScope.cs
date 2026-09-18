using Assets.Game.Scripts.GameFiles.GameRoot;
using Game.Scripts.Systems;
using Mirror;
using Mirror.FizzySteam;
using Root;
using System.Runtime.InteropServices.ComTypes;
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
        [SerializeField] private RootNetworkConfig rootNetworkConfig;
        [SerializeField] private GameObject steamAPI;

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("RootScope.Configure called");

            builder.RegisterInstance(rootNetworkConfig);
            
            var coroutines = new GameObject("[COROUTINES]").AddComponent<Coroutines>();
            DontDestroyOnLoad(coroutines.gameObject);
            builder.RegisterInstance<ICoroutineRunner>(coroutines);
            
            var uiRoot = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Root/UIRoot"));
            DontDestroyOnLoad(uiRoot.gameObject);

            var uiRootView = uiRoot.GetComponent<UIRootView>();
            builder.RegisterInstance(uiRootView);


            if (rootNetworkConfig.IsSteamUsing)
            {
                var steamAPIGo = Instantiate(steamAPI);
                DontDestroyOnLoad(steamAPIGo);

                var steamAPIFizzySteamWorksComponent = steamAPIGo.GetComponent<FizzySteamworks>();
                if (!steamAPIFizzySteamWorksComponent)
                {
                    Debug.LogError("steamAPIFizzySteamWorksComponent component not found prefab");
                }
                else
                {
                    builder.RegisterComponent(steamAPIFizzySteamWorksComponent);
                }
            }

            builder.Register<ConnectedPlayers>(Lifetime.Singleton);
            builder.Register<OptionsManager>(Lifetime.Singleton);
            
            builder.Register<GameInputManager>(Lifetime.Singleton);
            builder.Register<GameManager>(Lifetime.Singleton);
            
            
            builder.RegisterEntryPoint<EntryPoint>();
        }
    }
}