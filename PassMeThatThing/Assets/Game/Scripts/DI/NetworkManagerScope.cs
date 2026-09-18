
using Assets.Game.Scripts.GameFiles.GameRoot;
using Game.MainMenu.View.UI;
using Mirror;
using Mirror.FizzySteam;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Root;

namespace DI
{
    public class NetworkManagerScope : LifetimeScope
    {
        [SerializeField] private GameObject networkManager;
        [SerializeField] private GameObject steamLobbyManager;
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
            var isSteam = Parent.Container.Resolve<RootNetworkConfig>().IsSteamUsing;

            var networkManagerGo = Instantiate(networkManager);
            DontDestroyOnLoad(networkManagerGo);

            var networkManagerComponent = networkManagerGo.GetComponent<NetworkManager>();
            if (!networkManagerComponent)
            {
                Debug.LogError("NetworkManager component not found on networkManager prefab.");
            }
            else
            {
                if (isSteam)
                    networkManagerComponent.transport = Parent.Container.Resolve<FizzySteamworks>();

                builder.RegisterComponent(networkManagerComponent);
            }

            if (isSteam)
            {
                var steamLobbyManagerGo = Instantiate(steamLobbyManager);
                DontDestroyOnLoad(steamLobbyManagerGo);

                var steamLobbyManagerComponent = steamLobbyManagerGo.GetComponent<SteamLobbyManager>();
                if (!steamLobbyManagerComponent)
                {
                    Debug.LogError("steamLobbyManagerComponent component not found prefab");
                }
                else
                {
                    builder.RegisterComponent(steamLobbyManagerComponent);
                }
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
