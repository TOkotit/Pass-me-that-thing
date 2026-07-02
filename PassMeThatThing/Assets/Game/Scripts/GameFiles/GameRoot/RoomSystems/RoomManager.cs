using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Logging;

using FishNet.Component.Spawning;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;

public class CustomRoomManager : MonoBehaviour
{
    
    [Header("Scenes")]
    [SerializeField] private string offlineSceneName = "OfflineScene";
    [SerializeField] private string roomSceneName = "RoomScene";
    [SerializeField] private string gameplaySceneName = "GameplayScene";

    [Header("Prefabs")]
    [SerializeField] private NetworkObject roomPlayerPrefab;
    [SerializeField] private NetworkObject gameplayPlayerPrefab;
    
    [Tooltip("True to add player to the active scene when no global scenes are specified through the SceneManager.")]
    [SerializeField]
    private bool _addToDefaultScene = true;
    
    // [Tooltip("Prefab to spawn for the player.")]
    // [SerializeField]
    // private NetworkObject _playerPrefab;
    
    
    private NetworkManager _networkManager;
    private int _nextSpawn;
    
    public static CustomRoomManager Instance { get; private set; }

    public List<NetworkRoomPlayer> RoomPlayers { get; private set; } = new ();

    public void SetPlayerPrefab(NetworkObject nob) => gameplayPlayerPrefab = nob;

    public Transform[] Spawns = new Transform[0];


    public event Action<NetworkObject> OnSpawned;

    private void Awake()
    {
        InitializeOnce();
    }
    
    private void InitializeOnce()
    {
        Instance = this;
        
        _networkManager = GetComponentInParent<NetworkManager>();
        if (_networkManager == null)
            _networkManager = InstanceFinder.NetworkManager;

        if (_networkManager == null)
        {
            _networkManager.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
            return;
        }
        
        _networkManager.SceneManager.OnClientLoadedStartScenes += SpawnRoomPlayer;
        
        _networkManager.ServerManager.OnServerConnectionState += serverStateChanged;

    }
    
    private void OnDestroy()
    {
        // if (_networkManager != null)
        //     _networkManager.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
    }

    [Server]
    private void SpawnGameplayPlayer(NetworkConnection conn, bool asServer)
    {
        if (!asServer)
            return;
        if (gameplayPlayerPrefab == null)
        {
            _networkManager.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }

        Vector3 position;
        Quaternion rotation;
        SetSpawn(gameplayPlayerPrefab.transform, out position, out rotation);

        NetworkObject nob = _networkManager.GetPooledInstantiated(gameplayPlayerPrefab, position, rotation, true);
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

    [Server]
    private void SpawnRoomPlayer(NetworkConnection conn, bool asServer)
    {
        if (!asServer)
            return;
        if (roomPlayerPrefab == null)
        {
            _networkManager.LogWarning($"Room Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }

        Vector3 position;
        Quaternion rotation;
        SetSpawn(roomPlayerPrefab.transform, out position, out rotation);

        NetworkObject nob = _networkManager.GetPooledInstantiated(roomPlayerPrefab, position, rotation, true);
        _networkManager.ServerManager.Spawn(nob, conn);

        // If there are no global scenes 
        if (_addToDefaultScene)
            _networkManager.SceneManager.AddOwnerToDefaultScene(nob);

        OnSpawned?.Invoke(nob);
    }

    public void Start()
    {
        Debug.Log($"Starting LoadRoomScene");
    }

    private void serverStateChanged(ServerConnectionStateArgs  args)
    {
        Debug.Log($"serverStateChanged {args.ConnectionState}");
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            LoadRoomScene();
            _networkManager.ServerManager.OnServerConnectionState -= serverStateChanged;
        }
        
    }

    [Server]
    private void LoadRoomScene()
    {
        SceneLoadData sld = new SceneLoadData(roomSceneName);
        sld.ReplaceScenes = ReplaceOption.All;
        _networkManager.SceneManager.LoadGlobalScenes(sld);
    }
    
    [Server]
    private void LoadGameplayScene()
    {
        SceneLoadData sld = new SceneLoadData(gameplaySceneName);
        
        sld.ReplaceScenes = ReplaceOption.All;
        
        _networkManager.SceneManager.LoadGlobalScenes(sld);
        
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

        LoadGameplayScene();
        
        //почему-то 2 раза инвок
        _networkManager.SceneManager.OnClientPresenceChangeEnd += ClientPresenceChangeEnd;
    }


    // private void loadEnd(SceneLoadEndEventArgs args)
    // {
    //     Debug.Log($"<color=yellow>loadEnd");
    // }
    //
    // private void queueEnd()
    // {
    //     Debug.Log($"<color=yellow>queueEnd");
    //
    // }
    //
    // private void ActiveSceneSet(bool a)
    // {
    //     Debug.Log($"<color=yellow>ActiveSceneSet");
    // }
    
    [Server]
    private void ClientPresenceChangeEnd(ClientPresenceChangeEventArgs args)
    {
        Debug.Log($"<color=yellow>ClientPresenceChangeEnd");
        SpawnGameplayPlayersFromConns();
    }

    [Server]
    private void SpawnGameplayPlayersFromConns()
    {
        // foreach (var s in args.LoadedScenes)
        // {
        //     Debug.Log($"<color=yellow>SpawnGameplayPlayersFromConns {s.name}");
        // }
        
        
        foreach (var v in _networkManager.ServerManager.Clients)
        {
            Debug.Log($"<color=yellow> CLIENT {v.Value.ClientId} Total {_networkManager.ServerManager.Clients.Count}");
            
            SpawnGameplayPlayer(v.Value, true);
        }
        
        _networkManager.SceneManager.OnClientPresenceChangeEnd -= ClientPresenceChangeEnd;
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
        // FindObjectOfType<RoomGUI>()?.UpdateLobbyUI();
    }

    

    public void LeaveToOffline()
    {
        // if (NetworkManager.IsServerStarted) NetworkManager.ServerManager.StopConnection(true);
        // if (NetworkManager.IsClientStarted) NetworkManager.ClientManager.StopConnection();
        //
        //
        // UnityEngine.SceneManagement.SceneManager.LoadScene(offlineSceneName);
    }
}