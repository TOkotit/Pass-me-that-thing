using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Events
{
    public class PlayerMinigameHandler : NetworkBehaviour
    {
        [TargetRpc]
        public void TargetOpenMinigame(MinigameParameters parameters)
        {
            Debug.Log($"[Client] minigame {parameters.eventType} с ограничением {parameters.timeLimit} сек.");
            
            // локальный UI-код:

        }
    }
}