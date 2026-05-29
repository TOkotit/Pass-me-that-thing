using Mirror;
using UnityEngine;

namespace Game.Scripts.GameFiles.Events
{
    public class PlayerMinigameHandler : NetworkBehaviour
    {
        
        [Command(requiresAuthority = false)]
        private void CmdWriteConnToItem(BaseGameEvent activator)
        {
            if (activator == null) return;

            // Передаем управление активатору и говорим, какая связь (connectionToClient) это вызвала
            activator.ServerActivateMinigame(connectionToClient);
        }

        // 3. ОТВЕТ ОТ СЕРВЕРА: Выполняется ТОЛЬКО на клиенте, который вызвал команду
        // Префикс "Target" в названии метода обязателен для Mirror!
        [TargetRpc]
        public void TargetOpenMinigame(MinigameParameters parameters)
        {
            Debug.Log($"[Client] Открываю миниигру: {parameters.eventType} с ограничением {parameters.timeLimit} сек.");
        
            // Здесь ваш локальный UI-код:
            // MinigameUI.Instance.Open(parameters);
        }
    }
}