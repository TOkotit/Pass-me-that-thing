using FishNet;
using UnityEngine;

public class RoomGUI : MonoBehaviour
{
    // private string playerNameInput = "Player_" + Random.Range(100, 999);
    //
    // private void OnGUI()
    // {
    //     // Если мы еще не подключены (главное меню)
    //     if (!InstanceFinder.NetworkManager.IsServerStarted && !InstanceFinder.NetworkManager.IsClientStarted)
    //     {
    //         DrawMainMenu();
    //     }
    //     else // Мы в лобби
    //     {
    //         DrawLobbyMenu();
    //     }
    // }
    //
    // private void DrawMainMenu()
    // {
    //     GUILayout.BeginArea(new Rect(20, 20, 250, 200));
    //     GUILayout.Label("Имя игрока:");
    //     playerNameInput = GUILayout.TextField(playerNameInput, 15);
    //
    //     if (GUILayout.Button("Создать сервер (Host)"))
    //     {
    //         InstanceFinder.ServerManager.StartConnection();
    //         InstanceFinder.ClientManager.StartConnection();
    //         Invoke(nameof(SetInitialName), 0.2f); // Небольшая задержка, чтобы объект игрока успел заспавниться
    //     }
    //
    //     if (GUILayout.Button("Подключиться (Client)"))
    //     {
    //         InstanceFinder.ClientManager.StartConnection();
    //         Invoke(nameof(SetInitialName), 0.2f);
    //     }
    //     GUILayout.EndArea();
    // }
    //
    // private void DrawLobbyMenu()
    // {
    //     GUILayout.BeginArea(new Rect(20, 20, 300, 400));
    //     GUILayout.Label("=== ЛОББИ ===");
    //
    //     // Выводим список игроков
    //     if (RoomManager.Instance != null)
    //     {
    //         foreach (var player in RoomManager.Instance.Players)
    //         {
    //             string readyStatus = player.IsReady ? "[ГОТОВ]" : "[НЕ ГОТОВ]";
    //             GUILayout.Label($"{player.PlayerName} — {readyStatus}");
    //         }
    //
    //         GUILayout.Space(20);
    //
    //         // Ищем локального игрока, чтобы дать ему управлять своей готовностью
    //         NetworkRoomPlayer localPlayer = GetLocalRoomPlayer();
    //         if (localPlayer != null)
    //         {
    //             string btnText = localPlayer.IsReady ? "Снять готовность" : "Готов!";
    //             if (GUILayout.Button(btnText))
    //             {
    //                 localPlayer.CmdSetReady(!localPlayer.IsReady);
    //             }
    //         }
    //     }
    //
    //     if (GUILayout.Button("Выйти"))
    //     {
    //         InstanceFinder.NetworkManager.TransportManager.Transport.StopConnection(true);
    //         InstanceFinder.NetworkManager.TransportManager.Transport.StopConnection(false);
    //     }
    //
    //     GUILayout.EndArea();
    // }
    //
    // private void SetInitialName()
    // {
    //     NetworkRoomPlayer localPlayer = GetLocalRoomPlayer();
    //     if (localPlayer != null)
    //     {
    //         localPlayer.CmdSetName(playerNameInput);
    //     }
    // }
    //
    // private NetworkRoomPlayer GetLocalRoomPlayer()
    // {
    //     foreach (var player in RoomManager.Instance.Players)
    //     {
    //         if (player.IsOwner) return player;
    //     }
    //     return null;
    // }
    //
    // public void UpdateLobbyUI()
    // {
    //     // Вызывается для принудительного перерисовывания (в OnGUI это происходит автоматически, 
    //     // но метод нужен для связки со SyncVar)
    // }
}