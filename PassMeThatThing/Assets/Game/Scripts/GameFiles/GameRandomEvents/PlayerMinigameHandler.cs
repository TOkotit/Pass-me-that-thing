using FishNet.Object;
using FishNet.Object.Synchronizing;
using Game.Gameplay.View.UI;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Events
{
    public class PlayerMinigameHandler : NetworkBehaviour
    {
        [Inject] private GameplayUIManager  gameplayUIManager;

        [SyncVar]
        private bool _isClientBusy;
        
        public bool IsClientBusy
        {
            get => _isClientBusy;
            set => _isClientBusy = value;
        }

        [TargetRpc]
        public void TargetOpenMinigame(MinigameParameters parameters)
        {
            Debug.Log($"[Client] minigame {parameters.eventType} IsClientBusy = true");
            IsClientBusy = true;
            gameplayUIManager.OpenScreenMinigame(parameters);
        }
        
        [TargetRpc]
        public void TargetCloseMinigame()
        {
            Debug.Log($"[Client] IsClientBusy = false ");
            IsClientBusy = false;
        }

    }
}