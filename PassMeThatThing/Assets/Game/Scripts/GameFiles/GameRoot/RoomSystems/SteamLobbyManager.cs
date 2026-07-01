// using FishNet.Managing;
// using Mirror;
// using Steamworks;
// using UnityEngine;
//
// namespace Root
// {
//     public class SteamLobbyManager : MonoBehaviour
//     {
//         private const string HostAddressKey = "HostSteamID";
//         private NetworkManager _networkManager;
//
//         private Callback<LobbyCreated_t> _lobbyCreated;
//         private Callback<GameLobbyJoinRequested_t> _joinRequested;
//         private Callback<LobbyEnter_t> _lobbyEntered;
//
//         private void Start()
//         {
//             _networkManager = NetworkManager.singleton;
//
//             if (!SteamManager.Initialized)
//             {
//                 Debug.LogError("[STEAM] Стим не инициализирован! Скрипт лобби работать не будет.");
//                 return;
//             }
//
//             _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
//             _joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
//             _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
//
//             Debug.Log(
//                 $"<color=orange>[STEAM] Менеджер лобби запущен. Мой SteamID:</color> <color=green>{SteamUser.GetSteamID().m_SteamID}</color>");
//         }
//
//         // ==========================================
//         // ЛОГИКА ХОСТА (Создание комнаты)
//         // ==========================================
//
//         // ЭТОТ МЕТОД вызывай из своего UI (кнопка "Создать игру") вместо _networkManager.StartHost()
//         public void CreateSteamLobby()
//         {
//             Debug.Log("[STEAM] Отправляем запрос на создание лобби...");
//             // Создаем лобби только для друзей (k_ELobbyTypeFriendsOnly) с лимитом мест из Mirror
//             SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, _networkManager.maxConnections);
//         }
//
//         private void OnLobbyCreated(LobbyCreated_t callback)
//         {
//             if (callback.m_eResult != EResult.k_EResultOK)
//             {
//                 Debug.LogError("[STEAM] Ошибка создания лобби на серверах Стим.");
//                 return;
//             }
//
//             Debug.Log(
//                 $"<color=green>[STEAM] Лобби успешно создано в Стиме. ID лобби: {callback.m_ulSteamIDLobby}</color>");
//
//             // 1. Запускаем локальный Mirror-хост (сервер + клиент)
//             _networkManager.StartHost();
//
//             // 2. Записываем наш личный SteamID64 в метаданные Стим-лобби
//             CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
//             string mySteamID = SteamUser.GetSteamID().ToString();
//             SteamMatchmaking.SetLobbyData(lobbyID, HostAddressKey, mySteamID);
//         }
//
//         // ==========================================
//         // ЛОГИКА КЛИЕНТА (Подключение друга)
//         // ==========================================
//
//         // Срабатывает у друга, когда он в Стиме жмет кнопку "Присоединиться к игре"
//         private void OnJoinRequested(GameLobbyJoinRequested_t callback)
//         {
//             Debug.Log("[STEAM] Получен инвайт/запрос на вход. Заходим в Стим-лобби...");
//             SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
//         }
//
//         // Срабатывает у друга, когда Стим успешно закинул его внутрь твоей Стим-комнаты
//         private void OnLobbyEntered(LobbyEnter_t callback)
//         {
//             // Если мы сами являемся Хостом — игнорируем, мы уже в игре
//             if (NetworkServer.active) return;
//
//             CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
//
//             // Достаем из Стим-лобби зашитый туда SteamID64 создателя (Хоста)
//             string hostSteamID = SteamMatchmaking.GetLobbyData(lobbyID, HostAddressKey);
//
//             Debug.Log($"[STEAM] Успешно вошли в лобби. Подключаемся к Mirror-хосту по SteamID: {hostSteamID}");
//
//             // Подставляем этот ID в Mirror в качестве адреса и запускаем подключение клиента
//             _networkManager.networkAddress = hostSteamID;
//             _networkManager.StartClient();
//         }
//
//         // Отписываемся от событий при уничтожении объекта, чтобы не было утечек памяти
//         private void OnDestroy()
//         {
//             _lobbyCreated?.Dispose();
//             _joinRequested?.Dispose();
//             _lobbyEntered?.Dispose();
//         }
//     }
// }