using Mirror;
using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Game.Scripts.GameFiles.GameRoot
{
    public class CustomNetworkRoomManager : NetworkRoomManager
    {
        public event Action<bool> OnServerSceneLoadStateChanged;
        public event Action<bool> OnClientSceneLoadStateChanged;

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