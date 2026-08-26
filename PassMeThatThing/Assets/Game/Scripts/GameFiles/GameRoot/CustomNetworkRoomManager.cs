using Mirror;
using System;
using Game.Scripts.GameFiles.LevelGeneration;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Game.Scripts.GameFiles.GameRoot
{
    public class CustomNetworkRoomManager : NetworkRoomManager
    {
        public event Action<bool> OnServerSceneLoadStateChanged;
        public event Action<bool> OnClientSceneLoadStateChanged;
        
        
        [SerializeField] private bool useRandomSeed = true;
        [SerializeField] private int customSeed = 12345;
        
        public override void OnServerChangeScene(string newSceneName)
        {
            base.OnServerChangeScene(newSceneName);

            Debug.Log($"[CNRM] OnServerChangeScene new - {newSceneName}");

            OnServerSceneLoadStateChanged?.Invoke(true);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            if (sceneName == GameplayScene)
            {
                GenerateLevelDeterministic("Server");
            }
            
            base.OnServerSceneChanged(sceneName);
        
            Debug.Log($"[CNRM] OnServerSceneChanged {sceneName}");

            OnServerSceneLoadStateChanged?.Invoke(false);
        }

        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
        {
            base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);

            if (NetworkServer.active) return;

            Debug.Log("[CNRM] OnClientChangeScene");

            OnClientSceneLoadStateChanged?.Invoke(true);
        }

        public override void OnClientSceneChanged()
        {
            
            base.OnClientSceneChanged();

            if (NetworkServer.active) return;
            
            var sceneName = SceneManager.GetActiveScene().path;
            if (sceneName == GameplayScene)
            {
                Debug.Log($"[CNRM]<color=green> ВЫзов генерации на клиенте, попытка");
                GenerateLevelDeterministic("Client");
            }
            else
            {
                Debug.Log($"[CNRM]<color=red> sceneName: {sceneName}, need: {GameplayScene}");
            }
            
            Debug.Log("[CNRM] OnClientSceneChanged");
            
            OnClientSceneLoadStateChanged?.Invoke(false);
        }
        
        private void GenerateLevelDeterministic(string who)
        {
            var orchestrator = FindObjectOfType<LevelOrchestrator>();
            if (orchestrator == null)
            {
                Debug.LogError($"[CNRM] ({who}) LevelOrchestrator не найден на сцене!");
                return;
            }

            int activeSeed = useRandomSeed ? UnityEngine.Random.Range(int.MinValue, int.MaxValue) : customSeed;
 
            var generator = new LevelGenerator(new LevelGraphConfig(), activeSeed);
            var clusters = generator.GenerateClusters();
 
            if (clusters is { Count: > 0 })
            {
                orchestrator.GeneratePhysicalLevel(clusters, activeSeed);
                Debug.Log($"[CNRM] ({who}) Уровень сгенерирован, seed={activeSeed}, комнат в кластерах={clusters.Count}.");
            }
            else
            {
                Debug.LogError($"[CNRM] ({who}) Не удалось сгенерировать кластеры для уровня.");
            }
        }

    }
}