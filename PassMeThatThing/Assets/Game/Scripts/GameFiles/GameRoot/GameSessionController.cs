using DI;
using Mirror;
using Root;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.GameRoot
{
    public class GameSessionController : MonoBehaviour
    {
        [Inject] private NetworkManagerScope _networkScope;
        [Inject] private RootNetworkConfig _rootNetworkConfig;

        private SteamLobbyManager _steamLobbyManager;

        [Inject]
        public void Construct(IObjectResolver resolver)
        {
            if (_rootNetworkConfig.IsSteamUsing)
            {
                _steamLobbyManager = resolver.Resolve<SteamLobbyManager>();
            }
        }

        public void ReturnToMainMenu()
        {
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopHost();
            }
            else if (NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopClient();
                SceneManager.LoadScene("MainMenu");
            }

            Destroy(_networkScope.gameObject);
            if (_rootNetworkConfig.IsSteamUsing)
            {
                _steamLobbyManager.LeaveLobby();
                Destroy(_steamLobbyManager.gameObject);
            }
                
        }
    }
}
