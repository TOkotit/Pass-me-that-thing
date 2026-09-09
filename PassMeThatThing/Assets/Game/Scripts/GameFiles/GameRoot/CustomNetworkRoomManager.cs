using Mirror;
using System;
using Game.Scripts.GameFiles.LevelGeneration;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Game.Scripts.GameFiles.GameRoot
{
    
    //Структура сообщения для передачи сида по сети
    public struct LevelSeedMessage : NetworkMessage
    {
        public int Seed;
    }
    
    public class CustomNetworkRoomManager : NetworkRoomManager
    {
        public event Action<bool> OnServerSceneLoadStateChanged;
        public event Action<bool> OnClientSceneLoadStateChanged;
        
        private int? _syncedSeed = null;
        private bool _isLevelGeneratedOnClient = false;
        
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            NetworkClient.RegisterHandler<LevelSeedMessage>(OnClientReceiveSeedMessage);
        }
        
        public override void OnServerChangeScene(string newSceneName)
        {
            base.OnServerChangeScene(newSceneName);
            Debug.Log($"[CNRM] OnServerChangeScene new - {newSceneName}");
            OnServerSceneLoadStateChanged?.Invoke(true);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);
            Debug.Log($"[CNRM] OnServerSceneChanged {sceneName}");

            if (sceneName == GameplayScene)
            {
                var orchestrator = FindObjectOfType<LevelOrchestrator>();
                if (orchestrator)
                {
                    _syncedSeed = orchestrator.GenerateLevelFromConfig("Server");
                    Debug.Log($"[CNRM] Сервер утвердил сид для сессии: {_syncedSeed}");
                }
            }
            OnServerSceneLoadStateChanged?.Invoke(false);
        }
        
        //Отправка сида клиенту, когда он завершил загрузку сцены и готов
        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);

            var sceneName = SceneManager.GetActiveScene().path;
            if (sceneName == GameplayScene && _syncedSeed.HasValue)
            {
                conn.Send(new LevelSeedMessage { Seed = _syncedSeed.Value });
            }
        }

        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
            base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
            if (NetworkServer.active) return;


            _syncedSeed = null;
            _isLevelGeneratedOnClient = false;
            
            Debug.Log("[CNRM] OnClientChangeScene");
            OnClientSceneLoadStateChanged?.Invoke(true);
        }

        public override void OnClientSceneChanged()
        {
            
            base.OnClientSceneChanged();
            if (NetworkServer.active) return;

            Debug.Log("[CNRM] OnClientSceneChanged");
            
            TryGenerateClientLevel();
            
            OnClientSceneLoadStateChanged?.Invoke(false);
        }
        
        private void OnClientReceiveSeedMessage(LevelSeedMessage msg)
        {
            _syncedSeed = msg.Seed;
            Debug.Log($"[CNRM] Клиент получил сид от сервера: {_syncedSeed}");
            TryGenerateClientLevel();
        }
        
        private void TryGenerateClientLevel()
        {
            if (NetworkServer.active) return; 
            if (_isLevelGeneratedOnClient) return;

            var sceneName = SceneManager.GetActiveScene().path;
            if (sceneName == GameplayScene)
            {
                if (_syncedSeed.HasValue)
                {
                    Debug.Log($"[CNRM] Вызов генерации на клиенте с сидом {_syncedSeed}");
                    GenerateLevelDeterministic("Client", _syncedSeed.Value);
                    _isLevelGeneratedOnClient = true;
                }
                else
                {
                    Debug.Log("[CNRM] Сцена загружена, но сид от сервера еще не получен. Ожидание...");
                }
            }
        }
        
        private void GenerateLevelDeterministic(string who, int seed)
        {
            var orchestrator = FindObjectOfType<LevelOrchestrator>();
            if (!orchestrator)
            {
                Debug.LogError($"[CNRM] ({who}) LevelOrchestrator не найден на сцене!");
                return;
            }

            orchestrator.GenerateLevelFromConfig(who, seed);
        }
    }
}