using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Game.Scripts.GameFiles.LevelGeneration.ItemSpawn;
using Game.Scripts.GameFiles.Items;
using Game.Scripts.GameFiles.Items.ItemPhysics;
using Game.Scripts.GameFiles.LevelGeneration;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using Game.Scripts.GameFiles.LevelGeneration.ItemSpawn;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.GameRoot
{
    public struct IterationInfo
    {
        public int Iteration;
        public int Seed;
    }

    public class LevelSessionManager : NetworkBehaviour
    {
        [SerializeField] [Range(0, 1000)] private float ClientsTimeout = 20f;

        [SerializeField] private LevelOrchestrator _levelOrchestrator;
        [SerializeField] private LevelConfig _levelConfig;
        [SerializeField] public NetworkObjectsOrchestrator networkObjectsOrchestrator;
        [SerializeField] public NetworkRarityItemsOrchestrator networkRarityItemsOrchestrator;

        [SyncVar(hook = nameof(OnIterationInfoChanged))]
        private IterationInfo _info;

        private int _builtIteration;
        private bool _isIterating;
        private int _serverHash;
        private bool _serverHashReady;

        private readonly Dictionary<int, int> _clientHashes = new();
        private readonly List<GameObject> _spawnedNetworkObjects = new();

        public int CurrentIteration => _info.Iteration;
        public int CurrentSeed => _info.Seed;
        public bool IsIterating => _isIterating;
        public IReadOnlyList<GameObject> SpawnedNetworkObjects => _spawnedNetworkObjects;

        [Inject]
        private void Construct(
            ItemRarityDatabase rarityDatabase,
            ItemPoolManager itemPoolManager,
            PhysicalItemRegistry physicalItemRegistry)
        {
            networkRarityItemsOrchestrator.Init(rarityDatabase, itemPoolManager, physicalItemRegistry);
        }

        
        public override void OnStartServer()
        {
            base.OnStartServer();
            StartCoroutine(StartFirstIterationRoutine());
        }

        private IEnumerator StartFirstIterationRoutine()
        {
            var deadline = Time.unscaledTime + 30f;

            while (!AllConnectionsReady() && Time.unscaledTime < deadline)
                yield return null;

            yield return null;
            StartNewIteration();
        }
        
        private bool AllConnectionsReady()
        {
            if (NetworkServer.connections.Count == 0) return false;

            return NetworkServer.connections.Values.All(conn => conn.isReady);
        }
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            TryBuildOnClient();
        }

        [Server]
        public void RegisterSpawned(GameObject obj)
        {
            if (obj) _spawnedNetworkObjects.Add(obj);
        }

        [Server]
        public void StartNewIteration()
        {
            if (_isIterating) return;

            if (!_levelOrchestrator || !_levelConfig || !networkObjectsOrchestrator || !networkRarityItemsOrchestrator)
            {
                Debug.LogError("[LSM] Не назначены ссылки в инспекторе");
                return;
            }

            StartCoroutine(ServerIterationRoutine());
        }

        private IEnumerator ServerIterationRoutine()
        {
            _isIterating = true;
            _serverHashReady = false;
            _clientHashes.Clear();

            ClearSpawned();
            networkObjectsOrchestrator.ClearSpawnObjects();

            var seed = Random.Range(int.MinValue, int.MaxValue);

            var info = new IterationInfo { Iteration = _info.Iteration + 1, Seed = seed };
            _info = info;

            yield return StartCoroutine(BuildRoutine(info));

            _serverHash = ComputeLayoutHash();
            _serverHashReady = true;
            foreach (var pair in _clientHashes)
                CheckHash(pair.Key, pair.Value);

            yield return StartCoroutine(WaitForClients());

            networkObjectsOrchestrator.SpawnNetworkObjects(_levelOrchestrator.AllLevelSpots);
            networkRarityItemsOrchestrator.SpawnNetworkRarityItem(_levelOrchestrator.AllLevelRarityItemSpots);

            yield return null;
            PlacePlayers();

            _isIterating = false;
        }
        
        private void PlacePlayers()
        {
            var points = NetworkManager.startPositions.Where(p => p).ToList();

            if (points.Count == 0)
            {
                Debug.LogError("[LSM] Нет точек спавна игроков");
                return;
            }

            var i = 0;
            foreach (var conn in NetworkServer.connections.Values)
            {
                if (!conn.identity) continue;

                var point = points[i % points.Count];
                i++;
                TargetTeleportPlayer(conn, point.position, point.rotation);
            }
        }

        [TargetRpc]
        private void TargetTeleportPlayer(NetworkConnectionToClient target, Vector3 pos, Quaternion rot)
        {
            var player = NetworkClient.localPlayer;
            if (!player) return;

            var cc = player.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;

            player.transform.SetPositionAndRotation(pos, rot);

            if (cc) cc.enabled = true;
        }

        private IEnumerator BuildRoutine(IterationInfo info)
        {
            _levelOrchestrator.ClearLevel();
            yield return null;
            Physics.SyncTransforms();

            _levelOrchestrator.GenerateLevelFromConfig("LSM", info.Seed, _levelConfig);
            networkObjectsOrchestrator.SetLevelSpots(_levelOrchestrator.AllLevelSpots);
        }

        private void OnIterationInfoChanged(IterationInfo oldInfo, IterationInfo newInfo)
        {
            TryBuildOnClient();
        }

        private void TryBuildOnClient()
        {
            if (isServer) return;
            if (_info.Iteration <= 0) return;
            if (_info.Iteration == _builtIteration) return;

            _builtIteration = _info.Iteration;
            StartCoroutine(ClientBuildRoutine(_info));
        }

        private IEnumerator ClientBuildRoutine(IterationInfo info)
        {
            yield return StartCoroutine(BuildRoutine(info));

            if (info.Iteration != _info.Iteration) yield break;

            CmdLevelReady(info.Iteration, ComputeLayoutHash());
        }

        [Command(requiresAuthority = false)]
        private void CmdLevelReady(int iteration, int hash, NetworkConnectionToClient sender = null)
        {
            if (sender == null) return;
            if (iteration != _info.Iteration) return;

            _clientHashes[sender.connectionId] = hash;

            if (_serverHashReady)
                CheckHash(sender.connectionId, hash);
        }

        private IEnumerator WaitForClients()
        {
            var deadline = Time.unscaledTime + ClientsTimeout;

            while (!AllClientsReady())
            {
                if (Time.unscaledTime > deadline)
                {
                    LogMissingClients();
                    yield break;
                }

                yield return null;
            }
        }

        private bool AllClientsReady()
        {
            foreach (var conn in NetworkServer.connections.Values)
            {
                if (conn == NetworkServer.localConnection) continue;
                if (!conn.isReady) continue;
                if (!_clientHashes.ContainsKey(conn.connectionId)) return false;
            }

            return true;
        }

        private void LogMissingClients()
        {
            foreach (var conn in NetworkServer.connections.Values)
            {
                if (conn == NetworkServer.localConnection) continue;
                if (!conn.isReady) continue;
                if (_clientHashes.ContainsKey(conn.connectionId)) continue;

                Debug.LogError($"[LSM] Клиент {conn.connectionId} не ответил за {ClientsTimeout} сек. на итерацию {_info.Iteration}");
            }
        }

        private void CheckHash(int connectionId, int hash)
        {
            if (hash == _serverHash) return;

            Debug.LogError($"[LSM] РАСХОЖДЕНИЕ раскладки. Клиент {connectionId}: {hash}, сервер: {_serverHash}, итерация {_info.Iteration}, сид {_info.Seed}");
        }

        private int ComputeLayoutHash()
        {
            unchecked
            {
                var h = 17;
                var rooms = _levelOrchestrator.AllPlacedRooms;

                if (rooms != null)
                {
                    foreach (var room in rooms)
                    {
                        h = h * 31 + room.RoomId;
                        h = h * 31 + room.Origin.x;
                        h = h * 31 + room.Origin.y;
                        h = h * 31 + room.Origin.z;
                        h = h * 31 + (int)room.Rotation;
                    }
                }

                h = h * 31 + _levelOrchestrator.AllLevelSpots.Count;
                return h;
            }
        }

        private void ClearSpawned()
        {
            foreach (var obj in _spawnedNetworkObjects)
            {
                if (obj) NetworkServer.Destroy(obj);
            }

            _spawnedNetworkObjects.Clear();
        }
    }
}