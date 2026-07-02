using System;
using FishNet;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomGUI : MonoBehaviour
{
    private string playerNameInput = "Player";

    private void Awake()
    {
        playerNameInput +=  "_" + Random.Range(100, 999);
    }

    private void OnGUI()
    {
        // Если мы еще не подключены (главное меню)
        if (!InstanceFinder.NetworkManager.IsServerStarted && !InstanceFinder.NetworkManager.IsClientStarted)
        {
            
        }
        else // Мы в лобби
        {
            DrawLobbyMenu();
        }
    }
    

    
    private void DrawLobbyMenu()
    {
        GUILayout.BeginArea(new Rect(20, 20, 300, 400));
        GUILayout.Label("=== ЛОББИ ===");
    
        // Выводим список игроков
        if (CustomRoomManager.Instance != null)
        {
            foreach (var player in CustomRoomManager.Instance.RoomPlayers)
            {
                string readyStatus = player.IsReady.Value ? "[ГОТОВ]" : "[НЕ ГОТОВ]";
                GUILayout.Label($"{player.PlayerName} — {readyStatus}");
            }
    
            GUILayout.Space(20);
    
            // Ищем локального игрока, чтобы дать ему управлять своей готовностью
            NetworkRoomPlayer localPlayer = GetLocalRoomPlayer();
            if (localPlayer != null)
            {
                string btnText = localPlayer.IsReady.Value ? "Снять готовность" : "Готов!";
                if (GUILayout.Button(btnText))
                {
                    localPlayer.CmdSetReady(!localPlayer.IsReady.Value);
                }
            }
        }
    
        if (GUILayout.Button("Выйти"))
        {
            InstanceFinder.NetworkManager.TransportManager.Transport.StopConnection(true);
            InstanceFinder.NetworkManager.TransportManager.Transport.StopConnection(false);
        }
    
        GUILayout.EndArea();
    }
    
    private void SetInitialName()
    {
        NetworkRoomPlayer localPlayer = GetLocalRoomPlayer();
        if (localPlayer != null)
        {
            localPlayer.CmdSetName(playerNameInput);
        }
    }
    
    private NetworkRoomPlayer GetLocalRoomPlayer()
    {
        foreach (var player in CustomRoomManager.Instance.RoomPlayers)
        {
            if (player.IsOwner) return player;
        }
        return null;
    }
    
    public void UpdateLobbyUI()
    {
        // Вызывается для принудительного перерисовывания (в OnGUI это происходит автоматически, 
        // но метод нужен для связки со SyncVar)
    }
}