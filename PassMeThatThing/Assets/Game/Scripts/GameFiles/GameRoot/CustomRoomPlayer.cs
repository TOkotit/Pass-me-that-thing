using System;
using System.Collections;
using Assets.Game.Scripts.GameFiles.GameRoot;
using DI;
using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Root
{
    public class CustomRoomPlayer : NetworkRoomPlayer
    {
        [Header("Steam things")]
        [SerializeField] private bool isSteam;
        
        [Inject] private RoomViewHandler _viewHandler;
        [Inject] private ConnectedPlayers _players;



        private void Awake()
        {
            var scope = LifetimeScope.Find<LobbyScope>();
            scope.Container.Inject(this);

            _viewHandler.LocalReadyStateChanged += SetReady;
        }

        private void OnDestroy()
        {
            _viewHandler.LocalReadyStateChanged -= SetReady;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            _players.players.Add(this);
            if (isLocalPlayer)
                _players.localPlayer = this;

            _players.PlayersViewDataChanged();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            _players.players.Remove(this);
            _players.PlayersViewDataChanged();
        }

        #region Steam Name and Image
        
        [SyncVar(hook = nameof(OnSteamIdChanged))]
        private ulong steamId;

        [SyncVar]
        public string nameText;
        [SyncVar]
        public Texture2D avatarImage;

        public override void OnStartLocalPlayer()
        {
            if (!isSteam) return;
            
            CmdSetSteamId(SteamUser.GetSteamID().m_SteamID);
        }

        [Command]
        private void CmdSetSteamId(ulong newSteamId)
        {
            steamId = newSteamId;
        }
        
        private void OnSteamIdChanged(ulong oldId, ulong newId)
        {
            if (!isSteam) return;
            
            var cSteamId = new CSteamID(newId);

            var personaName = SteamFriends.GetFriendPersonaName(cSteamId);
            if (nameText != null)
            {
                nameText = personaName;
            }

            StartCoroutine(LoadSteamAvatar(cSteamId));
        }

        private IEnumerator LoadSteamAvatar(CSteamID cSteamId)
        {
            Debug.Log("LoadSteamAvatar");
            int imageId = SteamFriends.GetSmallFriendAvatar(cSteamId);

            // Если аватар ещё не загружен в кэш Steam ждем его готовности
            while (imageId == -1)
            {
                yield return null;
                imageId = SteamFriends.GetSmallFriendAvatar(cSteamId);
            }

            if (imageId > 0)
            {
                var avatarTexture = GetSteamAvatarAsTexture(imageId);
                Debug.Log($"avatarTexture {avatarTexture == null}");
                if (avatarTexture != null)
                {
                    avatarImage = avatarTexture;
                }
            }

            _players.PlayersViewDataChanged();
        }
        
        private Texture2D GetSteamAvatarAsTexture(int imageId)
        {
            Debug.Log("GetSteamAvatarAsTexture");
            uint width, height;
            if (!SteamUtils.GetImageSize(imageId, out width, out height))
                return null;

            var imageBuffer = new byte[width * height * 4];
            if (!SteamUtils.GetImageRGBA(imageId, imageBuffer, imageBuffer.Length))
                return null;

            // создаем текстуру и переворачиваем её по вертикали тк Steam отдаёт байты снизу вверх
            var texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
            var pixels = new Color32[width * height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int srcIndex = (int)((y * width + x) * 4);
                    int dstIndex = (int)(((height - 1 - y) * width + x));
                    pixels[dstIndex] = new Color32(
                        imageBuffer[srcIndex],
                        imageBuffer[srcIndex + 1],
                        imageBuffer[srcIndex + 2],
                        imageBuffer[srcIndex + 3]
                    );
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }
        
        #endregion
        
        public override void OnClientEnterRoom() {}

        public override void OnClientExitRoom() {}

        public override void IndexChanged(int oldIndex, int newIndex)
        {
            _players.PlayersViewDataChanged();
        }

        public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
        {
            _players.PlayersViewDataChanged();
        }

        public void SetReady(bool readyState)
        {
            if (isLocalPlayer)
                CmdChangeReadyState(readyState);
        }
    }
}