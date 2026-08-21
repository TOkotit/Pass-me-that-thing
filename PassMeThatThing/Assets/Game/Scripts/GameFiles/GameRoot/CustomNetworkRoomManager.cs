using Mirror;
using System;
using Game.Scripts.GameFiles.LevelGeneration;
using Game.Scripts.GameFiles.LevelGeneration.Graph;
using UnityEditor;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.GameRoot
{
    public class CustomNetworkRoomManager : NetworkRoomManager
    {
        public event Action<bool> OnServerSceneLoadStateChanged;
        public event Action<bool> OnClientSceneLoadStateChanged;
        [SerializeField] private int defaultTestSeed = 12345;
        
        public override void OnServerChangeScene(string newSceneName)
        {
            base.OnServerChangeScene(newSceneName);

            Debug.Log($"[CNRM] OnServerChangeScene new - {newSceneName}");

            OnServerSceneLoadStateChanged?.Invoke(true);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            /*
            if (sceneName == GameplayScene)
            {
                var orchestrator = FindObjectOfType<LevelOrchestrator>();
                if (orchestrator != null)
                {
                    var generator = new LevelGenerator(new LevelGraphConfig(), defaultTestSeed);
                    var clusters = generator.GenerateClusters();
                    if (clusters != null && clusters.Count > 0)
                    {
                        orchestrator.GeneratePhysicalLevel(clusters, defaultTestSeed);
                    }
                    else
                    {
                        Debug.LogError("[CNRM] Не удалось сгенерировать кластеры для уровня.");
                    }
                }
                else
                {
                    Debug.LogError("[CNRM] LevelOrchestrator не найден на сцене!");
                }
            }
            */
            
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

            Debug.Log("[CNRM] OnClientSceneChanged");
            
            OnClientSceneLoadStateChanged?.Invoke(false);
        }
    }
}