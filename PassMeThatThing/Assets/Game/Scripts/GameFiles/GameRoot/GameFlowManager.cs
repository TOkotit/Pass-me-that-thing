using System;
using Game.Scripts.GameFiles.LevelGeneration;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Scripts.GameFiles.GameRoot
{
    
    public struct LevelSeedMessage : NetworkMessage
    {
        public int Seed;
    }

    
    /// <summary>
    /// Прослойка, которая управляет ходом игры: миссия, хаб, смена сцен, сид уровня.
    /// Не зависит от RoomManager и не требует объекта на сцене: достаточно вызвать
    /// GameFlowManager.Instance из любого места.
    ///
    /// RoomManager только пересылает сюда колбэки Mirror.
    /// </summary>
    public sealed class GameFlowManager
    {
        private static GameFlowManager _instance;
        public static GameFlowManager Instance => _instance ??= new GameFlowManager();
 
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _instance = null;
 
        
        // настройки из рум менеджера
        private string _lobbyScene;
        private string _gameplayScene;
        private string _hubScene;
        private int? _fixedSessionSeed;
 
        public void Configure(string lobbyScene, string gameplayScene, string hubScene, int? fixedSessionSeed)
        {
            _lobbyScene = lobbyScene;
            _gameplayScene = gameplayScene;
            _hubScene = hubScene;
            _fixedSessionSeed = fixedSessionSeed;
        }
 
       
        // настройки состояния сервера
        private SessionProgressionManager _session;
        private bool _isTransitioning;
        private int? _serverMissionSeed;
 
        // Прогресс сессии. Есть только на сервере и только пока идёт игра, иначе null
        public SessionProgressionManager Session => _session;
 
        public bool IsSessionActive => _session != null;
 
        // Сюда можно подписать создание хаба или старт контроллера миссии.
        public event Action<string> OnServerSceneReady;
 
        // Состояние клиента
        private int? _clientMissionSeed;
        private bool _isLevelGeneratedOnClient;
 
        
        // ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ ИГРЫ (только на сервере)
        
        /// <summary>Миссия выполнена. Дальше менеджер сам решит: следующая миссия или хаб.</summary>
        public void MissionCompleted()
        {
            if (!CanTransition()) return;
 
            var step = _session.CompleteMission();
            Debug.Log($"[FLOW] Миссия выполнена. Блок {_session.BlockNumber}, миссия {_session.MissionInBlock}/{_session.RequiredMissionsInBlock}, шаг: {step}");
 
            switch (step)
            {
                case SessionStep.NextMission:
                    GoToScene(_gameplayScene);
                    break;
                case SessionStep.GoToHub:
                    GoToScene(_hubScene);
                    break;
                case SessionStep.Ignored:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
 
        /// <summary>Все игроки погибли. Пока просто конец забега и возврат в лобби.</summary>
        public void MissionFailed()
        {
            if (!CanTransition()) return;
 
            Debug.Log("[FLOW] Миссия провалена, возврат в лобби");
            _session = null;
            GoToScene(_lobbyScene);
        }
 
        /// <summary>Игроки выходят из хаба, начинается следующий блок миссий.</summary>
        public void LeaveHub()
        {
            if (!CanTransition()) return;
 
            if (_session.LeaveHub())
                GoToScene(_gameplayScene);
        }
 
        private bool CanTransition()
        {
            return NetworkServer.active && _session != null && !_isTransitioning;
        }
 
        private void GoToScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[FLOW] Сцена для перехода не задана (проверьте Hub Scene в инспекторе RoomManager)");
                return;
            }
 
            _isTransitioning = true;
            NetworkManager.singleton.ServerChangeScene(sceneName);
        }
 
        // КОЛБЭКИ MIRROR, СЕРВЕР (вызывает RoomManager)
        public void ServerStopped()
        {
            _session = null;
            _isTransitioning = false;
            _serverMissionSeed = null;
        }
 
        public void ServerSceneChanging(string newSceneName)
        {
            _isTransitioning = true;
 
            // Старый сид больше не актуален, иначе клиент мог бы получить сид прошлой миссии
            _serverMissionSeed = null;
 
            // Игра стартует из лобби: создаём новую сессию
            if (newSceneName == _gameplayScene && _session == null)
            {
                var sessionSeed = _fixedSessionSeed ?? UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                _session = new SessionProgressionManager(sessionSeed);
                Debug.Log($"[FLOW] Новая сессия, сид сессии: {sessionSeed}");
            }
        }
 
        public void ServerSceneChanged(string sceneName)
        {
            if (sceneName == _gameplayScene && _session != null)
            {
                var orchestrator = UnityEngine.Object.FindObjectOfType<LevelOrchestrator>();
                if (orchestrator)
                {
                    _serverMissionSeed = _session.GetMissionSeed();
                    orchestrator.GenerateLevelFromConfig("Server", _serverMissionSeed);
                    Debug.Log($"[FLOW] Сид миссии {_session.TotalMissionNumber}: {_serverMissionSeed}");
 
                    // Клиенты, которые стали ready раньше, чем сервер сгенерировал уровень,
                    // не получили сид в ServerClientReady. Досылаем им.
                    foreach (var conn in NetworkServer.connections.Values)
                    {
                        if (conn == NetworkServer.localConnection || !conn.isReady) continue;
                        conn.Send(new LevelSeedMessage { Seed = _serverMissionSeed.Value });
                    }
                }
                else
                {
                    Debug.LogError("[FLOW] LevelOrchestrator не найден на сцене миссии!");
                }
            }
            else if (sceneName == _hubScene && _session != null)
            {
                Debug.Log($"[FLOW] Хаб, визит номер {_session.HubVisits}");
            }
 
            _isTransitioning = false;
            OnServerSceneReady?.Invoke(sceneName);
        }
 
        /// <summary>
        /// Отправка сида клиенту, когда он завершил загрузку сцены и готов
        /// </summary>
        /// <param name="conn"></param>
        public void ServerClientReady(NetworkConnectionToClient conn)
        {
            var sceneName = SceneManager.GetActiveScene().path;
            if (sceneName == _gameplayScene && _serverMissionSeed.HasValue)
            {
                conn.Send(new LevelSeedMessage { Seed = _serverMissionSeed.Value });
            }
        }
 
        // КОЛБЭКИ MIRROR, КЛИЕНТ (вызывает RoomManager)
 
        public void ClientStarted()
        {
            NetworkClient.RegisterHandler<LevelSeedMessage>(OnClientReceiveSeedMessage);
        }
 
        public void ClientSceneChanging()
        {
            if (NetworkServer.active) return; 
 
            _clientMissionSeed = null;
            _isLevelGeneratedOnClient = false;
        }
 
        public void ClientSceneChanged()
        {
            if (NetworkServer.active) return;
 
            TryGenerateClientLevel();
        }
 
        private void OnClientReceiveSeedMessage(LevelSeedMessage msg)
        {
            _clientMissionSeed = msg.Seed;
            Debug.Log($"[FLOW] Клиент получил сид от сервера: {_clientMissionSeed}");
            TryGenerateClientLevel();
        }
 
        private void TryGenerateClientLevel()
        {
            if (NetworkServer.active) return;
            if (_isLevelGeneratedOnClient) return;
 
            var sceneName = SceneManager.GetActiveScene().path;
            if (sceneName != _gameplayScene) return;
 
            if (!_clientMissionSeed.HasValue)
            {
                Debug.Log("[FLOW] Сцена загружена, но сид от сервера ещё не получен. Ожидание...");
                return;
            }
 
            var orchestrator = UnityEngine.Object.FindObjectOfType<LevelOrchestrator>();
            if (!orchestrator)
            {
                Debug.LogError("[FLOW] (Client) LevelOrchestrator не найден на сцене!");
                return;
            }
 
            Debug.Log($"[FLOW] Генерация на клиенте с сидом {_clientMissionSeed}");
            orchestrator.GenerateLevelFromConfig("Client", _clientMissionSeed.Value);
            _isLevelGeneratedOnClient = true;
        }
    }

}