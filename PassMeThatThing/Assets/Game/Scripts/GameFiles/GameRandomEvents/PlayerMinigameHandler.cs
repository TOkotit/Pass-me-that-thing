using Game.Gameplay.View.UI;
using Mirror;
using UnityEngine;
using VContainer;

namespace Game.Scripts.GameFiles.Events
{
    public class PlayerMinigameHandler : NetworkBehaviour
    {
        [Inject] private GameplayUIManager  gameplayUIManager;
        
        [TargetRpc]
        public void TargetOpenMinigame(MinigameParameters parameters)
        {
            Debug.Log($"[Client] minigame {parameters.eventType} с ограничением {parameters.timeLimit} сек.");

            gameplayUIManager.OpenScreenMinigame(parameters);
        }
    }
}