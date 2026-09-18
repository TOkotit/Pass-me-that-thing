using Mirror;
using Steamworks;
using UnityEngine;
using VContainer;

namespace Root
{
    public class SteamLobbyManager : MonoBehaviour
    {
        private const string HostAddressKey = "HostSteamID";
        [Inject] private NetworkManager _networkManager;

        public CSteamID CurrentLobbyID { get; private set; }

        private Callback<LobbyCreated_t> _lobbyCreated;
        private Callback<GameLobbyJoinRequested_t> _joinRequested;
        private Callback<LobbyEnter_t> _lobbyEntered;

        private void Start()
        {
            //_networkManager = NetworkManager.singleton;

            if (!SteamManager.Initialized)
            {
                Debug.LogError("[STEAM] Стим не инициализирован! Скрипт лобби работать не будет.");
                return;
            }

            _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            _joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
            _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

            Debug.Log(
                $"<color=orange>[STEAM] Менеджер лобби запущен. Мой SteamID:</color> <color=green>{SteamUser.GetSteamID().m_SteamID}</color>");
        }

        // ==========================================
        // ЛОГИКА ХОСТА (Создание комнаты)
        // ==========================================

        // ЭТОТ МЕТОД вызывай из своего UI (кнопка "Создать игру") вместо _networkManager.StartHost()
        public void CreateSteamLobby()
        {
            Debug.Log("[STEAM] Отправляем запрос на создание лобби...");
            // Создаем лобби только для друзей (k_ELobbyTypeFriendsOnly) с лимитом мест из Mirror
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, _networkManager.maxConnections);
        }

        public void OpenFriends()
        {
            if (SteamManager.Initialized)
            {
                SteamFriends.ActivateGameOverlay("Friends");
            }
        }

        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                Debug.LogError("[STEAM] Ошибка создания лобби на серверах Стим.");
                return;
            }

            // Запоминаем ID созданного лобби
            CurrentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);

            Debug.Log(
                $"<color=green>[STEAM] Лобби успешно создано в Стиме. ID лобби: {CurrentLobbyID.m_SteamID}</color>");

            _networkManager.StartHost();

            string mySteamID = SteamUser.GetSteamID().ToString();
            SteamMatchmaking.SetLobbyData(CurrentLobbyID, HostAddressKey, mySteamID);
        }

        // ==========================================
        // ЛОГИКА КЛИЕНТА (Подключение друга)
        // ==========================================

        // Срабатывает у друга, когда он в Стиме жмет кнопку "Присоединиться к игре"
        private void OnJoinRequested(GameLobbyJoinRequested_t callback)
        {
            Debug.Log("[STEAM] Получен инвайт/запрос на вход. Заходим в Стим-лобби...");
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        // Срабатывает у друга когда Стим успешно закинул его внутрь твоей Стим-комнаты
        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            CurrentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);

            if (NetworkServer.active) return;

            string hostSteamID = SteamMatchmaking.GetLobbyData(CurrentLobbyID, HostAddressKey);

            Debug.Log($"[STEAM] Успешно вошли в лобби. Подключаемся к Mirror-хосту по SteamID: {hostSteamID}");

            _networkManager.networkAddress = hostSteamID;
            _networkManager.StartClient();
        }

        public void LeaveLobby()
        {
            if (CurrentLobbyID.m_SteamID != 0)
            {
                SteamMatchmaking.LeaveLobby(CurrentLobbyID);
                CurrentLobbyID = new CSteamID(0);
                Debug.Log("[STEAM] Вы вышли из Стим-лобби.");
            }

            //// Останавливаем сетевую сессию Mirror
            //if (NetworkServer.active && NetworkClient.isConnected)
            //{
            //    // Если мы хост — останавливаем и сервер, и клиента
            //    _networkManager.StopHost();
            //}
            //else if (NetworkClient.isConnected)
            //{
            //    // Если мы простой клиент — отключаемся от хоста
            //    _networkManager.StopClient();
            //}
        }

        private void OnDestroy()
        {
            LeaveLobby();

            _lobbyCreated?.Dispose();
            _joinRequested?.Dispose();
            _lobbyEntered?.Dispose();
        }
    }
}