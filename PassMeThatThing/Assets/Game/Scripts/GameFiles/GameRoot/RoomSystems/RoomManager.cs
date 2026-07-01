using System;
using System.Collections.Generic;
using FishNet;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Logging;

using FishNet.Component.Spawning;
using FishNet.Managing;
using FishNet.Managing.Scened;

public class CustomRoomManager : MonoBehaviour
{

        public event Action<NetworkObject> OnSpawned;


        [Tooltip("Prefab to spawn for the player.")]
        [SerializeField]
        private NetworkObject _playerPrefab;


        public void SetPlayerPrefab(NetworkObject nob) => _playerPrefab = nob;


        [Tooltip("True to add player to the active scene when no global scenes are specified through the SceneManager.")]
        [SerializeField]
        private bool _addToDefaultScene = true;

        public Transform[] Spawns = new Transform[0];



        private NetworkManager _networkManager;

        private int _nextSpawn;


        private void Awake()
        {
            InitializeOnce();
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
                _networkManager.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
        }


        private void InitializeOnce()
        {
            _networkManager = GetComponentInParent<NetworkManager>();
            if (_networkManager == null)
                _networkManager = InstanceFinder.NetworkManager;

            if (_networkManager == null)
            {
                _networkManager.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
                return;
            }

            _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
        }


        private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
        {
            if (!asServer)
                return;
            if (_playerPrefab == null)
            {
                _networkManager.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
                return;
            }

            Vector3 position;
            Quaternion rotation;
            SetSpawn(_playerPrefab.transform, out position, out rotation);

            NetworkObject nob = _networkManager.GetPooledInstantiated(_playerPrefab, position, rotation, true);
            _networkManager.ServerManager.Spawn(nob, conn);

            // If there are no global scenes 
            if (_addToDefaultScene)
                _networkManager.SceneManager.AddOwnerToDefaultScene(nob);

            OnSpawned?.Invoke(nob);
        }


        private void SetSpawn(Transform prefab, out Vector3 pos, out Quaternion rot)
        {
            // No spawns specified.
            if (Spawns.Length == 0)
            {
                SetSpawnUsingPrefab(prefab, out pos, out rot);
                return;
            }

            Transform result = Spawns[_nextSpawn];
            if (result == null)
            {
                SetSpawnUsingPrefab(prefab, out pos, out rot);
            }
            else
            {
                pos = result.position;
                rot = result.rotation;
            }

            // Increase next spawn and reset if needed.
            _nextSpawn++;
            if (_nextSpawn >= Spawns.Length)
                _nextSpawn = 0;
        }

        private void SetSpawnUsingPrefab(Transform prefab, out Vector3 pos, out Quaternion rot)
        {
            pos = prefab.position;
            rot = prefab.rotation;
        }
    
    public static CustomRoomManager Instance { get; private set; }

    [Header("Scene Settings")]
    [SerializeField] private string offlineSceneName = "OfflineScene";
    [SerializeField] private string roomSceneName = "RoomScene";
    [SerializeField] private string gameplaySceneName = "GameplayScene";

    [Header("Prefab Settings")]
    [SerializeField] private NetworkObject roomPlayerPrefab;
    [SerializeField] private NetworkObject gameplayPlayerPrefab;

    public List<NetworkRoomPlayer> RoomPlayers { get; private set; } = new List<NetworkRoomPlayer>();

    protected void Awake()
    {
        Instance = this;
    }

    public void OnStartServer()
    {
        // Как только сервер стартовал (например, при нажатии Host), 
        // он сразу переводит всех на глобальную сцену комнаты
        LoadRoomScene();
    }

    private void LoadRoomScene()
    {
        SceneLoadData sld = new SceneLoadData(roomSceneName);
        sld.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    }

    [Server(Logging = LoggingType.Off)]
    protected void SpawnPlayer(NetworkConnection connection)
    {
        // Спавним лобби-игрока только если активна сцена комнаты
        // if (InstanceFinder.SceneManager.GetScene(roomSceneName).IsValid())
        {
            NetworkObject noble = Instantiate(roomPlayerPrefab);
            InstanceFinder.ServerManager.Spawn(noble, connection);
        }
    }

    public void RegisterRoomPlayer(NetworkRoomPlayer player)
    {
        if (!RoomPlayers.Contains(player)) RoomPlayers.Add(player);
        RefreshGUI();
    }

    public void UnregisterRoomPlayer(NetworkRoomPlayer player)
    {
        if (RoomPlayers.Contains(player)) RoomPlayers.Remove(player);
        RefreshGUI();
    }

    public void RefreshGUI()
    {
        FindObjectOfType<RoomGUI>()?.UpdateLobbyUI();
    }

    [Server]
    public void CheckStartCondition()
    {
        if (RoomPlayers.Count == 0) return;

        foreach (var player in RoomPlayers)
        {
            if (!player.IsReady.Value) return;
        }

        StartGameplay();
    }

    [Server]
    private void StartGameplay()
    {
        List<NetworkConnection> connections = new List<NetworkConnection>();
        foreach (var p in RoomPlayers)
        {
            if (p.Owner != null) connections.Add(p.Owner);
        }

        RoomPlayers.Clear();

        SceneLoadData sld = new SceneLoadData(gameplaySceneName);
        sld.Options.DisallowedScenes = new string[] { roomSceneName };
        
        NetworkManager.SceneManager.LoadGlobalScenes(sld);

        NetworkManager.SceneManager.OnClientLoadedSceneStart += SpawnGameplayPlayers;
        
        void SpawnGameplayPlayers(SceneLoadStartEventArgs args)
        {
            if (args.QueueData.SceneLoadData.SceneNames[0] == gameplaySceneName)
            {
                foreach (var conn in connections)
                {
                    if (conn.IsActive)
                    {
                        NetworkObject gamePlayer = Instantiate(gameplayPlayerPrefab);
                        NetworkManager.ServerManager.Spawn(gamePlayer, conn);
                    }
                }
                NetworkManager.SceneManager.OnClientLoadedSceneStart -= SpawnGameplayPlayers;
            }
        }
    }

    // Метод для корректного выхода в оффлайн-сцену
    public void LeaveToOffline()
    {
        // Отключаем сеть
        if (NetworkManager.IsServerStarted) NetworkManager.ServerManager.StopConnection(true);
        if (NetworkManager.IsClientStarted) NetworkManager.ClientManager.StopConnection();

        // Локально загружаем offline-сцену обратно для игрока (если он был клиентом)
        UnityEngine.SceneManagement.SceneManager.LoadScene(offlineSceneName);
    }
}