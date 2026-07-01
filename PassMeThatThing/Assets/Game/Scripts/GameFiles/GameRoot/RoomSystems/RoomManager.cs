
using FishNet.Object;
using System.Collections.Generic;
using FishNet.Managing.Scened;
using UnityEngine;

public class RoomManager : NetworkBehaviour
{
    // public static RoomManager Instance { get; private set; }
    //
    // [SerializeField] private string gameSceneName = "GameScene";
    //
    // public List<NetworkRoomPlayer> Players { get; private set; } = new List<NetworkRoomPlayer>();
    //
    // private void Awake()
    // {
    //     Instance = this;
    // }
    //
    // public void RegisterPlayer(NetworkRoomPlayer player)
    // {
    //     if (!Players.Contains(player)) Players.Add(player);
    //     RefreshGUI();
    // }
    //
    // public void UnregisterPlayer(NetworkRoomPlayer player)
    // {
    //     if (Players.Contains(player)) Players.Remove(player);
    //     RefreshGUI();
    // }
    //
    // public void RefreshGUI()
    // {
    //     // Находим GUI скрипт и обновляем его
    //     FindObjectOfType<RoomGUI>()?.UpdateLobbyUI();
    // }
    //
    // // Проверка на сервере: если все готовы — запускаем игру
    // [Server]
    // public void CheckStartCondition()
    // {
    //     if (Players.Count == 0) return;
    //
    //     foreach (var player in Players)
    //     {
    //         if (!player.IsReady) return; // Кто-то еще не готов
    //     }
    //
    //     // Если все готовы, загружаем сцену для всех игроков
    //     StartGame();
    // }
    //
    // [Server]
    // private void StartGame()
    // {
    //     SceneLoadData sld = new SceneLoadData(gameSceneName);
    //     // FishNet автоматически переместит всех подключенных игроков на новую сцену
    //     NetworkManager.SceneManager.LoadGlobalScenes(sld);
    // }
}