
using Assets.Game.Scripts.GameFiles.GameRoot;
using Game.MainMenu.View.UI;
using Mirror;
using Mirror.FizzySteam;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DI
{
    public class NetworkManagerScope : LifetimeScope
    {
        [SerializeField] private GameObject networkManager;
        [SerializeField] private GameSessionController sessionController;
        protected override void Awake()
        {
            Debug.Log("NetworkManagerScope Awake");
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("NetworkManagerScope Configure");

            var networkManagerGo = Instantiate(networkManager);
            
            DontDestroyOnLoad(networkManagerGo);

            var networkManagerComponent = networkManagerGo.GetComponent<NetworkManager>();
            if (!networkManagerComponent)
            {
                Debug.LogError("NetworkManager component not found on networkManager prefab.");
            }
            else
            {
                if (Parent.Container.Resolve<RootNetworkConfig>().IsSteamUsing)
                    networkManagerComponent.transport = Parent.Container.Resolve<FizzySteamworks>();

                builder.RegisterComponent(networkManagerComponent);
            }

            builder.RegisterComponent(sessionController);

            builder.RegisterEntryPoint<NetworkManagerScopeEntryPoint>();
        }

        protected override void OnDestroy()
        {
            Debug.Log("NetworkManagerScope OnDestroy");
            if (NetworkManager.singleton != null)
            {
                if (NetworkServer.active || NetworkClient.active)
                {
                    NetworkManager.singleton.StopHost();
                }
                if (NetworkClient.isConnected)
                {
                    NetworkManager.singleton.StopClient();
                }

                if (NetworkManager.singleton != null && NetworkManager.singleton.gameObject != null)
                {
                    Destroy(NetworkManager.singleton.gameObject);
                }
            }

            base.OnDestroy();
        }
    }
}
