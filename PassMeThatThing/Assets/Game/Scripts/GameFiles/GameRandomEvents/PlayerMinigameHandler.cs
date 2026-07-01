using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Game.Gameplay.View.UI;

using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Events
{
    public class PlayerMinigameHandler : NetworkBehaviour
    {
        [Inject] private GameplayUIManager  gameplayUIManager;

        
        private readonly SyncVar<bool> _isClientBusy = new();
        
        public bool IsClientBusy
        {
            get => _isClientBusy.Value;
            set => _isClientBusy.Value = value;
        }

        [TargetRpc]
        public void TargetOpenMinigame(NetworkConnection conn, MinigameParameters parameters)
        {
            Debug.Log($"[Client] minigame {parameters.eventType} IsClientBusy = true");
            IsClientBusy = true;
            gameplayUIManager.OpenScreenMinigame(parameters);
        }
        
        [TargetRpc]
        public void TargetCloseMinigame(NetworkConnection conn)
        {
            Debug.Log($"[Client] IsClientBusy = false ");
            IsClientBusy = false;
        }

    }
}