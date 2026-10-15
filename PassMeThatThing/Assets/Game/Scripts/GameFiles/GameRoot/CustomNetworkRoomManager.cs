using Mirror;
using System;
using Game.Scripts.GameFiles.GameRoot;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.GameRoot
{
    /// <summary>
    /// Нужен для лобби. Вся логика миссий, хаба и сидов лежит в GameFlowManager,
    /// здесь только пересылка колбэков Mirror.
    /// </summary>
    public class CustomNetworkRoomManager : NetworkRoomManager
    {
        [Header("Hub")]
        [Scene, SerializeField] private string hubScene;

        [Header("Debug")]
        [Tooltip("Если включено, сид сессии берётся из поля ниже, а не случайный")]
        [SerializeField] private bool useFixedSessionSeed;
        [SerializeField] private int fixedSessionSeed;

        public event Action<bool> OnServerSceneLoadStateChanged;
        public event Action<bool> OnClientSceneLoadStateChanged;

        private static GameFlowManager Flow => GameFlowManager.Instance;

        private void ConfigureFlow()
        {
            Flow.Configure(
                lobbyScene: RoomScene,
                gameplayScene: GameplayScene,
                hubScene: hubScene,
                fixedSessionSeed: useFixedSessionSeed ? (int?)fixedSessionSeed : null);
        }

        // ---------------- сервер ----------------

        public override void OnStartServer()
        {
            base.OnStartServer();
            ConfigureFlow();
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            Flow.ServerStopped();
        }

        public override void OnServerChangeScene(string newSceneName)
        {
            base.OnServerChangeScene(newSceneName);
            Debug.Log($"[CNRM] OnServerChangeScene new - {newSceneName}");

            Flow.ServerSceneChanging(newSceneName);
            OnServerSceneLoadStateChanged?.Invoke(true);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);
            Debug.Log($"[CNRM] OnServerSceneChanged {sceneName}");

            Flow.ServerSceneChanged(sceneName);
            OnServerSceneLoadStateChanged?.Invoke(false);
        }

        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
            Flow.ServerClientReady(conn);
        }

        // ---------------- клиент ----------------

        public override void OnStartClient()
        {
            base.OnStartClient();
            ConfigureFlow();
            Flow.ClientStarted();
        }

        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
            base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
            if (NetworkServer.active) return;

            Debug.Log("[CNRM] OnClientChangeScene");
            Flow.ClientSceneChanging();
            OnClientSceneLoadStateChanged?.Invoke(true);
        }

        public override void OnClientSceneChanged()
        {
            base.OnClientSceneChanged();
            if (NetworkServer.active) return;

            Debug.Log("[CNRM] OnClientSceneChanged");
            Flow.ClientSceneChanged();
            OnClientSceneLoadStateChanged?.Invoke(false);
        }
    }
}