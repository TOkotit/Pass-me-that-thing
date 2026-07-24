using System;
using System.Collections;
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
            _viewHandler.players.Add(this);
            _viewHandler.PlayersViewDataChanged();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            _viewHandler.players.Remove(this);
            _viewHandler.PlayersViewDataChanged();
        }

        #region Steam Name and Image
        
        [SyncVar(hook = nameof(OnSteamIdChanged))]
        private ulong steamId;
        
        public string nameText;
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
            int imageId = SteamFriends.GetLargeFriendAvatar(cSteamId);

            // Если аватар ещё не загружен в кэш Steam ждем его готовности
            while (imageId == -1)
            {
                yield return null;
                imageId = SteamFriends.GetLargeFriendAvatar(cSteamId);
            }

            if (imageId > 0)
            {
                var avatarTexture = GetSteamAvatarAsTexture(imageId);
                if (avatarImage != null && avatarTexture != null)
                {
                    avatarImage = avatarTexture;
                }
            }
        }
        
        private Texture2D GetSteamAvatarAsTexture(int imageId)
        {
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
            _viewHandler.PlayersViewDataChanged();
        }

        public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
        {
            _viewHandler.PlayersViewDataChanged();
        }

        public void SetReady(bool readyState)
        {
            if (isLocalPlayer)
                CmdChangeReadyState(readyState);
        }
    }
}