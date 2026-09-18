using DI;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace Assets.Game.Scripts.GameFiles.GameRoot
{
    public class GameSessionController : MonoBehaviour
    {
        [Inject] private NetworkManagerScope _networkScope;

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
        }
    }
}
