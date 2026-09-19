using Mirror;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.GameRoot
{
    public class NetworkManagerContainer : MonoBehaviour
    {
        [SerializeField] private GameObject networkManager;
        private NetworkManager _instance;
        
        public NetworkManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var networkManagerGo = Instantiate(networkManager);
                    DontDestroyOnLoad(networkManagerGo);

                    var networkManagerComponent = networkManagerGo.GetComponent<NetworkManager>();
                    if (!networkManagerComponent)
                    {
                        Debug.LogError("NetworkManager component not found on networkManager prefab.");
                    }
                    else
                    {
                        _instance = networkManagerComponent;
                    }
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }


        public NetworkManagerContainer()
        {
            
        }
    }
}
